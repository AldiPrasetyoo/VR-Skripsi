using UnityEngine;
using UnityEngine.Playables;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class InputManagerPlayer : MonoBehaviour
{
    [Header("Timeline")]
    public PlayableDirector mainTimeline;

    [Header("Scene Setelah Cinematic")]
    public string nextSceneName = "NamaSceneSelanjutnya";

    [Header("UI")]
    public GameObject inputNamePanel;
    public TMP_InputField nameInputField;
    public Button startButton;

    [Header("Subtitle & Fade")]
    public GameObject canvasCinematic;
    public TextMeshProUGUI subtitleText;
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1f;

    [Header("Typing Subtitle")]
    public float typingSpeed = 0.035f;

    [Header("Object Awal yang Dimatikan Setelah Mulai")]
    public GameObject xrOrigin;
    public GameObject bg;

    [Header("Camera Object")]
    public GameObject cinematicCamera;
    public GameObject cinemachineKota;
    public GameObject cinemachineGedung1;
    public GameObject cinemachineGedung2;

    public static string playerName;

    private string currentSubtitle = "";
    private Coroutine typingCoroutine;
    private bool cinematicStarted = false;
    private bool cinematicFinished = false;

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

        if (cinemachineGedung1 != null)
            cinemachineGedung1.SetActive(false);

        if (cinemachineGedung2 != null)
            cinemachineGedung2.SetActive(false);

        if (subtitleText != null)
            subtitleText.text = "";

        if (canvasCinematic != null)
            canvasCinematic.SetActive(false);

        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 0f;

        if (mainTimeline != null)
        {
            mainTimeline.Stop();
            mainTimeline.stopped += OnTimelineFinished;
        }

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

        cinematicStarted = true;
        cinematicFinished = false;
        currentSubtitle = "";

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (subtitleText != null)
            subtitleText.text = "";

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

        if (cinemachineGedung1 != null)
            cinemachineGedung1.SetActive(true);

        if (cinemachineGedung2 != null)
            cinemachineGedung2.SetActive(true);

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

        string newSubtitle = "";

        if (time >= 0.5f && time < 12f)
        {
            newSubtitle = "Selamat datang " + PlayerData.playerName +
                ". Perhatikan kondisi jaringan pada area simulasi ini.";
        }
        else if (time >= 12.5f && time < 23f)
        {
            newSubtitle = "Gedung 1 adalah gedung pusat yang memiliki sumber internet utama.";
        }
        else if (time >= 25f && time < 35f)
        {
            newSubtitle = "Gedung 2 adalah gedung cabang yang belum mendapatkan akses internet.";
        }
        else if (time >= 35f && time < 40f)
        {
            newSubtitle = "Kedua gedung harus dihubungkan menggunakan NanoStation secara point to point.";
        }
        else if (time >= 40f && time < 48f)
        {
            newSubtitle = "Selesaikan konfigurasi jaringan hingga Gedung 2 berhasil terhubung ke internet.";
        }

        if (newSubtitle != currentSubtitle)
        {
            currentSubtitle = newSubtitle;

            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = StartCoroutine(TypeSubtitle(newSubtitle));
        }
    }

    private IEnumerator TypeSubtitle(string text)
    {
        subtitleText.text = "";

        if (string.IsNullOrEmpty(text))
            yield break;

        foreach (char letter in text)
        {
            subtitleText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private void OnTimelineFinished(PlayableDirector director)
    {
        if (!cinematicStarted) return;
        if (cinematicFinished) return;
        if (director != mainTimeline) return;

        cinematicFinished = true;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (subtitleText != null)
            subtitleText.text = "";

        StartCoroutine(GoToNextScene());
    }

    private IEnumerator GoToNextScene()
    {
        yield return StartCoroutine(Fade(0f, 1f));

        if (VRSceneTransition.Instance != null)
        {
            VRSceneTransition.Instance.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("VRSceneTransition.Instance tidak ditemukan. Pastikan prefab/script VRSceneTransition ada di scene.");
        }
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