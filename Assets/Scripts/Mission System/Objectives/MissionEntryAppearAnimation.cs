using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MissionEntryAppearAnimation : MonoBehaviour
{
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0.8f, 1, 1f);
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake ()
    {
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        transform.localScale = Vector3.one * 0.8f;
    }

    private void OnEnable ()
    {
        StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation ()
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float normalized = t / duration;

            // Escala
            float scale = scaleCurve.Evaluate(normalized);
            transform.localScale = Vector3.one * scale;

            // Alpha
            canvasGroup.alpha = normalized;

            yield return null;
        }

        transform.localScale = Vector3.one;
        canvasGroup.alpha = 1f;
    }
}
