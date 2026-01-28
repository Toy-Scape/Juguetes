using System.Collections;
using UnityEngine;

public class MenuMusicFader : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private float _fadeDuration = 1.5f;
    private bool _isFading;

    private static MenuMusicFader _instance;

    private void Awake()
    {
        // Singleton simple (evita duplicados)
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();
    }

    public void FadeOutAndStop()
    {
        if (_isFading) return;
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        _isFading = true;

        float startVolume = _audioSource.volume;
        float time = 0f;

        while (time < _fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            _audioSource.volume = Mathf.Lerp(startVolume, 0f, time / _fadeDuration);
            yield return null;
        }

        _audioSource.volume = 0f;
        _audioSource.Stop();
        _isFading = false;
    }


    // Opcional: por si vuelves a abrir el menú
    public void FadeIn()
    {
        if (_audioSource.isPlaying) return;
        StartCoroutine(FadeInRoutine());
    }

    public void StopImmediately()
    {
        StopAllCoroutines();
        _audioSource.Stop();
        _audioSource.volume = 0f;
        _isFading = false;
    }


    private IEnumerator FadeInRoutine()
    {
        _audioSource.volume = 0f;
        _audioSource.Play();

        float time = 0f;
        while (time < _fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            _audioSource.volume = Mathf.Lerp(0f, 1f, time / _fadeDuration);
            yield return null;
        }

        _audioSource.volume = 1f;
    }
}
