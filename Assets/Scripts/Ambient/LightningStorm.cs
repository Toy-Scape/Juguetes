using System.Collections;
using UnityEngine;
using UI_System.Menus;

public class LightningStorm : MonoBehaviour
{
    public Light lightningLight;
    public AudioSource thunderAudio;

    [Header("Tiempo entre rayos (segundos)")]
    public float minTimeBetweenLightnings = 60f;
    public float maxTimeBetweenLightnings = 300f;

    void Start()
    {
        if (lightningLight == null)
            Debug.LogError("Lightning Light NO asignada!");

        if (thunderAudio == null)
            Debug.LogError("Thunder Audio NO asignado!");

        StartCoroutine(StormRoutine());
    }

    IEnumerator StormRoutine()
    {
        while (true)
        {
            // Espera SOLO cuando no está en pausa
            yield return new WaitUntil(() => !GamePauseHandler.IsPaused);

            float waitTime = Random.Range(minTimeBetweenLightnings, maxTimeBetweenLightnings);
            yield return new WaitForSecondsRealtime(waitTime);

            StartCoroutine(DoLightning());
        }
    }

    IEnumerator DoLightning()
    {
        int flashes = Random.Range(2, 4);

        for (int i = 0; i < flashes; i++)
        {
            if (GamePauseHandler.IsPaused)
                yield break;

            float flashDuration = Random.Range(0.05f, 0.15f);
            float intervalBetweenFlashes = Random.Range(0.05f, 0.2f);

            yield return FlashLightning(flashDuration);
            yield return new WaitForSecondsRealtime(intervalBetweenFlashes);
        }

        float thunderDelay = Random.Range(0.5f, 2.5f);
        yield return new WaitForSecondsRealtime(thunderDelay);

        if (GamePauseHandler.IsPaused)
            yield break;

        thunderAudio.PlayOneShot(thunderAudio.clip);
    }

    IEnumerator FlashLightning(float duration)
    {
        float originalIntensity = lightningLight.intensity;
        lightningLight.intensity = 8f;

        yield return new WaitForSecondsRealtime(duration);

        lightningLight.intensity = originalIntensity;
    }
}
