using System.Collections;
using CheckpointSystem;
using UnityEngine;

namespace UI.Fade
{
    public class DeathSequenceManager : MonoBehaviour
    {
        public static DeathSequenceManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private Animator fadeAnimator;

        private Coroutine _deathCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void TriggerDeathSequence()
        {
            if (_deathCoroutine != null) return;

            var camController = FindFirstObjectByType<CinematicSystem.Infrastructure.CinemachineCameraController>();
            if (camController != null)
            {
                camController.ResetCamera(true);
            }

            var cinematicPlayer = FindFirstObjectByType<CinematicSystem.Application.CinematicPlayer>();
            if (cinematicPlayer != null && cinematicPlayer.IsPlaying)
            {
                cinematicPlayer.Stop();
            }

            _deathCoroutine = StartCoroutine(DeathRoutine());
        }

        /*private IEnumerator WaitForAnimation()
        {
            yield return null;

            AnimatorStateInfo state = fadeAnimator.GetCurrentAnimatorStateInfo(0);

            while (state.normalizedTime < 1f || fadeAnimator.IsInTransition(0))
            {
                yield return null;
                state = fadeAnimator.GetCurrentAnimatorStateInfo(0);
            }
        }*/

        private IEnumerator DeathRoutine()
        {
            yield return FadeSystem.Instance.FadeOutRoutine();

            CheckpointManager.Instance?.RespawnPlayer();

            yield return new WaitForSecondsRealtime(0.2f);

            yield return FadeSystem.Instance.FadeInRoutine();

            _deathCoroutine = null;
        }
    }
}