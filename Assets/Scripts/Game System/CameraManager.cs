using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineInputAxisController[] inputControllers;
    [SerializeField] private CinemachineCamera dialogueCamera; // Reference to the Dialogue VCam
    [SerializeField] private Vector3 dialogueOffset = new Vector3(0.6f, 1.6f, -1.2f); // Right Shoulder

    private GameObject _dialogueAnchor;
    private GameObject _player;

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
    }

    private void OnDisable()
    {
        InputMapManager.OnActionMapChanged -= HandleActionMapChanged;
        DialogueBox.OnDialogueVisibleClose -= HandleDialogueVisibleClose;
        if (_dialogueAnchor != null) Destroy(_dialogueAnchor);
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
}