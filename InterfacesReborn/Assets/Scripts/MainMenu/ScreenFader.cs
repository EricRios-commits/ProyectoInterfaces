using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public CanvasGroup fadeCanvasGroup;

    private void Awake()
    {
        // Asegurarse de que el canvas empiece invisible
        fadeCanvasGroup.alpha = 0f;
    }

    public void FadeIn(float duration = 1f)
    {
        StartCoroutine(Fade(0, 1, duration));
    }

    public void FadeOut(float duration = 1f)
    {
        StartCoroutine(Fade(1, 0, duration));
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            fadeCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        fadeCanvasGroup.alpha = to;
    }
}

