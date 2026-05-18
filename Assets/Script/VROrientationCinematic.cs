using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using TMPro;

public class VROrientationCinematic : MonoBehaviour
{
    [Header("References")]
    public PlayableDirector director;
    public GameObject xrOrigin;
    public GameObject cinematicCamera;
    public TextMeshProUGUI subtitleText;
    public CanvasGroup fadeCanvasGroup;

    [Header("Settings")]
    public float fadeDuration = 1f;

    private bool isFinished;

    void Start()
    {
        StartCoroutine(PlayCinematic());
    }

    IEnumerator PlayCinematic()
    {
        isFinished = false;

        if (xrOrigin != null)
            xrOrigin.SetActive(false);

        if (cinematicCamera != null)
            cinematicCamera.SetActive(true);

        if (subtitleText != null)
            subtitleText.text = "";

        yield return StartCoroutine(Fade(1f, 0f));

        if (director != null)
        {
            director.Play();

            while (director.state == PlayState.Playing)
            {
                UpdateSubtitle((float)director.time);
                yield return null;
            }
        }

        yield return StartCoroutine(EndCinematic());
    }

    void UpdateSubtitle(float time)
    {
        if (subtitleText == null) return;

        if (time >= 0f && time < 4f)
        {
            subtitleText.text = "Selamat datang pada simulasi jaringan wireless point to point.";
        }
        else if (time >= 4f && time < 8f)
        {
            subtitleText.text = "Gedung 1 merupakan gedung pusat yang memiliki sumber internet utama.";
        }
        else if (time >= 8f && time < 12f)
        {
            subtitleText.text = "Gedung 2 merupakan gedung cabang yang belum mendapatkan akses internet.";
        }
        else if (time >= 12f && time < 17f)
        {
            subtitleText.text = "Masalah yang harus diselesaikan adalah menghubungkan jaringan dari Gedung 1 ke Gedung 2.";
        }
        else if (time >= 17f && time < 22f)
        {
            subtitleText.text = "Untuk membangun koneksi antar gedung, diperlukan perangkat NanoStation dengan konsep point to point.";
        }
        else if (time >= 22f && time < 27f)
        {
            subtitleText.text = "Perangkat NanoStation harus dipasang pada tower dan mendapatkan sumber daya melalui PoE.";
        }
        else if (time >= 27f && time < 33f)
        {
            subtitleText.text = "Gedung pusat perlu dikonfigurasi sebagai Access Point, sedangkan gedung cabang dikonfigurasi sebagai Station.";
        }
        else if (time >= 33f && time < 38f)
        {
            subtitleText.text = "Selain pemasangan perangkat, konfigurasi IP Address, SSID, keamanan wireless, dan kabel LAN harus dilakukan dengan benar.";
        }
        else if (time >= 38f && time < 43f)
        {
            subtitleText.text = "Keberhasilan jaringan akan diuji melalui koneksi perangkat client di ruang komputer.";
        }
        else if (time >= 43f && time < 48f)
        {
            subtitleText.text = "Tugas Anda adalah menyelesaikan masalah jaringan hingga Gedung 2 berhasil terhubung ke internet.";
        }
        else
        {
            subtitleText.text = "";
        }
    }

    public void SkipCinematic()
    {
        if (isFinished) return;

        if (director != null)
            director.Stop();

        StopAllCoroutines();
        StartCoroutine(EndCinematic());
    }

    IEnumerator EndCinematic()
    {
        isFinished = true;

        if (subtitleText != null)
            subtitleText.text = "";

        yield return StartCoroutine(Fade(0f, 1f));

        if (cinematicCamera != null)
            cinematicCamera.SetActive(false);

        if (xrOrigin != null)
            xrOrigin.SetActive(true);

        yield return StartCoroutine(Fade(1f, 0f));
    }

    IEnumerator Fade(float from, float to)
    {
        if (fadeCanvasGroup == null)
            yield break;

        float timer = 0f;
        fadeCanvasGroup.alpha = from;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(from, to, timer / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = to;
    }
}