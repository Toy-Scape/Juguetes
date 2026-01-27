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

            EndCinematic(false); // Forced stop might imply cut? Let's default to blend unless user wants cut. Default false.
        }

        private bool _skipRequested;

        public void Advance()
        {
            if (IsPlaying)
            {
                _skipRequested = true;
            }
        }

        // Backward compatibility if needed, or just alias it
        public void Skip() => Advance();

        public IEnumerator Wait(float duration)
        {
            float timer = 0f;
            while (timer < duration)
            {
                if (_skipRequested) yield break; // Break the wait immediately if advanced
                timer += Time.unscaledDeltaTime; // Use unscaled time
                yield return null;
            }
        }

        private IEnumerator PlayRoutine(CinematicAsset cinematic)
        {
            _skipRequested = false;
            SetPlayerInput(!cinematic.blockPlayerInput);

            if (_cameraController != null)
                _cameraController.SetActive(true);

            foreach (var action in cinematic.actions)
            {
                // Reset skip flag at start of new action? 
                // No, if user spammed skip, maybe we want to skip multiple?
                // But for "Advance", we usually mean "Next Action".
                // So we should consume the flag.
                _skipRequested = false;

                if (action != null)
                {
                    if (action.waitForCompletion)
                    {
                        // Note: Actions usually return "WaitForSeconds" or similar.
                        // We rely on the Action using context.Wait() to support interruption.
                        // If the action uses "yield return new WaitForSeconds()", we can't interrupt it easily 
                        // unless we run it in a sub-routine and monitor it.
                        // Ideally, we refactor actions to use context.Wait().
                        yield return action.Execute(this);

                        // If user skipped during execution, we continue to next logic.

                        // Check HOLD AT END
                        Debug.Log($"[CinematicPlayer] Action Finished. HoldAtEnd: {action.holdAtEnd}, AdvanceRequested: {_skipRequested}");

                        if (action.holdAtEnd && !_skipRequested)
                        {
                            Debug.Log("[CinematicPlayer] Holding until Advance...");

                            // Wait until next advance signal
                            // Reset flag first just in case
                            _skipRequested = false;
                            while (!_skipRequested)
                            {
                                yield return null;
                            }
                            Debug.Log("[CinematicPlayer] Advanced from Hold.");
                        }
                    }
                    else
                    {
                        StartCoroutine(action.Execute(this));
                    }
                }
            }

            Debug.Log("[CinematicPlayer] Sequence Finished. Ending Cinematic.");
            yield return StartCoroutine(EndCinematicRoutine(cinematic.restoreCameraInstantly));
        }

        private IEnumerator EndCinematicRoutine(bool instant = false)
        {
            IsPlaying = false;

            if (_cameraController != null)
            {
                _cameraController.SetActive(false, instant); // Reset to gameplay camera

                // If NOT instant, wait for blend to finish before enabling input
                if (!instant)
                {
                    Debug.Log($"[CinematicPlayer] EndCinematicRoutine: Waiting for Blend... Time: {Time.time}");
                    yield return _cameraController.WaitForBlend();
                    Debug.Log($"[CinematicPlayer] EndCinematicRoutine: Blend Finished. Time: {Time.time}");
                }
            }

            Debug.Log($"[CinematicPlayer] EndCinematicRoutine: Enabling Player Input. Time: {Time.time}");
            SetPlayerInput(true);
        }

        // Backward compatibility wrapper for Stop()
        private void EndCinematic(bool instant = false)
        {
            StartCoroutine(EndCinematicRoutine(instant));
        }

        public void SetPlayerInput(bool enabled)
        {
            Debug.Log($"[CinematicPlayer] SetPlayerInput: {enabled}");

            if (InputMapManager.Instance != null)
            {
                if (enabled)
                {
                    // Enable Input -> End Cinematic Mode
                    InputMapManager.Instance.HandleCinematicEnd();
                }
                else
                {
                    // Disable Input -> Start Cinematic Mode
                    InputMapManager.Instance.HandleCinematicStart();
                }
            }
        }
    }
}
