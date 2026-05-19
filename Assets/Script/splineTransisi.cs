using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Splines;

public class SplineFadeImageController : MonoBehaviour
{
    [Header("Spline Animate Kamera")]
    public SplineAnimate splineAnimate;

    [Header("Fade Image")]
    public Image fadeImage;

    [Header("Jumlah Spline")]
    public int jumlahSpline = 4;

    [Header("Durasi Fade")]
    public float fadeOutDuration = 0.4f;
    public float blackHoldDuration = 0.25f;
    public float fadeInDuration = 0.4f;

    [Header("Area Trigger Fade")]
    [Range(0.001f, 0.05f)]
    public float triggerRange = 0.01f;

    private bool[] fadeTriggered;
    private bool isFading;

    private void Start()
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(true);
        }

        fadeTriggered = new bool[Mathf.Max(0, jumlahSpline - 1)];
    }

    private void Update()
    {
        if (splineAnimate == null || fadeImage == null) return;
        if (isFading) return;
        if (jumlahSpline <= 1) return;

        float progress = splineAnimate.NormalizedTime;
        progress = progress % 1f;

        for (int i = 1; i < jumlahSpline; i++)
        {
            float transitionPoint = (float)i / jumlahSpline;

            if (!fadeTriggered[i - 1] && Mathf.Abs(progress - transitionPoint) <= triggerRange)
            {
                fadeTriggered[i - 1] = true;
                StartCoroutine(FadeRoutine());
                break;
            }
        }
    }

    private IEnumerator FadeRoutine()
    {
        isFading = true;

        yield return StartCoroutine(FadeAlpha(0f, 1f, fadeOutDuration));

        yield return new WaitForSeconds(blackHoldDuration);

        yield return StartCoroutine(FadeAlpha(1f, 0f, fadeInDuration));

        isFading = false;
    }

    private IEnumerator FadeAlpha(float from, float to, float duration)
    {
        float timer = 0f;
        Color c = fadeImage.color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            c.a = Mathf.Lerp(from, to, t);
            fadeImage.color = c;

            yield return null;
        }

        c.a = to;
        fadeImage.color = c;
    }
}