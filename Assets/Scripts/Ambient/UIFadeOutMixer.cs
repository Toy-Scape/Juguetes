using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class UIFadeOutMixer : MonoBehaviour
{
    [SerializeField] private AudioMixer _mixer;
    [SerializeField] private string _volumeParam = "UI_Volume";
    [SerializeField] private float _fadeDuration = 1.5f;

    public void FadeOut()
    {
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        _mixer.GetFloat(_volumeParam, out float startVolume);

        float time = 0f;
        while (time < _fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            float v = Mathf.Lerp(startVolume, -80f, time / _fadeDuration);
            _mixer.SetFloat(_volumeParam, v);
            yield return null;
        }

        _mixer.SetFloat(_volumeParam, -80f);
    }
}
