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
        [SerializeField] private GameObject interactionPrompt = null;
        [SerializeField] private TMP_Text interactionPromptText = null;
        [SerializeField] private InputActionReference interactActionReference = null;
        [SerializeField] private string interactionKey = "interaction_prompt";

        [SerializeField] private Image interactionIconImage;

        [Header("Manual Icons (Assign specific button sprites)")]
        [SerializeField] private Sprite iconKeyboard;
        [SerializeField] private Sprite iconXbox;
        [SerializeField] private Sprite iconPlayStation;
        [SerializeField] private Sprite iconSwitch;

        [Header("Feedback")]
        [SerializeField] private GamepadVibration vibration;
        [SerializeField] private PlayerConfig config;

        [Header("Prompt Positioning")]
        [SerializeField] private Vector3 promptWorldOffset = new Vector3(0f, 1.5f, 0f);

        private IInteractable focusedInteractable;
        private GameObject focusedGameObject;
        private Outline focusedOutline;
        private bool focusedIsUsable;
        private Dictionary<int, IInteractable> interactableCache = new Dictionary<int, IInteractable>();
        private string promptBindingDisplay = "";
        private RectTransform promptRectTransform;
        private Vector3 promptTargetWorldPosition;
        private bool promptHasTarget;
        private PlayerInventory playerInventory;

        // Cache reference to ensure we subscribe/unsubscribe to the same object
        private PlayerInput _cachedPlayerInput;

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

            // Find PlayerInput consistently
            _cachedPlayerInput = GetComponentInParent<PlayerInput>();
            if (_cachedPlayerInput == null) _cachedPlayerInput = FindFirstObjectByType<PlayerInput>();

            if (_cachedPlayerInput != null)
            {
                _cachedPlayerInput.onControlsChanged += HandleControlsChanged;
            }
            else
            {
                Debug.LogWarning("[PlayerInteractor] Could not find PlayerInput! automatic icon switching will not work.");
            }

            UpdateBindingDisplay();

            if (LocalizationManager.Instance != null)
                LocalizationManager.OnLanguageChanged += HandleLanguageChanged;

            if (interactionPrompt != null)
            {
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
                    var agPc = GetComponent<PlayerController>();
                    if (agPc != null) config = agPc.Config;
                }
            }
        }

        void Start()
        {
        }

        void OnValidate()
        {
            InteractableBase.SetGlobalOutlineProperties(outlineColor, outlineWidth, outlineMode);
        }

        private void OnDestroy()
        {
            if (LocalizationManager.Instance != null)
                LocalizationManager.OnLanguageChanged -= HandleLanguageChanged;

            if (_cachedPlayerInput != null)
            {
                _cachedPlayerInput.onControlsChanged -= HandleControlsChanged;
            }
        }

        private void HandleLanguageChanged()
        {
            if (focusedInteractable != null)
            {
                ShowPrompt(true, promptBindingDisplay);
            }
        }

        private void HandleControlsChanged(PlayerInput input)
        {
            // Debug.Log($"[Interaction] Controls changed to: {input.currentControlScheme}");
            UpdateBindingDisplay();
            if (focusedInteractable != null)
            {
                ShowPrompt(true, promptBindingDisplay);
            }
        }

        private void UpdateBindingDisplay()
        {
            // Just triggers the prompt update, we don't strictly need the binding string for Manual Icons mode
            // but we keep it to support potentially getting the binding index if needed later.
            if (interactActionReference == null || interactActionReference.action == null)
            {
                promptBindingDisplay = "E";
                return;
            }

            // We can use this to detect if the binding changed significantly, but for now we just rely on ShowPrompt logic.
            promptBindingDisplay = "Interact";
        }

        void Update()
        {
            if (focusedInteractable != null && focusedGameObject == null)
            {
                ClearOutline();
            }

            UpdatePromptPosition();
            CheckForInteractable();
        }

        public void OnInteract()
        {
            if (focusedInteractable == null) return;
            if (!focusedIsUsable) return;

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
                }

                if (interactable != null)
                {
                    bool isUsable = interactable.IsInteractable();

                    if (focusedGameObject != hitObject)
                    {
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

                    return;
                }
            }

            if (focusedGameObject != null)
            {
                ClearOutline();
            }
        }

        private IInteractable FindInteractableInParents(GameObject start)
        {
            Transform t = start.transform;
            while (t != null)
            {
                if (t.TryGetComponent<IInteractable>(out var interactable)) return interactable;
                t = t.parent;
            }
            return null;
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

        private void ApplyOutline(GameObject target)
        {
            if (target == null) return;

            focusedOutline = target.GetComponent<Outline>();
            if (focusedOutline == null)
            {
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

            if (show)
            {
                // Determine Platform
                bool isPlayStation = false;
                bool isSwitch = false;
                bool isXbox = true; // Default/PC Gamepad usually maps to Xbox

                if (_cachedPlayerInput != null && _cachedPlayerInput.devices.Count > 0)
                {
                    var device = _cachedPlayerInput.devices[0];
                    string deviceName = device.name.ToLower();
                    string layout = device.layout.ToLower();

                    if (deviceName.Contains("ps") || deviceName.Contains("dualshock") || deviceName.Contains("playstation") || layout.Contains("dualshock"))
                    {
                        isPlayStation = true;
                        isXbox = false;
                    }
                    else if (deviceName.Contains("switch") || deviceName.Contains("pro") || layout.Contains("switch"))
                    {
                        isSwitch = true;
                        isXbox = false;
                    }
                    else if (deviceName.Contains("keyboard") || deviceName.Contains("mouse"))
                    {
                        isXbox = false; // It's Keyboard
                    }
                }

                // Select Icon based on Platform
                Sprite targetIcon = iconKeyboard; // Default to keyboard if not gamepad

                // If scheme is "Gamepad" or similar, use gamepad icons
                // Or if we detected a Gamepad device above (isXbox || isPS || isSwitch)
                string scheme = _cachedPlayerInput != null ? _cachedPlayerInput.currentControlScheme : "";

                // Robust check: check Scheme OR device type logic
                bool isGamepadParams = (isXbox || isPlayStation || isSwitch);
                bool isGamepadScheme = scheme.ToLower().Contains("gamepad") || scheme.ToLower().Contains("joystick");

                if (isGamepadScheme || (isGamepadParams && !scheme.ToLower().Contains("keyboard")))
                {
                    if (isPlayStation) targetIcon = iconPlayStation;
                    else if (isSwitch) targetIcon = iconSwitch;
                    else targetIcon = iconXbox;
                }

                // Set Image
                if (interactionIconImage != null)
                {
                    if (targetIcon != null)
                    {
                        interactionIconImage.sprite = targetIcon;
                        interactionIconImage.gameObject.SetActive(true);
                        interactionIconImage.enabled = true;
                        interactionIconImage.color = Color.white;
                    }
                    else
                    {
                        // If no icon assigned, hide it
                        interactionIconImage.enabled = false;
                        interactionIconImage.gameObject.SetActive(false);
                    }
                }

                // Set Description Text
                if (interactionPromptText != null)
                {
                    string format = LocalizationManager.Instance != null
                        ? LocalizationManager.Instance.GetLocalizedValue(interactionKey)
                        : "Interact";

                    interactionPromptText.text = format.Replace("{0}", "").Trim();
                    interactionPromptText.enabled = true;
                }
            }
            else
            {
                if (interactionPromptText != null) interactionPromptText.text = "";
                if (interactionIconImage != null) interactionIconImage.enabled = false;
            }

            interactionPrompt.SetActive(show);

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
