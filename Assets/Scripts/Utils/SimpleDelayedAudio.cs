using System.Collections;
using UnityEngine;

namespace Utils.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class SimpleDelayedAudio : MonoBehaviour
    {
        [Tooltip("Delay in seconds before playing the audio.")]
        [SerializeField] private float _delay = 0f;

        [Tooltip("If true, plays automatically on Start. If false, call Play() manually.")]
        [SerializeField] private bool _playOnStart = true;

        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            if (_playOnStart)
            {
                Play();
            }
        }

        public void Play()
        {
            StartCoroutine(PlayRoutine());
        }

        private IEnumerator PlayRoutine()
        {
            if (_delay > 0)
            {
                yield return new WaitForSeconds(_delay);
            }

            if (_audioSource != null)
            {
                _audioSource.Play();
            }
        }
    }
}
