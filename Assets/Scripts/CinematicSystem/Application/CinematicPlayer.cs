using System.Collections;
using CinematicSystem.Core;
using UnityEngine;

namespace CinematicSystem.Application
{
    public class CinematicPlayer : MonoBehaviour, ICinematicPlayer, ICinematicContext
    {
        [Header("References")]
        [SerializeField] private GameObject cameraControllerObj;
        [SerializeField] private GameObject resolverObj;

        private ICameraController _cameraController;
        private ISceneReferenceResolver _resolver;
        private Coroutine _currentCoroutine;

        // ICinematicContext implementation
        public ICameraController CameraController => _cameraController;
        public ISceneReferenceResolver Resolver => _resolver;
        public MonoBehaviour CoroutineRunner => this;

        public bool IsPlaying { get; private set; }

        private void Awake()
        {
            // Resolve dependencies
            if (cameraControllerObj != null)
                _cameraController = cameraControllerObj.GetComponent<ICameraController>();

            if (resolverObj != null)
                _resolver = resolverObj.GetComponent<ISceneReferenceResolver>();

            // Fallbacks or finding via interface if objects are null
            if (_cameraController == null) _cameraController = GetComponentInChildren<ICameraController>();
            if (_resolver == null) _resolver = GetComponentInChildren<ISceneReferenceResolver>();
        }

        public void Play(CinematicAsset cinematic)
        {
            if (IsPlaying)
            {
                Debug.LogWarning("[CinematicPlayer] Already playing a cinematic.");
                return;
            }

            if (cinematic == null) return;

            IsPlaying = true;
            _currentCoroutine = StartCoroutine(PlayRoutine(cinematic));
        }

        public void Stop()
        {
            if (_currentCoroutine != null)
            {
                StopCoroutine(_currentCoroutine);
                _currentCoroutine = null;
            }

            EndCinematic();
        }

        private IEnumerator PlayRoutine(CinematicAsset cinematic)
        {
            SetPlayerInput(!cinematic.blockPlayerInput);

            if (_cameraController != null)
                _cameraController.SetActive(true);

            foreach (var action in cinematic.actions)
            {
                if (action != null)
                {
                    if (action.waitForCompletion)
                    {
                        yield return action.Execute(this);
                    }
                    else
                    {
                        StartCoroutine(action.Execute(this));
                    }
                }
            }

            EndCinematic();
        }

        private void EndCinematic()
        {
            IsPlaying = false;

            if (_cameraController != null)
                _cameraController.SetActive(false); // Reset to gameplay camera

            SetPlayerInput(true);
        }

        public void SetPlayerInput(bool enabled)
        {
            // Ideally call a Game / Input Manager here.
            // For now we log.
            Debug.Log($"[CinematicPlayer] SetPlayerInput: {enabled}");

            // Example integration (commented out):
            // if (InputManager.Instance != null) InputManager.Instance.SetInputEnabled(enabled);
        }
    }
}
