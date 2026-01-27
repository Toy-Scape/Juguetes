using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FadeAndDestroy : AutoDestroyBase
{
    public float duration = 2f;
    public float fadeOutTime = 0.5f;

    private Graphic[] graphics;

    protected override void Awake ()
    {
        graphics = GetComponentsInChildren<Graphic>(true);
        base.Awake();
    }

    public override void BeginDestroy ()
    {
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine ()
    {
        yield return new WaitForSeconds(duration);

        float t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            float alpha = 1f - (t / fadeOutTime);

            foreach (var g in graphics)
            {
                if (g != null)
                {
                    Color c = g.color;
                    c.a = alpha;
                    g.color = c;
                }
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
