using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;


public class NPCInstructorPointing : MonoBehaviour
{

    [Header("UI Dialog")]
    public GameObject dialogPanel;
    public TMP_Text dialogText;
    public Button nextButton;


    [Header("Button Style")]
    public Color normalColor = new Color32(0,168,255,255);
    public Color hoverColor = new Color32(0,217,255,255);

    [Header("Player Lock")]
    public GameObject playerObject;


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
        "Selamat datang di simulasi jaringan wireless. Di sini kamu akan praktik membangun koneksi Point to Point (PtP) seperti di dunia nyata.",
        "Panduan Controller: Gunakan Trigger untuk memilih UI, Grip untuk mengambil barang, dan Joystick Kanan untuk teleportasi.",
        "Tekan Primary Button pada controller kiri untuk membuka tab informasi misi sebagai panduanmu.",
        "Tugasmu adalah memasang dan mengonfigurasi perangkat yang tersedia, seperti NanoStation dan antena dengan benar.",
        "Langkah pertama: Pergi ke Gedung 1 sesuai peta. Gunakan titik teleport hijau untuk naik ke area pemasangan di atap.",
        "Di Gedung 1 terdapat tower untuk memasang perangkat. Gunakan tombol pada tower untuk mengatur ketinggiannya.",
        "Setelah selesai, ikuti peta menuju Gedung 2. Masuk ke lift dan tekan tombol lantai untuk menuju area pemasangan kedua.",
        "Pastikan semua perangkat terpasang, antena terarah dengan tepat, dan jaringan terkonfigurasi. Selamat mencoba!"
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