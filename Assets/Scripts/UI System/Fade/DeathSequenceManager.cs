using System.Collections;
using CheckpointSystem;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Fade
{
    public class DeathSequenceManager : MonoBehaviour
    {
        public static DeathSequenceManager Instance { get; private set; }

        [Header("References")]
        [Tooltip("The Image component that covers the screen. Needs the CircularFadeUI material assigned.")]
        [SerializeField] private Image fadeImage;
        
        [Header("Settings")]
        [SerializeField] private float fadeDuration = 1.0f;
        [SerializeField] private float blackScreenDuration = 0.5f;

        private Material _fadeMaterial;
        private Coroutine _deathCoroutine;
        
        // Match this property name with the one in your Shader
        private readonly int _radiusProperty = Shader.PropertyToID("_CircleRadius");

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (fadeImage != null)
            {
                // We instantiate the material to avoid modifying the asset directly
                _fadeMaterial = new Material(fadeImage.material);
                fadeImage.material = _fadeMaterial;
                
                // Ensure starting open
                SetRadius(1.0f);
                fadeImage.enabled = false;
            }
        }

        public void TriggerDeathSequence()
        {
            if (_deathCoroutine != null) return; // Prevent double trigger
            
            // Instant priority 0 for the cinematic dolly camera as requested
            var camController = FindFirstObjectByType<CinematicSystem.Infrastructure.CinemachineCameraController>();
            if (camController != null)
            {
                camController.ResetCamera(true);
            }
            
            // Also stop the cinematic player if it's active
            var cinematicPlayer = FindFirstObjectByType<CinematicSystem.Application.CinematicPlayer>();
            if (cinematicPlayer != null && cinematicPlayer.IsPlaying)
            {
                cinematicPlayer.Stop();
            }
            
            if (fadeImage == null || _fadeMaterial == null)
            {
                Debug.LogError("DeathSequenceManager: Missing Image or Material reference! Falling back to instant respawn.");
                FallbackRespawn();
                return;
            }

            _deathCoroutine = StartCoroutine(DeathSequenceRoutine());
        }

        private IEnumerator DeathSequenceRoutine()
        {
            fadeImage.enabled = true;
            
            // 1. Fade OUT (Radius 1 -> 0)
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                float normalizedTime = t / fadeDuration;
                // Easing out sine
                float radius = Mathf.Lerp(1f, 0f, Mathf.Sin(normalizedTime * Mathf.PI * 0.5f));
                SetRadius(radius);
                yield return null;
            }
            
            SetRadius(0f); // Fully black

            // 2. Respawn logic
            if (CheckpointManager.Instance != null)
            {
                CheckpointManager.Instance.RespawnPlayer();
            }
            else
            {
                Debug.LogWarning("DeathSequenceManager: No CheckpointManager found. Player not moved.");
            }

            // 3. Wait in black screen
            yield return new WaitForSecondsRealtime(blackScreenDuration);

            // 4. Fade IN (Radius 0 -> 1)
            t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                float normalizedTime = t / fadeDuration;
                // Easing out sine
                float radius = Mathf.Lerp(0f, 1f, Mathf.Sin(normalizedTime * Mathf.PI * 0.5f));
                SetRadius(radius);
                yield return null;
            }

            SetRadius(1.0f); // Fully open
            fadeImage.enabled = false;
            
            // Reset state
            _deathCoroutine = null;
        }

        private void SetRadius(float value)
        {
            if (_fadeMaterial != null)
            {
                _fadeMaterial.SetFloat(_radiusProperty, value);
            }
        }

        private void FallbackRespawn()
        {
            if (CheckpointManager.Instance != null)
            {
                CheckpointManager.Instance.RespawnPlayer();
            }
        }
    }
}
