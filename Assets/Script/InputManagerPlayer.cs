using UnityEngine;
using UnityEngine.Playables;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class InputManagerPlayer : MonoBehaviour
{

    [Header("Skip Cinematic Hold")]
    public InputActionReference skipAction;

    public float holdDuration = 2f;

    private float skipTimer = 0f;
    private bool skipping = false;

    [Header("Timeline")]
    public PlayableDirector mainTimeline;


    [Header("Scene Setelah Cinematic")]
    public string nextSceneName = "NamaSceneSelanjutnya";


    [Header("Subtitle & Fade")]
    public GameObject canvasCinematic;
    public TextMeshProUGUI subtitleText;
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1f;


    [Header("Typing Subtitle")]
    public float typingSpeed = 0.035f;



    [Header("Object Awal")]
    public GameObject xrOrigin;
    public GameObject bg;



    [Header("Camera Object")]
    public GameObject cinematicCamera;
    public GameObject cinemachineKota;
    public GameObject cinemachineGedung1;
    public GameObject cinemachineGedung2;



    public static string playerName = "Player";



    private string currentSubtitle = "";
    private Coroutine typingCoroutine;

    private bool cinematicStarted = false;
    private bool cinematicFinished = false;

    private void Start()
    {


        // matikan player saat cinematic

        if(xrOrigin != null)
            xrOrigin.SetActive(false);



        if(bg != null)
            bg.SetActive(false);



        if(canvasCinematic != null)
            canvasCinematic.SetActive(true);



        if(cinematicCamera != null)
            cinematicCamera.SetActive(true);



        if(cinemachineKota != null)
            cinemachineKota.SetActive(true);



        if(cinemachineGedung1 != null)
            cinemachineGedung1.SetActive(true);



        if(cinemachineGedung2 != null)
            cinemachineGedung2.SetActive(true);



        if(subtitleText != null)
            subtitleText.text = "";



        if(fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 0;



        if(mainTimeline != null)
        {

            mainTimeline.Stop();

            mainTimeline.stopped += OnTimelineFinished;


        }

        if(skipAction != null)
        {
            skipAction.action.Enable();
        }
        // langsung mulai cinematic

        StartMainCinematic();


    }

    private void Update()
    {

        if(mainTimeline != null &&
           mainTimeline.state == PlayState.Playing)
        {

            UpdateSubtitle((float)mainTimeline.time);

        }

        HandleSkip();

    }

     private void SkipCinematic()
    {

        cinematicFinished = true;


        if(mainTimeline != null)
        {
            mainTimeline.Stop();
        }


        if(typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }


        subtitleText.text = "";


        StartCoroutine(GoToNextScene());

    }
    
    private void HandleSkip()
    {

        if(cinematicFinished)
            return;


        if(skipAction == null)
            return;



        if(skipAction.action.IsPressed())
        {

            skipTimer += Time.deltaTime;


            if(skipTimer >= holdDuration && !skipping)
            {

                skipping = true;

                Debug.Log("Cinematic di skip");

                SkipCinematic();

            }

        }
        else
        {

            skipTimer = 0;

        }

    }

    public void StartMainCinematic()
    {
        cinematicStarted = true;
        cinematicFinished = false;
        currentSubtitle = "";

        if(typingCoroutine != null)
        {

            StopCoroutine(typingCoroutine);

            typingCoroutine = null;

        }

        if(mainTimeline != null)
        {

            mainTimeline.time = 0;

            mainTimeline.Play();

        }


        Debug.Log("Cinematic dimulai");

    }

    private void UpdateSubtitle(float time)
    {

        if(subtitleText == null)
            return;

        string newSubtitle = "";

        if(time >= 0.5f && time < 4f)
        {
            newSubtitle = "Selamat datang di simulasi jaringan wireless.";
        }
        else if(time >= 4.5f && time < 13f)
        {
            newSubtitle = "Perhatikan area di sekitar Anda. Tugas Anda adalah menyelesaikan masalah konektivitas antar bangunan.";
        }
        else if(time >= 15.5f && time < 23f)
        {
            newSubtitle = "Lihat Gedung 1 di depan Anda. Gedung ini akan berfungsi sebagai pusat jaringan dan sumber internet utama.";
        }
        else if(time >= 25f && time < 35f)
        {
            newSubtitle = "Namun, Gedung 2 di seberang sana saat ini belum terjangkau oleh jaringan internet.";
        }
        else if(time >= 37f && time < 44f)
        {
            newSubtitle = "Kita perlu menghubungkan kedua gedung ini menggunakan topologi jaringan Point-to-Point.";
        }
        else if(time >= 44f && time < 49.5f)
        {
            newSubtitle = "Selesaikan konfigurasi pada perangkat agar internet dapat terdistribusi ke Gedung 2.";
        }



        if(newSubtitle != currentSubtitle)
        {
            currentSubtitle = newSubtitle;
            if(typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            typingCoroutine =
            StartCoroutine(TypeSubtitle(newSubtitle));

        }
    }

    IEnumerator TypeSubtitle(string text)
    {

        subtitleText.text = "";

        foreach(char letter in text)
        {

            subtitleText.text += letter;
            yield return new WaitForSeconds(typingSpeed);

        }

    }

    private void OnTimelineFinished(PlayableDirector director)
    {

        if(!cinematicStarted)
            return;


        if(cinematicFinished)
            return;


        if(director != mainTimeline)
            return;
        cinematicFinished = true;
        if(typingCoroutine != null)
        {

            StopCoroutine(typingCoroutine);

            typingCoroutine = null;

        }
        subtitleText.text = "";
        StartCoroutine(GoToNextScene());
    }

    IEnumerator GoToNextScene()
    {
        yield return StartCoroutine(Fade(0,1));

        if(VRSceneTransition.Instance != null)
        {
            VRSceneTransition.Instance.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("VRSceneTransition tidak ditemukan");
        }


    }

    IEnumerator Fade(float from,float to)
    {

        if(fadeCanvasGroup == null)
            yield break;
        float timer = 0;
        fadeCanvasGroup.alpha = from;
        while(timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha =
            Mathf.Lerp(
                from,
                to,
                timer / fadeDuration
            );
            yield return null;

        }
        fadeCanvasGroup.alpha = to;
    }
}