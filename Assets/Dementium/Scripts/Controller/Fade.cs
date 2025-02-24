using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Fade : MonoBehaviour
{
    [SerializeField] private Renderer renderer;

    public UnityEvent OnFadeInStart;
    public UnityEvent OnFadeInComplete;
    public UnityEvent OnFadeOutStart;
    public UnityEvent OnFadeOutComplete;

    private Coroutine fadeCoroutine;

    private void Start()
    {
        StartFadeIn(1.0f);
    }

    public void StartFadeIn(float duration = 1.0f)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeIn(duration));
    }

    public void StartFadeOut(float duration = 1.0f)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOut(duration));
    }

    private IEnumerator FadeIn(float duration)
    {
        OnFadeInStart?.Invoke();
        yield return FadeTo(0f, duration);
        OnFadeInComplete?.Invoke();
    }

    private IEnumerator FadeOut(float duration)
    {
        OnFadeOutStart?.Invoke();
        yield return FadeTo(1f, duration);
        OnFadeOutComplete?.Invoke();
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        Material material = renderer.sharedMaterial;

        if (material == null) yield break;

        float startAlpha = material.color.a;
        float elapsedTime = 0f;
        Color color = material.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            material.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        material.color = new Color(color.r, color.g, color.b, targetAlpha);
    }
}
