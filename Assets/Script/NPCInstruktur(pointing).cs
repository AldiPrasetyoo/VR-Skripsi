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

        "Selamat datang di area simulasi jaringan wireless.\n\n" +
        "Tempat ini merupakan gambaran kondisi dunia nyata dimana kamu akan menyelesaikan permasalahan jaringan.",


        "Sebelum memulai simulasi, perhatikan panduan penggunaan controller VR yang tersedia di sebelah kanan.",


        "Gunakan Trigger untuk memilih tombol pada UI, Grip untuk mengambil atau memindahkan perangkat, dan joystick kanan untuk berpindah menggunakan teleport.",


        "Pada simulasi ini kamu akan diberikan sebuah misi untuk membangun koneksi jaringan Point To Point antar lokasi.",


        "Misi yang harus kamu kerjakan dapat dilihat melalui tab informasi dengan menekan Primary Button pada controller kiri.",


        "Gunakan informasi tersebut sebagai panduan untuk menyelesaikan setiap tahapan konfigurasi jaringan.",


        "Di area ini tersedia beberapa perangkat jaringan seperti NanoStation, antenna, dan perangkat pendukung lainnya.",


        "Lakukan eksplorasi terhadap perangkat yang tersedia dan gunakan pengetahuan yang sudah dipelajari sebelumnya.",


        "Pastikan pemasangan perangkat, arah antenna, serta konfigurasi jaringan dilakukan dengan benar.",


        "Jika sudah memahami instruksi awal, silahkan lanjutkan dan mulai menyelesaikan misi jaringan Point To Point.",



        "Untuk memulai simulasi, silahkan menuju lokasi Gedung 1 yang telah ditandai pada peta.",


        "Setelah sampai di Gedung 1, gunakan titik teleport berwarna hijau untuk menuju area pemasangan di bagian atas gedung.",


        "Pada area Gedung 1 terdapat lokasi pemasangan perangkat dan sebuah tower yang dapat digunakan dalam simulasi.",


        "Tower tersebut memiliki fitur pengaturan ketinggian yang dapat digunakan sesuai kebutuhan simulasi.",

        "Untuk mengatur ketinggian tower, gunakan tombol yang tersedia pada perangkat tower tersebut.",



        "Setelah menyelesaikan tahapan pada Gedung 1, lanjutkan menuju Gedung 2 melalui jalur yang tersedia.",


        "Gunakan peta sebagai panduan untuk menemukan lokasi Gedung 2.",


        "Untuk menuju area pemasangan di Gedung 2, masuk terlebih dahulu ke ruangan lift yang tersedia.",


        "Pencet tombol lift sesuai dengan lantai yang ingin dikunjungi.",



        "Perhatikan setiap perangkat yang tersedia dan gunakan pemahaman jaringan Point To Point untuk menyelesaikan simulasi.",


        "Selamat menyelesaikan simulasi jaringan wireless."

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