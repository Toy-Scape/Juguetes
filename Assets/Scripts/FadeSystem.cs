using System.Collections;
using UnityEngine;

public class FadeSystem : MonoBehaviour
{
    public static FadeSystem Instance;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 1f;

    private Coroutine currentFade;

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

    public void FadeOut()
    {
        StartFade(1);
    }

    public void FadeIn()
    {
        StartFade(0);
    }

    private void StartFade(float target)
    {
        if (currentFade != null)
            StopCoroutine(currentFade);

        currentFade = StartCoroutine(FadeRoutine(target));
    }

    private IEnumerator FadeRoutine(float target)
    {
        float start = canvasGroup.alpha;
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;

            canvasGroup.alpha = Mathf.Lerp(start, target, time / fadeDuration);

            yield return null;
        }

        canvasGroup.alpha = target;
    }

    public IEnumerator FadeOutRoutine()
    {
        yield return FadeRoutine(1);
    }

    public IEnumerator FadeInRoutine()
    {
        yield return FadeRoutine(0);
    }
}