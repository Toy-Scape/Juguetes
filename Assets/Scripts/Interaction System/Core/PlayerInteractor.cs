using System.Collections.Generic;
using InteractionSystem.Interfaces;
using Inventory;
using Localization;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
        [SerializeField] private GameObject interactionPrompt;
        [SerializeField] private TMP_Text interactionPromptText;
        [SerializeField] private string interactionKey = "interaction_prompt";
        [SerializeField] private Image interactionIconImage;
        [SerializeField] private InputPromptIconProvider iconProvider;

        [Header("Feedback")]
        [SerializeField] private GamepadVibration vibration;
        [SerializeField] private PlayerConfig config;

        [Header("Prompt Positioning")]
        [SerializeField] private Vector3 promptWorldOffset = new Vector3(0f, 1.5f, 0f);

        private IInteractable focusedInteractable;
        private GameObject focusedGameObject;
        private Outline focusedOutline;
        private bool focusedIsUsable;
        private Dictionary<int, IInteractable> interactableCache = new();
        private RectTransform promptRectTransform;
        private Vector3 promptTargetWorldPosition;
        private bool promptHasTarget;
        private PlayerInventory playerInventory;
        private PlayerInput playerInput;

        public float InteractionDistance => interactionDistance;

        public Transform[] RayOrigins =>
            rayOrigins != null && rayOrigins.Length > 0
                ? rayOrigins
                : new Transform[] { transform };

        private void Awake()
        {
            if (rayOrigins == null || rayOrigins.Length == 0)
            {
                var cam = Camera.main;
                rayOrigins = cam != null
                    ? new Transform[] { cam.transform }
                    : new Transform[] { transform };
            }

            InteractableBase.SetGlobalOutlineProperties(outlineColor, outlineWidth, outlineMode);

            playerInput = GetComponentInParent<PlayerInput>();
            if (playerInput == null)
                playerInput = FindFirstObjectByType<PlayerInput>();

            if (playerInput != null)
                playerInput.onControlsChanged += OnControlsChanged;

            if (LocalizationManager.Instance != null)
                LocalizationManager.OnLanguageChanged += OnLanguageChanged;

            if (interactionPrompt != null)
                promptRectTransform = interactionPrompt.GetComponent<RectTransform>();

            if (interactionPromptText != null)
            {
                interactionPromptText.text = "";
                interactionPromptText.enabled = false;
            }

            playerInventory = GetComponent<PlayerInventory>();

            if (vibration == null)
                vibration = GetComponentInChildren<GamepadVibration>();

            if (config == null)
            {
                var pc = GetComponent<PlayerController>();
                if (pc != null)
                    config = pc.Config;
            }
        }

        private void OnDestroy()
        {
            if (LocalizationManager.Instance != null)
                LocalizationManager.OnLanguageChanged -= OnLanguageChanged;

            if (playerInput != null)
                playerInput.onControlsChanged -= OnControlsChanged;
        }

        private void Update()
        {
            if (focusedInteractable != null && focusedGameObject == null)
                ClearOutline();

            UpdatePromptPosition();
            CheckForInteractable();
        }

        public void OnInteract()
        {
            if (focusedInteractable == null || !focusedIsUsable)
                return;

            var context = new InteractContext
            {
                PlayerInventory = playerInventory
            };

            if (vibration != null && config != null)
                vibration.Vibrate(
                    config.InteractVibration.x,
                    config.InteractVibration.y,
                    config.InteractVibration.z
                );

            focusedInteractable.Interact(context);
        }

        private void CheckForInteractable()
        {
            RaycastHit closestHit = default;
            bool hasHit = false;
            float closestDistance = float.MaxValue;

            foreach (var origin in RayOrigins)
            {
                if (origin == null) continue;

                if (Physics.Raycast(origin.position, origin.forward, out var hit, interactionDistance))
                {
                    if (hit.distance < closestDistance)
                    {
                        closestDistance = hit.distance;
                        closestHit = hit;
                        hasHit = true;
                    }
                }
            }

            if (!hasHit)
            {
                if (focusedGameObject != null)
                    ClearOutline();
                return;
            }

            var hitObject = closestHit.collider.gameObject;
            int id = hitObject.GetInstanceID();

            if (!interactableCache.TryGetValue(id, out var interactable))
            {
                interactable = FindInteractableInParents(hitObject);
                interactableCache[id] = interactable;
            }

            if (interactable == null)
                return;

            bool isUsable = interactable.IsInteractable();

            if (focusedGameObject != hitObject)
            {
                ClearOutline();
                focusedInteractable = interactable;
                focusedGameObject = hitObject;
                focusedIsUsable = isUsable;
                ApplyOutline(hitObject);
            }
            else
            {
                focusedIsUsable = isUsable;
            }
        }

        private IInteractable FindInteractableInParents(GameObject start)
        {
            Transform t = start.transform;
            while (t != null)
            {
                if (t.TryGetComponent(out IInteractable interactable))
                    return interactable;
                t = t.parent;
            }
            return null;
        }

        private void ApplyOutline(GameObject target)
        {
            focusedOutline = target.GetComponent<Outline>();
            if (focusedOutline == null)
                return;

            focusedOutline.enabled = true;
            ShowPrompt(true);
            SetPromptTarget(target);
        }

        private void ClearOutline()
        {
            if (focusedOutline != null)
                focusedOutline.enabled = false;

            focusedOutline = null;
            focusedInteractable = null;
            focusedGameObject = null;
            focusedIsUsable = false;
            ShowPrompt(false);
            promptHasTarget = false;
        }

        private void ShowPrompt(bool show)
        {
            if (interactionPrompt == null)
                return;

            if (show)
            {
                if (interactionIconImage != null && iconProvider != null)
                {
                    interactionIconImage.sprite = iconProvider.GetCurrentInteractIcon();
                    interactionIconImage.enabled = interactionIconImage.sprite != null;
                }

                if (interactionPromptText != null)
                {
                    string text = LocalizationManager.Instance != null
                        ? LocalizationManager.Instance.GetLocalizedValue(interactionKey)
                        : "Interact";

                    interactionPromptText.text = text;
                    interactionPromptText.enabled = true;
                }
            }
            else
            {
                if (interactionPromptText != null)
                    interactionPromptText.text = "";

                if (interactionIconImage != null)
                    interactionIconImage.enabled = false;
            }

            interactionPrompt.SetActive(show);

            if (show && focusedGameObject != null)
                SetPromptTarget(focusedGameObject);
        }

        private void SetPromptTarget(GameObject go)
        {
            promptHasTarget = true;

            var c = go.GetComponent<Collider>();
            promptTargetWorldPosition = c != null
                ? c.bounds.center + promptWorldOffset
                : go.transform.position + promptWorldOffset;

            interactionPrompt.transform.position = promptTargetWorldPosition;
        }

        private void UpdatePromptPosition()
        {
            if (!promptHasTarget || interactionPrompt == null)
                return;

            var cam = Camera.main;
            if (cam == null) return;

            interactionPrompt.transform.LookAt(
                interactionPrompt.transform.position + cam.transform.rotation * Vector3.forward,
                cam.transform.rotation * Vector3.up
            );
        }

        private void OnControlsChanged(PlayerInput input)
        {
            if (focusedInteractable != null)
                ShowPrompt(true);
        }

        private void OnLanguageChanged()
        {
            if (focusedInteractable != null)
                ShowPrompt(true);
        }
    }
}
