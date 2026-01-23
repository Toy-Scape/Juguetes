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
        private Transform _initialFollow;
        private Transform _initialLookAt;

        private void Awake()
        {
            _resolver = GetComponent<ISceneReferenceResolver>();
            _brain = FindFirstObjectByType<CinemachineBrain>();

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

        public void SetActive(bool active)
        {
            if (active)
            {
                ActivateCinematicCamera();
            }
            else
            {
                ResetCamera();
            }
        }

        public void MoveTo(string targetId, float duration, bool smooth = true)
        {
            Transform target = _resolver.Resolve(targetId);
            if (target == null) return;

            if (cinematicCamera == null) return;

            // Simple implementation: Teleport/Follow target
            cinematicCamera.Follow = target;
        }

        public void LookAt(string targetId, float duration)
        {
            Transform target = _resolver.Resolve(targetId);
            if (target == null) return;

            if (cinematicCamera != null)
            {
                cinematicCamera.LookAt = target;
            }
        }

        public void ResetCamera()
        {
            if (cinematicCamera != null)
            {
                cinematicCamera.Priority = 0; // Return control to gameplay camera
                cinematicCamera.Follow = _initialFollow;
                cinematicCamera.LookAt = _initialLookAt;
            }
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
