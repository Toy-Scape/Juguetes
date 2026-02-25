using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip musicaA;
    public AudioClip musicaB;

    void Start()
    {
        audioSource.clip = musicaA;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void ChangeFade(AudioClip nuevaMusica)
    {
        StartCoroutine(FadeMusic(nuevaMusica));
    }

    public void ChangeToMusicB()
    {
        ChangeFade(musicaB);
    }

    public void PauseMusic()
    {
        if (audioSource.isPlaying)
            audioSource.Pause();
    }

    public void ResumeMusic()
    {
        audioSource.UnPause();
    }

    IEnumerator FadeMusic(AudioClip nuevaClip)
    {
        while (audioSource.volume > 0)
        {
            audioSource.volume -= Time.deltaTime;
            yield return null;
        }

        audioSource.clip = nuevaClip;
        audioSource.Play();

        while (audioSource.volume < 1)
        {
            audioSource.volume += Time.deltaTime;
            yield return null;
        }
    }
}