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
            var collider = cinematicCamera.GetComponent<CinemachineCollider>();
            if (collider != null) collider.enabled = !ignoreCollision;

            var deoccluder = cinematicCamera.GetComponent<CinemachineDeoccluder>();
            if (deoccluder != null) deoccluder.enabled = !ignoreCollision; 

            // Setup Pivot
            _pivot.transform.position = target.position;
            _pivot.transform.rotation = Quaternion.identity; 

            // apply offset to handle
            _cameraHandle.transform.localPosition = offset;
            
            // Assign Follow Target
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
                if (_brain != null)
                {
                    ApplyInstantCut();
                }
            }
        }

        // State for restoration
        private CinemachineBlendDefinition _savedBlend;
        private CinemachineBlenderSettings _savedCustomBlends;

        public void ResetCamera(bool instant = false)
        {
            if (cinematicCamera != null)
            {
                Debug.Log($"[CinemachineCameraController] Resetting Camera. Instant: {instant}");
                
                if (instant && _brain != null)
                {
                     ApplyInstantCut();
                }

                cinematicCamera.Priority = 0; // Return control to gameplay camera
                cinematicCamera.Follow = _initialFollow;
                cinematicCamera.LookAt = _initialLookAt;
                _isOrbiting = false;
            }
        }

        private void ApplyInstantCut()
        {
             // Save state
             _savedBlend = _brain.DefaultBlend;
             _savedCustomBlends = _brain.CustomBlends;
             
             // Apply Cut
             _brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0);
             _brain.CustomBlends = null;
             
             Debug.Log("[CinemachineCameraController] Applied Cut settings synchronously.");

             StartCoroutine(RestoreBrainRoutine());
        }
        
        private System.Collections.IEnumerator RestoreBrainRoutine()
        {
            if (_brain == null) yield break;
            
            // Wait for one frame (Brain update) to ensure the Cut is consumed
            yield return null; 

            // Restore
            _brain.DefaultBlend = _savedBlend;
            _brain.CustomBlends = _savedCustomBlends;
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

        public System.Collections.IEnumerator WaitForBlend()
        {
             if (_brain == null) yield break;

             // Wait a couple of frames for the blend to definitely start logic in Brain
             yield return null;
             yield return null;

             Debug.Log($"[CinemachineCameraController] WaitForBlend Started. IsBlending: {_brain.IsBlending}");

             while (_brain.IsBlending)
             {
                 // Optional: Log blend progress
                 // Debug.Log($"Blending... Time: {_brain.ActiveBlend?.TimeInBlend}");
                 yield return null;
             }
             
             Debug.Log("[CinemachineCameraController] WaitForBlend Finished.");
        }
    }
}
