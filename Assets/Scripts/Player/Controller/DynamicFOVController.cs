using Unity.Cinemachine;
using UnityEngine;

public class DynamicFOVController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CinemachineCamera virtualCamera;

    private float _defaultFOV;
    private float _targetFOV;

    // Pitch calculation
    private const float PITCH_THRESHOLD = 270f; // Angles above this are "looking up" locally if using 0-360 range

    private void Start()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (virtualCamera == null)
        {
            // Try to find the main camera if not assigned
            var brain = FindFirstObjectByType<CinemachineBrain>();
            if (brain != null && brain.ActiveVirtualCamera != null)
            {
                virtualCamera = brain.ActiveVirtualCamera as CinemachineCamera;
            }
        }

        if (playerController != null && playerController.Config != null)
        {
            _defaultFOV = playerController.Config.BaseFOV;
        }
        else if (virtualCamera != null)
        {
            _defaultFOV = virtualCamera.Lens.FieldOfView;
        }
    }

    private void LateUpdate()
    {
        if (playerController == null || virtualCamera == null) return;

        CalculateTargetFOV();
        UpdateCameraFOV();
    }

    private void CalculateTargetFOV()
    {
        float fovModifier = 0f;

        // 1. Sprint Modifier
        // Only apply if sprinting AND moving
        bool isMoving = playerController.CharacterController.velocity.magnitude > 0.1f;
        if (playerController.Context != null && playerController.Context.IsSprinting && isMoving)
        {
            fovModifier += playerController.Config.SprintFOVModifier;
        }

        // 2. Look Up Modifier
        float pitch = playerController.CameraTransform.localEulerAngles.x;
        float lookUpFactor = 0f;

        // localEulerAngles.x is 0..90 when looking down, 360..270 when looking up (wrapped)
        // Adjust logic based on your camera rig. Assuming standard pitch:
        // -90 (up) to 90 (down) often shows as:
        // 0 to 90 (down)
        // 360 to 270 (up) aka -1 to -90

        if (pitch > PITCH_THRESHOLD)
        {
            // Normalize 360..270 to 0..1
            // 360 -> 0
            // 270 -> 1 (90 degrees up)
            float angleDiff = 360f - pitch;
            lookUpFactor = Mathf.Clamp01(angleDiff / 90f);
        }

        fovModifier += lookUpFactor * playerController.Config.LookUpFOVModifier;

        _targetFOV = playerController.Config.BaseFOV + fovModifier;
    }

    private void UpdateCameraFOV()
    {
        float currentFOV = virtualCamera.Lens.FieldOfView;
        float newFOV = Mathf.Lerp(currentFOV, _targetFOV, Time.deltaTime * playerController.Config.FOVSmoothTime);
        virtualCamera.Lens.FieldOfView = newFOV;
    }
}
