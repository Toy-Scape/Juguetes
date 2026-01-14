using System.Collections.Generic;
using InteractionSystem.Interfaces;
using Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InteractionSystem.Core
{
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private float interactionDistance = 3f;
        [SerializeField] private Transform[] rayOrigins = new Transform[0];

        [Header("Outline Settings")]
        [SerializeField] private Color outlineColor = Color.yellow;
        [SerializeField, Range(0f, 10f)] private float outlineWidth = 2f;
        [SerializeField] private Outline.Mode outlineMode = Outline.Mode.OutlineAll;

        [Header("UI Prompt")]
        [SerializeField] private GameObject interactionPrompt = null;
        [SerializeField] private TMP_Text interactionPromptText = null;
        [SerializeField] private InputActionReference interactActionReference = null;
        [SerializeField] private string fallbackPrompt = "E";

        [Header("Feedback")]
        [SerializeField] private GamepadVibration vibration;
        [SerializeField] private PlayerConfig config;

        [Header("Prompt Positioning")]
        [SerializeField] private Vector3 promptWorldOffset = new Vector3(0f, 1.5f, 0f);
        //[SerializeField, Range(0.01f, 20f)] private float promptFollowSpeed = 10f;

        private IInteractable focusedInteractable;
        private GameObject focusedGameObject;
        private Outline focusedOutline;
        private bool focusedIsUsable;
        private Dictionary<int, IInteractable> interactableCache = new Dictionary<int, IInteractable>();
        private string promptBindingDisplay = "E";
        private RectTransform promptRectTransform;
        private Vector3 promptTargetWorldPosition;
        private bool promptHasTarget;
        private PlayerInventory playerInventory;

        public float InteractionDistance => interactionDistance;

        public Transform[] RayOrigins
        {
            get
            {
                if (rayOrigins != null && rayOrigins.Length > 0)
                {
                    return rayOrigins;
                }

                return new Transform[] { transform };
            }
        }

        void Awake()
        {
            if (rayOrigins == null || rayOrigins.Length == 0)
            {
                var mainCam = Camera.main;
                if (mainCam != null)
                {
                    rayOrigins = new Transform[] { mainCam.transform };
                }
                else
                {
                    rayOrigins = new Transform[] { transform };
                }
            }

            InteractableBase.SetGlobalOutlineProperties(outlineColor, outlineWidth, outlineMode);

            if (interactActionReference != null && interactActionReference.action != null)
            {
                promptBindingDisplay = interactActionReference.action.GetBindingDisplayString();
            }
            else
            {
                promptBindingDisplay = fallbackPrompt;
            }

            if (interactionPrompt != null)
            {
                // No desactivar el canvas, solo controlar el texto
                promptRectTransform = interactionPrompt.GetComponent<RectTransform>();
                if (interactionPromptText != null)
                {
                    interactionPromptText.text = "";
                    interactionPromptText.enabled = false;
                }
            }

            playerInventory = GetComponent<PlayerInventory>();
            if (playerInventory == null)
            {
                Debug.LogWarning("PlayerInteractor: PlayerInventory not found on the same GameObject.");
            }

            // Auto-wire feedback dependencies
            if (vibration == null) vibration = GetComponent<GamepadVibration>();
            if (vibration == null) vibration = GetComponentInChildren<GamepadVibration>();

            if (config == null)
            {
                var pc = GetComponent<PlayerController>();
                if (pc != null) 
                {
                    config = pc.Config;
                }
                else
                {
                    var agPc = GetComponent<Assets.Scripts.AntiGravityController.AntiGravityPlayerController>();
                    if (agPc != null) config = agPc.Config;
                }
            }
        }

        void OnValidate()
        {
            InteractableBase.SetGlobalOutlineProperties(outlineColor, outlineWidth, outlineMode);

            if (interactActionReference != null && interactActionReference.action != null)
            {
                promptBindingDisplay = interactActionReference.action.GetBindingDisplayString();
            }
        }

        void Update()
        {
            // Check if the focused object was destroyed externally
            if (focusedInteractable != null && focusedGameObject == null)
            {
                ClearOutline();
            }

            UpdatePromptPosition();
            CheckForInteractable();
        }

        public void OnInteract()
        {
            if (focusedInteractable == null)
            {
                Debug.Log("OnInteract called but no focused interactable");
                return;
            }

            if (!focusedIsUsable)
            {
                Debug.Log("OnInteract blocked: focused interactable not usable");
                return;
            }

            Debug.Log("OnInteract performed on " + focusedGameObject.name);
            InteractContext context = new InteractContext
            {
                PlayerInventory = playerInventory
            };
            InvokeInteractionVibration();
            focusedInteractable.Interact(context);
        }

        private void InvokeInteractionVibration()
        {
            if (vibration != null && config != null)
                vibration.Vibrate(config.InteractVibration.x, config.InteractVibration.y, config.InteractVibration.z);
        }

        private void CheckForInteractable()
        {
            RaycastHit closestHitInfo = default;
            bool hasHit = false;
            float closestDistance = float.MaxValue;
            Transform[] origins = RayOrigins;

            for (int i = 0; i < origins.Length; i++)
            {
                Transform origin = origins[i] != null ? origins[i] : transform;
                Ray ray = new Ray(origin.position, origin.forward);

                if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
                {
                    if (hit.distance < closestDistance)
                    {
                        closestDistance = hit.distance;
                        closestHitInfo = hit;
                        hasHit = true;
                    }
                }
            }

            if (hasHit)
            {
                GameObject hitObject = closestHitInfo.collider.gameObject;
                int id = hitObject.GetInstanceID();

                if (!interactableCache.TryGetValue(id, out IInteractable interactable))
                {
                    interactable = FindInteractableInParents(hitObject);
                    interactableCache[id] = interactable;
                    Debug.Log($"Interactable cacheado: {hitObject.name} ({id}) => {interactable}");
                }

                if (interactable != null)
                {
                    bool isUsable = interactable.IsInteractable();

                    if (focusedGameObject != hitObject)
                    {
                        Debug.Log($"Nuevo interactable enfocado: {hitObject.name} (usable: {isUsable})");
                        ClearOutline();
                        focusedInteractable = interactable;
                        focusedGameObject = hitObject;
                        focusedIsUsable = isUsable;
                        ApplyOutline(focusedGameObject);
                    }
                    else
                    {
                        focusedIsUsable = isUsable;
                    }

                    // SetPromptTargetFromHit(closestHitInfo); // Removed to keep prompt fixed on object center
                    return;
                }
                else
                {
                    //Debug.Log($"No IInteractable en: {hitObject.name}");
                }
            }

            if (focusedGameObject != null)
            {
                Debug.Log("Interactable enfocado perdido, limpiando outline.");
                ClearOutline();
            }
        }

        private IInteractable FindInteractableInParents(GameObject start)
        {
            Transform t = start.transform;

            while (t != null)
            {
                if (t.TryGetComponent<IInteractable>(out var interactable))
                {
                    return interactable;
                }

                t = t.parent;
            }

            return null;
        }

        private void SetPromptTargetFromHit(RaycastHit hit)
        {
            promptHasTarget = true;
            promptTargetWorldPosition = hit.point + promptWorldOffset;
            if (interactionPrompt != null) interactionPrompt.transform.position = promptTargetWorldPosition;
        }

        private void SetPromptTargetFromGameObject(GameObject go)
        {
            promptHasTarget = true;

            Collider c = go.GetComponent<Collider>();

            if (c != null)
            {
                promptTargetWorldPosition = c.bounds.center + promptWorldOffset;
                if (interactionPrompt != null) interactionPrompt.transform.position = promptTargetWorldPosition;
                return;
            }

            promptTargetWorldPosition = go.transform.position + promptWorldOffset;
            if (interactionPrompt != null) interactionPrompt.transform.position = promptTargetWorldPosition;
        }

        private void SetPromptTargetToPlayer()
        {
            promptHasTarget = true;
            Transform followTarget = (rayOrigins != null && rayOrigins.Length > 0) ? rayOrigins[0] : transform;
            promptTargetWorldPosition = followTarget.position + promptWorldOffset;
            if (interactionPrompt != null) interactionPrompt.transform.position = promptTargetWorldPosition;
        }

        private void ApplyOutline(GameObject target)
        {
            if (target == null)
                return;

            focusedOutline = target.GetComponent<Outline>();

            if (focusedOutline == null)
            {
                Debug.LogWarning($"El objeto {target.name} no tiene componente Outline. Añádelo manualmente para personalizar el borde desde el inspector.");
                return;
            }

            focusedOutline.enabled = true;
            ShowPrompt(true, promptBindingDisplay);
            SetPromptTargetFromGameObject(target);
        }

        private void ClearOutline()
        {
            if (focusedOutline != null)
            {
                focusedOutline.enabled = false;
                focusedOutline = null;
            }

            focusedInteractable = null;
            focusedGameObject = null;
            focusedIsUsable = false;
            ShowPrompt(false, "");
            promptHasTarget = false;
        }

        private void ShowPrompt(bool show, string bindingText)
        {
            if (interactionPrompt == null)
                return;

            if (interactionPromptText != null)
            {
                interactionPromptText.text = show ? $"Press {bindingText}" : "";
                interactionPromptText.enabled = show;
            }

            // Si hay un objeto enfocado, el prompt sigue ese objeto; si no, no hay target
            if (show && focusedGameObject != null)
            {
                SetPromptTargetFromGameObject(focusedGameObject);
            }
            else
            {
                promptHasTarget = false;
            }
        }

        private void UpdatePromptPosition()
        {
            if (interactionPrompt == null || !promptHasTarget)
                return;

            // Position is now updated only once when target is set, so we don't update it here.
            // interactionPrompt.transform.position = Vector3.Lerp(current, targetPosition, Time.deltaTime * promptFollowSpeed);

            Camera cam = Camera.main;
            if (cam == null && rayOrigins != null && rayOrigins.Length > 0)
            {
                cam = rayOrigins[0].GetComponent<Camera>();
            }

            if (cam != null)
            {
                interactionPrompt.transform.LookAt(
                    interactionPrompt.transform.position + cam.transform.rotation * Vector3.forward,
                    cam.transform.rotation * Vector3.up
                );
            }
        }

        private void ShowPrompt(bool show)
        {
            ShowPrompt(show, promptBindingDisplay);
        }
    }
}
