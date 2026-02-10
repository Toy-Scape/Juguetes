using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PauseMenuBlur : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup blurPanel;
    public CanvasGroup menuCanvas;

    [Header("Settings")]
    public float fadeDuration = 0.3f;

    public void OpenMenu()
    {
        Debug.Log("Opening Pause Menu with Blur Effect");
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(FadeCanvasGroup(blurPanel, 0f, 0.5f, fadeDuration));
        StartCoroutine(FadeCanvasGroup(menuCanvas, 0f, 1f, fadeDuration));
    }

    public void CloseMenu()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutAndDeactivate());
    }

    private IEnumerator FadeOutAndDeactivate()
    {
        yield return StartCoroutine(FadeCanvasGroup(blurPanel, blurPanel.alpha, 0f, fadeDuration));
        yield return StartCoroutine(FadeCanvasGroup(menuCanvas, menuCanvas.alpha, 0f, fadeDuration));
        gameObject.SetActive(false);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        cg.alpha = to;
    }
}
