using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class VRSceneTransition : MonoBehaviour
{
    public static VRSceneTransition Instance;

    [Header("UI Fade")]
    public Image fadeImage;
    public float fadeDuration = 1f;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }



    private void Start()
    {
        StartCoroutine(FadeIn());
    }



    public void LoadScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }



    IEnumerator FadeAndLoad(string sceneName)
    {

        yield return StartCoroutine(FadeOut());


        AsyncOperation asyncLoad =
        SceneManager.LoadSceneAsync(sceneName);



        while (!asyncLoad.isDone)
        {
            yield return null;
        }



        yield return StartCoroutine(FadeIn());



        // destroy setelah selesai pindah scene
        Destroy(gameObject);

    }




    IEnumerator FadeOut()
    {

        float t = 0;


        while (t < fadeDuration)
        {

            t += Time.deltaTime;


            fadeImage.color =
            new Color(0,0,0,t / fadeDuration);


            yield return null;

        }

    }





    IEnumerator FadeIn()
    {

        float t = fadeDuration;


        while (t > 0)
        {

            t -= Time.deltaTime;


            fadeImage.color =
            new Color(0,0,0,t / fadeDuration);


            yield return null;

        }

    }





    public void ExitApp()
    {

        StartCoroutine(ExitRoutine());

    }



    IEnumerator ExitRoutine()
    {

        yield return StartCoroutine(FadeOut());


        Application.Quit();

    }

}