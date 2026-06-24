using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;


public class VRPauseMenu : MonoBehaviour
{

    [Header("Pause UI")]
    public GameObject pausePanel;


    [Header("Buttons")]
    public Button continueButton;
    public Button mainMenuButton;



    [Header("XRI Input")]
    public InputActionReference menuButtonAction;



    [Header("Scene")]
    public string mainMenuScene;



    private bool menuOpen = false;




    void Start()
    {

        pausePanel.SetActive(false);


        continueButton.onClick.AddListener(CloseMenu);


        mainMenuButton.onClick.AddListener(OpenMainMenu);


        menuButtonAction.action.Enable();

    }





    void Update()
    {

        if(menuButtonAction.action.WasPressedThisFrame())
        {

            ToggleMenu();

        }

    }







    void ToggleMenu()
    {

        menuOpen = !menuOpen;


        pausePanel.SetActive(menuOpen);


        // Hapus Time.timeScale
        // VR tetap berjalan normal agar ray tidak macet

    }







    public void CloseMenu()
    {

        menuOpen = false;


        pausePanel.SetActive(false);


    }







    public void OpenMainMenu()
    {


        if(VRSceneTransition.Instance != null)
        {

            VRSceneTransition.Instance.LoadScene(mainMenuScene);

        }


    }


}