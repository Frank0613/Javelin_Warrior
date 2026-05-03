using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class FadeUI : MonoBehaviour
{
    [Header("Settings")]
    public float fadeDuration = 1f;
    public bool fadeOutOnStart = false;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        if (fadeOutOnStart)
        {
            canvasGroup.alpha = 1f;
            FadeIn();
        }
    }

    public void FadeOut()
    {
        StopAllCoroutines();
        StartCoroutine(Fade(1f));
    }

    public void FadeIn()
    {
        StopAllCoroutines();
        StartCoroutine(Fade(0f));
    }

    public void FadeTo(float targetAlpha)
    {
        StopAllCoroutines();
        StartCoroutine(Fade(targetAlpha));
    }

    IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}