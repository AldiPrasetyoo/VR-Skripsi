using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;


public class NPCInstructor : MonoBehaviour
{

    [Header("UI Dialog")]
    public GameObject dialogPanel;
    public TMP_Text dialogText;
    public Button nextButton;
    public AutoDoorTrigger door;
    public DoorEnterScene pindahSceneTrigger;



    [Header("Player Lock")]
    public GameObject playerObject;



    [Header("Button Style")]
    public Color normalColor = new Color32(0,168,255,255);
    public Color hoverColor = new Color32(0,217,255,255);



    [Header("NPC Voice")]
    public AudioSource voiceSource;
    public AudioClip[] voiceClips;



    [Header("Button Audio")]
    public AudioSource clickSource;
    public AudioClip clickSound;

    public AudioSource hoverSource;
    public AudioClip hoverSound;



    private int step = 0;



    private string[] dialog =
    {

        "Selamat datang di ruang pembelajaran jaringan wireless.\n\nUntuk melanjutkan, arahkan laser ke tombol Lanjut lalu tekan tombol depan (jari telunjuk).",


        "Sebelum memulai pembelajaran, kamu perlu mengenal cara berinteraksi menggunakan controller VR.",


        "Untuk mengambil atau memindahkan perangkat, gunakan tombol Grip pada controller.",


        "Untuk memilih tombol atau membaca materi, arahkan laser lalu tekan tombol depan.",


        "Untuk berpindah tempat dapat menggunakan fitur teleport pada controller.",


        "Di ruang ini tersedia materi pembelajaran mengenai jaringan Point To Point (PTP).",


        "Silahkan baca materi pada papan pembelajaran terlebih dahulu.",


        "Setelah memahami materi, eksplor perangkat jaringan yang tersedia seperti NanoStation.",


        "Jika sudah siap, pergi ke arah pintu untuk masuk ke simulasi pembangunan jaringan PTP."

    };





    void Start()
    {

        LockPlayer();


        dialogPanel.SetActive(true);


        nextButton.onClick.AddListener(NextDialog);


        SetupButton();


        ShowDialog();

    }







    void LockPlayer()
    {

        if(playerObject != null)
        {

            playerObject.SetActive(false);

        }


        Debug.Log("Player dikunci");

    }







    void UnlockPlayer()
    {

        if(playerObject != null)
        {

            playerObject.SetActive(true);

        }


        Debug.Log("Player aktif kembali");

    }







    void SetupButton()
    {

        Image img = nextButton.GetComponent<Image>();


        if(img)
        {
            img.color = normalColor;
        }



        EventTrigger trigger = nextButton.gameObject.AddComponent<EventTrigger>();




        // HOVER MASUK

        EventTrigger.Entry enter = new EventTrigger.Entry();

        enter.eventID = EventTriggerType.PointerEnter;



        enter.callback.AddListener((data)=>
        {

            if(img)
            {
                img.color = hoverColor;
            }


            if(hoverSource != null && hoverSound != null)
            {
                hoverSource.PlayOneShot(hoverSound);
            }


        });



        trigger.triggers.Add(enter);






        // HOVER KELUAR

        EventTrigger.Entry exit = new EventTrigger.Entry();

        exit.eventID = EventTriggerType.PointerExit;



        exit.callback.AddListener((data)=>
        {

            if(img)
            {
                img.color = normalColor;
            }


        });



        trigger.triggers.Add(exit);







        // CLICK SOUND

        nextButton.onClick.AddListener(()=>
        {

            if(clickSource != null && clickSound != null)
            {

                clickSource.PlayOneShot(clickSound);

            }


        });


    }








    void NextDialog()
    {

        step++;



        if(step >= dialog.Length)
        {

            dialogPanel.SetActive(false);

            UnlockPlayer();


            if(door != null)
            {
                door.ActivateDoor();
            }
            
            if(pindahSceneTrigger != null)
            {
                pindahSceneTrigger.ActivateDoor();
            }


            return;

        }



        ShowDialog();


    }









    void ShowDialog()
    {


        dialogText.text = dialog[step];



        // PLAY SUARA SESUAI DIALOG

        if(voiceSource != null && voiceClips.Length > step)
        {

            voiceSource.Stop();


            voiceSource.clip = voiceClips[step];


            voiceSource.Play();

        }


    }



}