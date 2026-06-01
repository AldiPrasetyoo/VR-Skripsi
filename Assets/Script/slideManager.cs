using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlideImageController : MonoBehaviour
{
    [Header("UI Gambar di Papan Tulis")]
    public Image slideImage;

    [Header("Daftar Gambar Slide")]
    public Sprite[] slides;

    [Header("Button")]
    public Button nextButton;
    public Button prevButton;
    public Button backButton;

    [Header("Teks Tombol Next")]
    public TMP_Text nextButtonText;

    [Header("Icon Tombol Next")]
    public GameObject nextButtonIcon;

    [Header("Panel")]
    public GameObject panelSlideMateri;
    public GameObject panelPilihanPembelajaran;

    private int currentIndex = 0;

    private void Start()
    {
        ShowSlide();

        if (nextButton != null)
            nextButton.onClick.AddListener(NextSlide);

        if (prevButton != null)
            prevButton.onClick.AddListener(PrevSlide);

        if (backButton != null)
            backButton.onClick.AddListener(KembaliKePilihanPembelajaran);
    }

    public void NextSlide()
    {
        if (slides.Length == 0) return;

        if (currentIndex >= slides.Length - 1)
        {
            KembaliKePilihanPembelajaran();
            return;
        }

        currentIndex++;
        ShowSlide();
    }

    public void PrevSlide()
    {
        if (slides.Length == 0) return;

        currentIndex--;

        if (currentIndex < 0)
            currentIndex = 0;

        ShowSlide();
    }

    public void KembaliKePilihanPembelajaran()
    {
        if (panelSlideMateri != null)
            panelSlideMateri.SetActive(false);

        if (panelPilihanPembelajaran != null)
            panelPilihanPembelajaran.SetActive(true);

        currentIndex = 0;
        ShowSlide();
    }

    private void ShowSlide()
    {
        if (slideImage == null || slides.Length == 0) return;

        slideImage.sprite = slides[currentIndex];

        if (prevButton != null)
            prevButton.interactable = currentIndex > 0;

        bool isLastSlide = currentIndex >= slides.Length - 1;

        if (nextButtonText != null)
        {
            if (isLastSlide)
                nextButtonText.text = "Selesai";
            else
                nextButtonText.text = "Selanjutnya";
        }

        if (nextButtonIcon != null)
            nextButtonIcon.SetActive(!isLastSlide);
    }
}