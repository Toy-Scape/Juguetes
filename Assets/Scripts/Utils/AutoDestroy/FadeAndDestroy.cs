using System.Collections;
using UnityEngine;

public class FadeAndDestroy : AutoDestroyBase
{
    public float duration = 2f;
    public float fadeOutTime = 0.5f;

    private CanvasGroup cg;

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
    }

    public override void BeginDestroy()
    {
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        yield return new WaitForSeconds(duration);

        float t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            cg.alpha = 1f - (t / fadeOutTime);
            yield return null;
        }

        Destroy(gameObject);
    }
}
