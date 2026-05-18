using UnityEngine;
using UnityEngine.Playables;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class InputManagerPlayer : MonoBehaviour
{
    [Header("Timeline")]
    public PlayableDirector mainTimeline;

    [Header("UI")]
    public GameObject inputNamePanel;
    public TMP_InputField nameInputField;
    public Button startButton;

    [Header("Subtitle & Fade")]
    public GameObject canvasCinematic;
    public TextMeshProUGUI subtitleText;
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1f;

    [Header("Object Awal yang Dimatikan Setelah Mulai")]
    public GameObject xrOrigin;
    public GameObject bg;

    [Header("Camera Object")]
    public GameObject cinematicCamera;
    public GameObject cinemachineKota;
    public GameObject cinemachineGedung;

    public static string playerName;

    private void Start()
    {
        if (xrOrigin != null)
            xrOrigin.SetActive(true);

        if (bg != null)
            bg.SetActive(true);

        if (inputNamePanel != null)
            inputNamePanel.SetActive(true);

        if (cinematicCamera != null)
            cinematicCamera.SetActive(false);

        if (cinemachineKota != null)
            cinemachineKota.SetActive(false);

        if (cinemachineGedung != null)
            cinemachineGedung.SetActive(false);

        if (subtitleText != null)
            subtitleText.text = "";

        if(canvasCinematic != null)
            canvasCinematic.SetActive(false);

        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 0f;

        if (mainTimeline != null)
            mainTimeline.Stop();

        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(StartMainCinematic);
        }
    }

    private void Update()
    {
        if (mainTimeline != null && mainTimeline.state == PlayState.Playing)
        {
            UpdateSubtitle((float)mainTimeline.time);
        }
    }

    public void StartMainCinematic()
    {
        if (nameInputField == null) return;

        string inputName = nameInputField.text.Trim();

        if (string.IsNullOrEmpty(inputName))
        {
            Debug.Log("Nama belum diisi.");
            return;
        }

        playerName = inputName;
        PlayerData.playerName = inputName;

        PlayerPrefs.SetString("PlayerName", inputName);
        PlayerPrefs.Save();

        if (inputNamePanel != null)
            inputNamePanel.SetActive(false);

        if (bg != null)
            bg.SetActive(false);

        if (xrOrigin != null)
            xrOrigin.SetActive(false);

        if (canvasCinematic != null)
            canvasCinematic.SetActive(true);

        if (cinematicCamera != null)
            cinematicCamera.SetActive(true);

        if (cinemachineKota != null)
            cinemachineKota.SetActive(true);



        if (cinemachineGedung != null)
            cinemachineGedung.SetActive(true);

        if (mainTimeline != null)
        {
            mainTimeline.time = 0;
            mainTimeline.Play();
        }

        Debug.Log("Nama player tersimpan: " + inputName);
    }

    private void UpdateSubtitle(float time)
    {
        if (subtitleText == null) return;

        if (time >= 0f && time < 4f)
            subtitleText.text = "Selamat datang pada simulasi jaringan wireless point to point.";
        else if (time >= 4f && time < 8f)
            subtitleText.text = "Gedung 1 merupakan gedung pusat yang memiliki sumber internet utama.";
        else if (time >= 8f && time < 12f)
            subtitleText.text = "Gedung 2 merupakan gedung cabang yang belum mendapatkan akses internet.";
        else if (time >= 12f && time < 17f)
            subtitleText.text = "Masalah yang harus diselesaikan adalah menghubungkan jaringan dari Gedung 1 ke Gedung 2.";
        else if (time >= 17f && time < 22f)
            subtitleText.text = "Untuk membangun koneksi antar gedung, diperlukan perangkat NanoStation dengan konsep point to point.";
        else if (time >= 22f && time < 27f)
            subtitleText.text = "Perangkat NanoStation harus dipasang pada tower dan mendapatkan sumber daya melalui PoE.";
        else if (time >= 27f && time < 33f)
            subtitleText.text = "Gedung pusat perlu dikonfigurasi sebagai Access Point, sedangkan gedung cabang dikonfigurasi sebagai Station.";
        else if (time >= 33f && time < 38f)
            subtitleText.text = "Selain pemasangan perangkat, konfigurasi IP Address, SSID, keamanan wireless, dan kabel LAN harus dilakukan dengan benar.";
        else if (time >= 38f && time < 43f)
            subtitleText.text = "Keberhasilan jaringan akan diuji melalui koneksi perangkat client di ruang komputer.";
        else if (time >= 43f && time < 48f)
            subtitleText.text = "Tugas Anda adalah menyelesaikan masalah jaringan hingga Gedung 2 berhasil terhubung ke internet.";
        else
            subtitleText.text = "";
    }

    private IEnumerator Fade(float from, float to)
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