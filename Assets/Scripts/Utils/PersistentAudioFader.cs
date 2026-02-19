using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

namespace Utils
{
    /// <summary>
    /// A component that persists across scene loads and automatically fades out its AudioSource
    /// when the active scene changes, then destroys itself.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class PersistentAudioFader : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Time in seconds to fade out the audio when the scene changes.")]
        [SerializeField] private float fadeOutDuration = 2f;

        [Tooltip("If true, the game object will be destroyed after the fade out completes.")]
        [SerializeField] private bool destroyAfterFade = true;

        private AudioSource _audioSource;
        private bool _isFading = false;

        private string _originSceneName;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            
            // Capture the scene this object belongs to BEFORE moving it to DontDestroyOnLoad.
            // This is crucial for additive scene loading, as we want to ignore the activation of THIS scene.
            _originSceneName = gameObject.scene.name;

            // Detach from parent to ensure DontDestroyOnLoad works correctly
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            // Subscribe to scene change event
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks or errors
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        private void OnActiveSceneChanged(Scene current, Scene next)
        {
            // Only fade out if we are moving TO a scene that is NOT the one this object originated from.
            // This handles the case where the scene is loaded additively and then becomes active (we don't want to fade then).
            // We only want to fade when we LEAVE this scene for another one.
            if (next.name != _originSceneName)
            {
                StartFadeOut();
            }
        }

        public void StartFadeOut()
        {
            if (_isFading) return;
            _isFading = true;

            // Unsubscribe immediately so it doesn't trigger again
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;

            if (_audioSource != null)
            {
                // Use DOTween to fade volume to 0
                _audioSource.DOFade(0f, fadeOutDuration)
                    .SetEase(Ease.OutQuad)
                    .SetUpdate(true) // Ignore Time.timeScale
                    .OnComplete(() =>
                    {
                        if (destroyAfterFade)
                        {
                            Destroy(gameObject);
                        }
                    });
            }
            else
            {
                // Fallback if AudioSource is missing
                if (destroyAfterFade)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
