using CinematicSystem.Core;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

namespace CinematicSystem.Infrastructure
{
    [RequireComponent(typeof(ISceneReferenceResolver))]
    public class CinemachineCameraController : MonoBehaviour, ICameraController
    {
        [Header("Cinemachine")]
        [SerializeField] private CinemachineCamera cinematicCamera;

        [Header("Settings")]
        
        private ISceneReferenceResolver _resolver;
        private CinemachineBrain _brain;
        // private float _defaultBlendTime; // Not needed
        private Transform _initialFollow;
        private Transform _initialLookAt;

        private GameObject _pivot;
        private GameObject _cameraHandle;
        private bool _isOrbiting;
        private float _currentOrbitSpeed;

        private void Awake()
        {
            _resolver = GetComponent<ISceneReferenceResolver>();
            _brain = FindFirstObjectByType<CinemachineBrain>();

            // Create pivot hierarchy
            _pivot = new GameObject("Cinematic_Pivot");
            _cameraHandle = new GameObject("Cinematic_CameraHandle");
            _cameraHandle.transform.SetParent(_pivot.transform);
            DontDestroyOnLoad(_pivot); // Optional, maybe better to keep in scene context

            if (cinematicCamera == null)
            {
                Debug.LogError("[CinemachineCameraController] Cinemachine Camera is not assigned!");
            }
            else
            {
                // Ensure it starts disabled or with low priority
                cinematicCamera.Priority = 0;
            }
        }

        private void OnDestroy()
        {
            if (_pivot != null) Destroy(_pivot);
        }

        private void Update()
        {
            if (_isOrbiting && _pivot != null)
            {
                // Debug.Log($"Orbiting: {_currentOrbitSpeed}"); // Commented out to avoid spam unless needed
                _pivot.transform.Rotate(Vector3.up, _currentOrbitSpeed * Time.unscaledDeltaTime, Space.World);
            }
        }

        public void SetActive(bool active, bool instant = false)
        {
            if (active)
            {
                ActivateCinematicCamera();
            }
            else
            {
                ResetCamera(instant);
            }
        }

        public void SetShot(string targetId, string lookAtId, Vector3 offset, float fov, float duration, bool useOrbit, float orbitSpeed, bool instant = false, bool ignoreCollision = false)
        {
            Transform target = _resolver.Resolve(targetId);
            if (target == null) return;

            Transform lookAt = string.IsNullOrEmpty(lookAtId) ? target : _resolver.Resolve(lookAtId);

            if (cinematicCamera == null) return;

            // Handle Collision (Anti-Zoom)
            // Try to find CinemachineCollider or Deoccluder. 
            // In Unity 6 / CM 3, it might be a component on the camera object.
            var collider = cinematicCamera.GetComponent<CinemachineCollider>();
            if (collider != null) collider.enabled = !ignoreCollision;

            var deoccluder = cinematicCamera.GetComponent<CinemachineDeoccluder>();
            if (deoccluder != null) deoccluder.enabled = !ignoreCollision; 

            // Setup Pivot
            _pivot.transform.position = target.position;
            // Reset rotation to Identity ensures the offset is World-Aligned (relative to Pivot center)
            // This prevents the camera from flip-flopping if the target has weird rotations.
            _pivot.transform.rotation = Quaternion.identity; 

            // apply offset to handle
            _cameraHandle.transform.localPosition = offset;
            
            // Assign Follow Target (Critical for Orbit)
            cinematicCamera.Follow = _cameraHandle.transform;

            // LookAt
            cinematicCamera.LookAt = lookAt; 
            
            cinematicCamera.Lens.FieldOfView = fov;

            _isOrbiting = useOrbit;
            _currentOrbitSpeed = orbitSpeed;
            
            // IMPORTANT: If 'instant' is requested, we force the camera to warp/reset
            if (instant)
            {
                // Force VCam reset
                cinematicCamera.PreviousStateIsValid = false;

                // FORCE BRAIN CUT
                // If we are transitioning, the Brain uses the Default Blend (usually).
                // We temporarily set it to 0 to force a Cut.
                if (_brain != null)
                {
                    StartCoroutine(ForceCutRoutine());
                }
            }
        }

        public void ResetCamera(bool instant = false)
        {
            if (cinematicCamera != null)
            {
                Debug.Log($"[CinemachineCameraController] Resetting Camera. Instant: {instant}");
                
                cinematicCamera.Priority = 0; // Return control to gameplay camera
                cinematicCamera.Follow = _initialFollow;
                cinematicCamera.LookAt = _initialLookAt;
                _isOrbiting = false;

                if (instant && _brain != null)
                {
                     StartCoroutine(ForceCutRoutine());
                }
            }
        }
        
        private System.Collections.IEnumerator ForceCutRoutine()
        {
            if (_brain == null) yield break;
            
            Debug.Log("[CinemachineCameraController] Forcing Cut on Brain...");

            // Save original settings
            var originalBlend = _brain.DefaultBlend;
            var originalCustomBlends = _brain.CustomBlends; // Try property first for CM 3 compatibility
            
            // Set temporary Cut
            _brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0);
            _brain.CustomBlends = null; // Disable custom blends for this frame

            // Wait for one frame (Brain update)
            yield return null; 

            // Restore
            _brain.DefaultBlend = originalBlend;
            _brain.CustomBlends = originalCustomBlends;
            Debug.Log("[CinemachineCameraController] Restored Brain Default Blend and Custom Blends.");
        }

        public void MoveTo(string targetId, float duration, bool smooth = true)
        {
            SetShot(targetId, "", new Vector3(0, 5, -10), 60, duration, false, 0);
        }

        public void LookAt(string targetId, float duration)
        {
            // Legacy/Simple support
            Transform target = _resolver.Resolve(targetId);
            if (cinematicCamera != null && target != null) cinematicCamera.LookAt = target;
        }

        public void ActivateCinematicCamera()
        {
            if (cinematicCamera != null)
            {
                // Store initial state if needed
                _initialFollow = cinematicCamera.Follow;
                _initialLookAt = cinematicCamera.LookAt;

                cinematicCamera.Priority = 100; // Force high priority
            }
        }
    }
}
