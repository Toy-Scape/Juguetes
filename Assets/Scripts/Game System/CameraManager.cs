using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineInputAxisController[] inputControllers;
    [SerializeField] private CinemachineCamera crouchingCamera;
    [SerializeField] private CinemachineCamera generalCamera;
    [SerializeField] private CinemachineCamera dialogueCamera;
    [SerializeField] private Vector3 dialogueOffset = new Vector3(0.6f, 1.6f, -1.2f); // Right Shoulder

    private GameObject _dialogueAnchor;
    private GameObject _player;
    private bool CrouchingState;

    public static CameraManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        _dialogueAnchor = new GameObject("DialogueCameraAnchor");
        DontDestroyOnLoad(_dialogueAnchor);
    }

    private void Start()
    {
        LockCursor();
        UnlockCameraMovement();
        if (dialogueCamera != null) dialogueCamera.Priority = 0; // Ensure it starts off

        // Find Player
        var playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null) _player = playerController.gameObject;
    }

    private void Update()
    {
        // Keep anchor updated relative to player
        if (_player != null && _dialogueAnchor != null)
        {
            // Position the anchor exactly at the player's position + rotation
            _dialogueAnchor.transform.position = _player.transform.position;
            _dialogueAnchor.transform.rotation = _player.transform.rotation;
        }
    }

    private void OnEnable()
    {
        InputMapManager.OnActionMapChanged += HandleActionMapChanged;
        DialogueBox.OnDialogueVisibleClose += HandleDialogueVisibleClose;

        if (_player != null)
        {
            var pInput = _player.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (pInput != null) pInput.onControlsChanged += OnControlsChanged;
        }

        UpdateCameraSensitivity();
    }

    private void OnDisable()
    {
        InputMapManager.OnActionMapChanged -= HandleActionMapChanged;
        DialogueBox.OnDialogueVisibleClose -= HandleDialogueVisibleClose;

        if (_player != null)
        {
            var pInput = _player.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (pInput != null) pInput.onControlsChanged -= OnControlsChanged;
        }

        if (_dialogueAnchor != null) Destroy(_dialogueAnchor);
    }

    private void OnControlsChanged(UnityEngine.InputSystem.PlayerInput input)
    {
        UpdateCameraSensitivity();
    }

    private Dictionary<object, float> _initialGains = new Dictionary<object, float>();

    public void UpdateCameraSensitivity()
    {
        // Auto-find if missing
        if (inputControllers == null || inputControllers.Length == 0)
        {
            inputControllers = FindObjectsByType<CinemachineInputAxisController>(FindObjectsSortMode.None);
            Debug.Log($"[CameraManager] Auto-found {inputControllers.Length} CinemachineInputAxisControllers.");
        }

        if (inputControllers == null || inputControllers.Length == 0)
        {
            Debug.LogWarning("[CameraManager] No CinemachineInputAxisController found! Sensitivity cannot be applied.");
            return;
        }

        float sensitivity = 1f;

        // Determine sensitivity based on current control scheme
        if (_player != null)
        {
            var pInput = _player.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (pInput != null)
            {
                if (pInput.currentControlScheme == "Keyboard&Mouse")
                {
                    sensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
                }
                else if (pInput.currentControlScheme == "Gamepad" || pInput.currentControlScheme == "Joystick")
                {
                    sensitivity = PlayerPrefs.GetFloat("GamepadSensitivity", 1f);
                }
            }
        }

        // Apply to all controllers
        int updatedCount = 0;
        foreach (var controller in inputControllers)
        {
            if (controller != null && controller.Controllers != null)
            {
                foreach (var axis in controller.Controllers)
                {
                    if (axis.Input != null)
                    {
                        // Cache initial gain if not already cached
                        if (!_initialGains.ContainsKey(axis))
                        {
                            _initialGains[axis] = axis.Input.Gain;
                            // Debug.Log($"[CameraManager] Cached initial gain for axis {axis.Name}: {axis.Input.Gain}");
                        }

                        // Apply sensitivity as multiplier on base gain
                        axis.Input.Gain = _initialGains[axis] * sensitivity;
                        updatedCount++;
                    }
                }
            }
        }

        Debug.Log($"[CameraManager] Applied Sensitivity {sensitivity} to {updatedCount} axes (Scheme: {_player?.GetComponent<UnityEngine.InputSystem.PlayerInput>()?.currentControlScheme}).");
    }

    private void HandleDialogueVisibleClose()
    {
        // When Dialogue UI closes (even if input remains blocked), reset camera priority
        if (dialogueCamera != null)
        {
            dialogueCamera.Priority = 0;
        }
    }

    private void HandleActionMapChanged(string newMap)
    {
        // Handle Dialogue Camera Priority
        if (dialogueCamera != null)
        {
            if (newMap == ActionMaps.Dialogue)
            {
                // Only enable camera if Dialogue is actually OPEN (visuals)
                // This prevents camera from staying locked if input is delayed but dialogue UI is closed.
                if (DialogueBox.Instance != null && DialogueBox.Instance.IsOpen)
                {
                    dialogueCamera.Priority = 20;

                    // Configure Camera Position (Over the Shoulder)
                    if (_player != null)
                    {
                        // Try to configure CinemachineThirdPersonFollow if present
                        var thirdPerson = dialogueCamera.GetComponent<CinemachineThirdPersonFollow>();
                        if (thirdPerson != null)
                        {
                            thirdPerson.ShoulderOffset = dialogueOffset;
                            dialogueCamera.Follow = _player.transform;
                            dialogueCamera.LookAt = _player.transform;
                        }
                        else
                        {
                            // Fallback: If no dedicated component, we position the VCam manually via a temp target 
                            // attached to the anchor.
                            // For now, let's just create a child on the anchor.
                            Transform camTarget = _dialogueAnchor.transform.Find("CamPos");
                            if (camTarget == null)
                            {
                                camTarget = new GameObject("CamPos").transform;
                                camTarget.SetParent(_dialogueAnchor.transform);
                            }

                            camTarget.localPosition = dialogueOffset;
                            camTarget.localRotation = Quaternion.identity;

                            dialogueCamera.Follow = camTarget;
                            dialogueCamera.LookAt = _player.transform; // Look at player
                        }
                    }
                }
                else
                {
                    // Dialogue map active (input blocked) but UI closed -> Return to gameplay view
                    dialogueCamera.Priority = 0;
                }
            }
            else
            {
                dialogueCamera.Priority = 0;
            }
        }

        if (newMap == ActionMaps.UI)
        {
            UnlockCursor();
            LockCameraMovement();
        }
        else if (newMap == ActionMaps.Dialogue)
        {
            LockCursor();
            LockCameraMovement();
        }
        else
        {
            ActivarCamaraAgachado(this.CrouchingState);
            LockCursor();
            UnlockCameraMovement();
        }
    }

    public void LockCameraMovement()
    {
        if (inputControllers != null)
        {
            foreach (var controller in inputControllers)
            {
                if (controller != null) controller.enabled = false;
            }
        }
    }

    public void UnlockCameraMovement()
    {
        if (inputControllers != null)
        {
            foreach (var controller in inputControllers)
            {
                if (controller != null) controller.enabled = true;
            }
        }
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    void ActivarCamaraAgachado(bool CrouchingState)
    {
        Debug.Log($"Crouching: {CrouchingState}");
        if (CrouchingState)
        {
            generalCamera.Priority = 0;
            crouchingCamera.Priority = 10;
            return;
        }
        generalCamera.Priority = 10;
        crouchingCamera.Priority = 0;
    }


    public void SetCrouchingState(bool CrouchingState) {
        // Añañdo el estado de agachado por si ocurre algun dialogo mientras estamos agachados
        this.CrouchingState = CrouchingState;
        ActivarCamaraAgachado(CrouchingState);
    }

}