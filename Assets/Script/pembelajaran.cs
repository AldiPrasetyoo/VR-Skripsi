using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class VRLearningUIController : MonoBehaviour
{
    [System.Serializable]
    public class NavPage
    {
        [Header("Nama Halaman")]
        public string pageName;

        [Header("Button Navbar")]
        public Button button;

        [Header("Panel Halaman")]
        public GameObject pagePanel;

        [Header("Tampilan Navbar Optional")]
        public Image buttonBackground;
        public Image iconImage;
        public TMP_Text labelText;
        public Image dividerLine;
    }

    [System.Serializable]
    public class MateriSubTopic
    {
        [Header("Nama Sub Materi")]
        public string subTopicName;

        [Header("Button di Menu Sub Materi")]
        public Button subButton;

        [Header("Slide Sub Materi Ini")]
        public GameObject[] slides;
    }

    [System.Serializable]
    public class MateriTopic
    {
        [Header("Nama Materi")]
        public string topicName;

        [Header("Button Card di Kotak Utama")]
        public Button cardButton;

        [Header("Mode Sub Menu")]
        public bool useSubMenu;

        [Header("Untuk Materi Biasa")]
        public GameObject[] slides;

        [Header("Untuk Materi yang Punya Menu Lagi")]
        public GameObject subMenuSlide;
        public Button subBackButton;
        public Button subContinueButton;
        public TMP_Text subContinueButtonText;
        public MateriSubTopic[] subTopics;
    }

    [System.Serializable]
    public class JumpButton
    {
        [Header("Button yang Dipencet")]
        public Button button;

        [Header("Tujuan Materi")]
        public int targetTopicIndex;

        [Header("Tujuan Slide di Materi Tersebut")]
        public int targetSlideIndex;
    }

    [Header("Daftar Navbar")]
    public NavPage[] pages;

    [Header("Nama Halaman")]
    public string homePageName = "Home";
    public string materiPageName = "Materi";

    [Header("Panel Dalam Materi")]
    public GameObject materiIntroPanel;
    public GameObject materiSlidePanel;

    [Header("Tombol Materi Intro")]
    public Button kembaliButton;
    public Button mulaiButton;
    public TMP_Text mulaiButtonText;

    [Header("Tulisan Tombol Materi Intro")]
    public string textMulai = "Mulai";
    public string textLanjutkan = "Lanjutkan";
    public string textUlangi = "Ulangi";

    [Header("Daftar Materi dan Slide")]
    public MateriTopic[] materiTopics;

    [Header("Tombol Loncat Khusus Optional")]
    public JumpButton[] jumpButtons;

    [Header("Tombol Navigasi Slide")]
    public Button sebelumButton;
    public Button lanjutButton;

    [Header("Text Tombol Navigasi")]
    public TMP_Text sebelumButtonText;
    public TMP_Text lanjutButtonText;

    [Header("Tulisan Tombol Slide")]
    public string textKembaliKeIntro = "Kembali";
    public string textSebelumnya = "Sebelumnya";
    public string textBerikutnya = "Berikutnya";
    public string textFinish = "Finish";

    [Header("Tulisan Tombol Sub Menu")]
    public string textSubMulai = "Mulai";
    public string textSubLanjutkan = "Lanjutkan";
    public string textSubSelesai = "Selesai";
    public string textSubUlangi = "Ulangi";

    [Header("Progress Materi Per Bagian")]
    public GameObject progressRoot;
    public Image progressFillImage;
    public TMP_Text progressPercentText;
    public TMP_Text slideCounterText;
    public TMP_Text topicTitleText;
    public TMP_Text topicSlideCounterText;

    [Header("Pengaturan Menu")]
    public bool hideSlideNavigationOnSubMenu = true;
    public bool hideProgressOnSubMenu = true;

    [Header("Warna Aktif Neon")]
    public Color activeColor = new Color(0f, 0.72f, 1f, 1f);
    public Color activeBackgroundColor = new Color(0f, 0.72f, 1f, 0.18f);

    [Header("Warna Hover Navbar Aktif")]
    public Color activeHoverColor = new Color(0f, 1f, 1f, 1f);
    public Color activeHoverBackgroundColor = new Color(0f, 0.9f, 1f, 0.28f);
    public Color activeHoverLineColor = new Color(0f, 1f, 1f, 1f);
    public float activeHoverScale = 1.12f;

    [Header("Warna Hover Navbar Nonaktif")]
    public bool useNavbarHoverEffect = true;
    public Color hoverColor = new Color(0f, 0.9f, 1f, 1f);
    public Color hoverBackgroundColor = new Color(0f, 0.72f, 1f, 0.10f);
    public Color hoverLineColor = new Color(0f, 0.72f, 1f, 0.9f);
    public float hoverScale = 1.04f;

    [Header("Warna Tidak Aktif")]
    public Color inactiveTextColor = new Color(0.85f, 0.92f, 1f, 1f);
    public Color inactiveIconColor = new Color(0.75f, 0.85f, 0.95f, 1f);
    public Color inactiveLineColor = new Color(0.15f, 0.25f, 0.32f, 0.8f);
    public Color inactiveBackgroundColor = new Color(0f, 0f, 0f, 0f);

    [Header("Efek Scale Navbar")]
    public bool useScaleEffect = true;
    public float activeScale = 1.08f;
    public float inactiveScale = 1f;

    [Header("Object yang Tidak Boleh Dimatikan")]
    public GameObject[] protectedObjects;

    [Header("Debug Panel")]
    public bool enablePanelDebugLog = false;

    // ================= SOUND TAMBAHAN =================

    [Header("UI Sound")]
    public bool enableUISound = true;
    public bool autoCreateAudioSource = true;
    public bool autoRegisterAllChildButtons = true;

    [Tooltip("Kalau ada button yang bukan child dari object ini, masukkan ke sini.")]
    public Button[] extraSoundButtons;

    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Audio Clip")]
    public AudioClip hoverClip;
    public AudioClip clickClip;
    public AudioClip pageOpenClip;
    public AudioClip slideChangeClip;
    public AudioClip backClip;
    public AudioClip finishClip;
    public AudioClip errorClip;

    [Header("Volume Sound")]
    [Range(0f, 1f)] public float hoverVolume = 0.65f;
    [Range(0f, 1f)] public float clickVolume = 0.85f;
    [Range(0f, 1f)] public float pageOpenVolume = 0.75f;
    [Range(0f, 1f)] public float slideChangeVolume = 0.75f;
    [Range(0f, 1f)] public float backVolume = 0.75f;
    [Range(0f, 1f)] public float finishVolume = 0.85f;
    [Range(0f, 1f)] public float errorVolume = 0.85f;

    [Header("Hover Sound Setting")]
    public float hoverSoundCooldown = 0.08f;

    private readonly HashSet<Button> registeredSoundButtons = new HashSet<Button>();
    private float lastHoverSoundTime = -999f;
    private bool isInitializing = true;

    // ================= RUNTIME =================

    private int currentPageIndex = -1;

    private int currentTopicIndex = 0;
    private int currentSlideIndex = 0;

    private int currentSubTopicIndex = -1;
    private int currentSubSlideIndex = 0;

    private bool isInSubTopic = false;

    private bool[] completedTopics;
    private bool[][] completedSubTopics;
    private bool[] navHoverStates;

    private void Start()
    {
        isInitializing = true;

        SetupAudioSource();

        AutoFindButtonTexts();
        InitCompletedData();
        InitNavbarHover();

        SetupNavbarButtons();
        SetupMateriButtons();
        SetupCardButtons();
        SetupSubTopicButtons();
        SetupJumpButtons();
        SetupSlideButtons();

        RegisterAllButtonSounds();

        ShowPageByName(homePageName);
        ShowMateriIntro();

        isInitializing = false;
    }

    // ================= SOUND SYSTEM =================

    private void SetupAudioSource()
    {
        if (!enableUISound) return;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null && autoCreateAudioSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
        }
    }

    private void RegisterAllButtonSounds()
    {
        if (!enableUISound) return;

        registeredSoundButtons.Clear();

        if (pages != null)
        {
            for (int i = 0; i < pages.Length; i++)
            {
                if (pages[i] != null)
                {
                    RegisterButtonSound(pages[i].button);
                }
            }
        }

        RegisterButtonSound(kembaliButton);
        RegisterButtonSound(mulaiButton);
        RegisterButtonSound(sebelumButton);
        RegisterButtonSound(lanjutButton);

        if (materiTopics != null)
        {
            for (int i = 0; i < materiTopics.Length; i++)
            {
                if (materiTopics[i] == null) continue;

                RegisterButtonSound(materiTopics[i].cardButton);
                RegisterButtonSound(materiTopics[i].subBackButton);
                RegisterButtonSound(materiTopics[i].subContinueButton);

                if (materiTopics[i].subTopics != null)
                {
                    for (int j = 0; j < materiTopics[i].subTopics.Length; j++)
                    {
                        if (materiTopics[i].subTopics[j] == null) continue;

                        RegisterButtonSound(materiTopics[i].subTopics[j].subButton);
                    }
                }
            }
        }

        if (jumpButtons != null)
        {
            for (int i = 0; i < jumpButtons.Length; i++)
            {
                if (jumpButtons[i] == null) continue;

                RegisterButtonSound(jumpButtons[i].button);
            }
        }

        if (extraSoundButtons != null)
        {
            for (int i = 0; i < extraSoundButtons.Length; i++)
            {
                RegisterButtonSound(extraSoundButtons[i]);
            }
        }

        if (autoRegisterAllChildButtons)
        {
            Button[] childButtons = GetComponentsInChildren<Button>(true);

            for (int i = 0; i < childButtons.Length; i++)
            {
                RegisterButtonSound(childButtons[i]);
            }
        }
    }

    private void RegisterButtonSound(Button button)
    {
        if (!enableUISound) return;
        if (button == null) return;

        if (registeredSoundButtons.Contains(button))
        {
            return;
        }

        registeredSoundButtons.Add(button);

        button.onClick.AddListener(() =>
        {
            PlayClickSound();
        });

        EventTrigger trigger = button.GetComponent<EventTrigger>();

        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) =>
        {
            PlayHoverSound();
        });

        trigger.triggers.Add(enterEntry);
    }

    private void PlayHoverSound()
    {
        if (!enableUISound) return;

        if (Time.unscaledTime - lastHoverSoundTime < hoverSoundCooldown)
        {
            return;
        }

        lastHoverSoundTime = Time.unscaledTime;

        PlaySound(hoverClip, hoverVolume);
    }

    private void PlayClickSound()
    {
        PlaySound(clickClip, clickVolume);
    }

    private void PlayPageOpenSound()
    {
        PlaySound(pageOpenClip, pageOpenVolume);
    }

    private void PlaySlideChangeSound()
    {
        PlaySound(slideChangeClip, slideChangeVolume);
    }

    private void PlayBackSound()
    {
        PlaySound(backClip, backVolume);
    }

    private void PlayFinishSound()
    {
        PlaySound(finishClip, finishVolume);
    }

    private void PlayErrorSound()
    {
        PlaySound(errorClip, errorVolume);
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (!enableUISound) return;
        if (audioSource == null) return;
        if (clip == null) return;

        audioSource.PlayOneShot(clip, volume);
    }

    // ================= INIT =================

    private void AutoFindButtonTexts()
    {
        if (mulaiButtonText == null && mulaiButton != null)
            mulaiButtonText = mulaiButton.GetComponentInChildren<TMP_Text>(true);

        if (sebelumButtonText == null && sebelumButton != null)
            sebelumButtonText = sebelumButton.GetComponentInChildren<TMP_Text>(true);

        if (lanjutButtonText == null && lanjutButton != null)
            lanjutButtonText = lanjutButton.GetComponentInChildren<TMP_Text>(true);

        if (materiTopics == null) return;

        for (int i = 0; i < materiTopics.Length; i++)
        {
            if (materiTopics[i].subContinueButtonText == null && materiTopics[i].subContinueButton != null)
            {
                materiTopics[i].subContinueButtonText =
                    materiTopics[i].subContinueButton.GetComponentInChildren<TMP_Text>(true);
            }
        }
    }

    private void InitCompletedData()
    {
        if (materiTopics == null)
        {
            completedTopics = new bool[0];
            completedSubTopics = new bool[0][];
            return;
        }

        completedTopics = new bool[materiTopics.Length];
        completedSubTopics = new bool[materiTopics.Length][];

        for (int i = 0; i < materiTopics.Length; i++)
        {
            if (materiTopics[i].subTopics != null)
                completedSubTopics[i] = new bool[materiTopics[i].subTopics.Length];
            else
                completedSubTopics[i] = new bool[0];
        }
    }

    private void InitNavbarHover()
    {
        if (pages == null)
        {
            navHoverStates = new bool[0];
            return;
        }

        navHoverStates = new bool[pages.Length];
    }

    // ================= SETUP BUTTON =================

    private void SetupNavbarButtons()
    {
        if (pages == null) return;

        for (int i = 0; i < pages.Length; i++)
        {
            int index = i;

            if (pages[i].button != null)
            {
                pages[i].button.onClick.RemoveAllListeners();
                pages[i].button.onClick.AddListener(() =>
                {
                    ShowPage(index);

                    if (IsSamePage(pages[index].pageName, materiPageName))
                    {
                        ShowMateriIntro();
                    }
                });

                AddNavbarHoverEvent(pages[i].button, index);
            }
        }
    }

    private void AddNavbarHoverEvent(Button button, int index)
    {
        if (button == null) return;

        EventTrigger trigger = button.GetComponent<EventTrigger>();

        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) =>
        {
            SetNavbarHover(index, true);
        });

        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) =>
        {
            SetNavbarHover(index, false);
        });

        trigger.triggers.Add(enterEntry);
        trigger.triggers.Add(exitEntry);
    }

    private void SetNavbarHover(int index, bool isHover)
    {
        if (!useNavbarHoverEffect) return;
        if (pages == null) return;
        if (index < 0 || index >= pages.Length) return;

        if (navHoverStates == null || navHoverStates.Length != pages.Length)
            InitNavbarHover();

        navHoverStates[index] = isHover;

        bool isActive = index == currentPageIndex;

        UpdateNavbarVisual(pages[index], isActive, isHover);
    }

    private void SetupMateriButtons()
    {
        if (kembaliButton != null)
        {
            kembaliButton.onClick.RemoveAllListeners();
            kembaliButton.onClick.AddListener(KembaliKeHome);
        }

        if (mulaiButton != null)
        {
            mulaiButton.onClick.RemoveAllListeners();
            mulaiButton.onClick.AddListener(MulaiAtauLanjutkan);
        }
    }

    private void SetupCardButtons()
    {
        if (materiTopics == null) return;

        for (int i = 0; i < materiTopics.Length; i++)
        {
            int topicIndex = i;

            if (materiTopics[i].cardButton != null)
            {
                materiTopics[i].cardButton.onClick.RemoveAllListeners();
                materiTopics[i].cardButton.onClick.AddListener(() =>
                {
                    BukaMateriLangsung(topicIndex);
                });
            }
        }
    }

    private void SetupSubTopicButtons()
    {
        if (materiTopics == null) return;

        for (int topicIndex = 0; topicIndex < materiTopics.Length; topicIndex++)
        {
            int capturedTopicIndex = topicIndex;

            if (materiTopics[topicIndex].subBackButton != null)
            {
                materiTopics[topicIndex].subBackButton.onClick.RemoveAllListeners();
                materiTopics[topicIndex].subBackButton.onClick.AddListener(() =>
                {
                    ShowMateriIntro();

                    if (!isInitializing)
                    {
                        PlayBackSound();
                    }
                });
            }

            if (materiTopics[topicIndex].subContinueButton != null)
            {
                materiTopics[topicIndex].subContinueButton.onClick.RemoveAllListeners();
                materiTopics[topicIndex].subContinueButton.onClick.AddListener(() =>
                {
                    OnSubContinueClicked(capturedTopicIndex);
                });
            }

            if (materiTopics[topicIndex].subTopics == null) continue;

            for (int subIndex = 0; subIndex < materiTopics[topicIndex].subTopics.Length; subIndex++)
            {
                int capturedSubIndex = subIndex;

                Button btn = materiTopics[topicIndex].subTopics[subIndex].subButton;

                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() =>
                    {
                        BukaSubMateriLangsung(capturedTopicIndex, capturedSubIndex);
                    });
                }
            }
        }
    }

    private void SetupJumpButtons()
    {
        if (jumpButtons == null) return;

        for (int i = 0; i < jumpButtons.Length; i++)
        {
            int index = i;

            if (jumpButtons[i].button != null)
            {
                jumpButtons[i].button.onClick.RemoveAllListeners();
                jumpButtons[i].button.onClick.AddListener(() =>
                {
                    JumpToSlide(
                        jumpButtons[index].targetTopicIndex,
                        jumpButtons[index].targetSlideIndex
                    );
                });
            }
        }
    }

    private void SetupSlideButtons()
    {
        if (sebelumButton != null)
        {
            sebelumButton.onClick.RemoveAllListeners();
            sebelumButton.onClick.AddListener(OnSebelumButtonClicked);
        }

        if (lanjutButton != null)
        {
            lanjutButton.onClick.RemoveAllListeners();
            lanjutButton.onClick.AddListener(OnLanjutButtonClicked);
        }
    }

    // ================= NAVIGATION =================

    public void KembaliKeHome()
    {
        ShowPageByName(homePageName);
        ShowMateriIntro();

        if (!isInitializing)
        {
            PlayBackSound();
        }
    }

    public void MulaiAtauLanjutkan()
    {
        if (materiTopics == null || materiTopics.Length == 0)
        {
            PlayErrorSound();
            return;
        }

        if (IsAllTopicsCompleted())
        {
            ResetProgressMateri();
            BukaMateriLangsung(0);
            return;
        }

        int nextTopic = GetFirstIncompleteTopicIndex();
        BukaMateriLangsung(nextTopic);
    }

    public void BukaMateriLangsung(int topicIndex)
    {
        if (materiTopics == null || materiTopics.Length == 0)
        {
            PlayErrorSound();
            return;
        }

        topicIndex = Mathf.Clamp(topicIndex, 0, materiTopics.Length - 1);

        ShowPageByName(materiPageName);
        ShowMateriSlidePanel();

        if (materiTopics[topicIndex].useSubMenu)
        {
            ShowSubMenu(topicIndex);
        }
        else
        {
            ShowNormalSlide(topicIndex, 0);
        }

        if (!isInitializing)
        {
            PlaySlideChangeSound();
        }
    }

    public void JumpToSlide(int topicIndex, int slideIndex)
    {
        if (materiTopics == null || materiTopics.Length == 0)
        {
            PlayErrorSound();
            return;
        }

        topicIndex = Mathf.Clamp(topicIndex, 0, materiTopics.Length - 1);

        if (materiTopics[topicIndex].useSubMenu)
        {
            ShowSubMenu(topicIndex);

            if (!isInitializing)
            {
                PlaySlideChangeSound();
            }

            return;
        }

        ShowPageByName(materiPageName);
        ShowMateriSlidePanel();
        ShowNormalSlide(topicIndex, slideIndex);

        if (!isInitializing)
        {
            PlaySlideChangeSound();
        }
    }

    public void BukaSubMateriLangsung(int topicIndex, int subTopicIndex)
    {
        if (!IsValidSubTopic(topicIndex, subTopicIndex))
        {
            PlayErrorSound();
            return;
        }

        ShowPageByName(materiPageName);
        ShowMateriSlidePanel();
        ShowSubSlide(topicIndex, subTopicIndex, 0);

        if (!isInitializing)
        {
            PlaySlideChangeSound();
        }
    }

    private void OnSubContinueClicked(int topicIndex)
    {
        if (topicIndex < 0 || topicIndex >= materiTopics.Length)
        {
            PlayErrorSound();
            return;
        }

        if (AreAllSubTopicsCompleted(topicIndex))
        {
            ShowMateriIntro();
            PlayFinishSound();
            return;
        }

        int nextSub = GetFirstIncompleteSubTopicIndex(topicIndex);
        BukaSubMateriLangsung(topicIndex, nextSub);
    }

    private void OnSebelumButtonClicked()
    {
        if (isInSubTopic)
        {
            if (IsFirstSlideInCurrentSubTopic())
            {
                ShowSubMenu(currentTopicIndex);
                PlayBackSound();
            }
            else
            {
                ShowSubSlide(currentTopicIndex, currentSubTopicIndex, currentSubSlideIndex - 1);
                PlaySlideChangeSound();
            }
        }
        else
        {
            if (IsFirstSlideInCurrentTopic())
            {
                ShowMateriIntro();
                PlayBackSound();
            }
            else
            {
                ShowNormalSlide(currentTopicIndex, currentSlideIndex - 1);
                PlaySlideChangeSound();
            }
        }
    }

    private void OnLanjutButtonClicked()
    {
        if (isInSubTopic)
        {
            if (IsLastSlideInCurrentSubTopic())
            {
                FinishCurrentSubTopic();
                PlayFinishSound();
            }
            else
            {
                ShowSubSlide(currentTopicIndex, currentSubTopicIndex, currentSubSlideIndex + 1);
                PlaySlideChangeSound();
            }
        }
        else
        {
            if (IsLastSlideInCurrentTopic())
            {
                FinishCurrentTopic();
                PlayFinishSound();
            }
            else
            {
                ShowNormalSlide(currentTopicIndex, currentSlideIndex + 1);
                PlaySlideChangeSound();
            }
        }
    }

    private void FinishCurrentTopic()
    {
        if (completedTopics != null &&
            currentTopicIndex >= 0 &&
            currentTopicIndex < completedTopics.Length)
        {
            completedTopics[currentTopicIndex] = true;
        }

        ShowMateriIntro();
    }

    private void FinishCurrentSubTopic()
    {
        if (completedSubTopics != null &&
            currentTopicIndex >= 0 &&
            currentTopicIndex < completedSubTopics.Length &&
            currentSubTopicIndex >= 0 &&
            currentSubTopicIndex < completedSubTopics[currentTopicIndex].Length)
        {
            completedSubTopics[currentTopicIndex][currentSubTopicIndex] = true;
        }

        ShowSubMenu(currentTopicIndex);
    }

    // ================= SHOW PANEL / SLIDE =================

    private void ShowMateriSlidePanel()
    {
        if (materiIntroPanel != null)
            SetActiveWithLog(materiIntroPanel, false);

        if (materiSlidePanel != null)
            SetActiveWithLog(materiSlidePanel, true);
    }

    public void ShowMateriIntro()
    {
        isInSubTopic = false;

        if (materiIntroPanel != null)
            SetActiveWithLog(materiIntroPanel, true);

        if (materiSlidePanel != null)
            SetActiveWithLog(materiSlidePanel, false);

        HideAllSlides();
        UpdateMulaiButtonText();
    }

    private void ShowSubMenu(int topicIndex)
    {
        if (materiTopics == null || materiTopics.Length == 0)
        {
            PlayErrorSound();
            return;
        }

        topicIndex = Mathf.Clamp(topicIndex, 0, materiTopics.Length - 1);

        currentTopicIndex = topicIndex;
        currentSlideIndex = 0;
        currentSubTopicIndex = -1;
        currentSubSlideIndex = 0;
        isInSubTopic = false;

        HideAllSlides();

        if (materiTopics[topicIndex].subMenuSlide != null)
            SetActiveWithLog(materiTopics[topicIndex].subMenuSlide, true);

        SetSlideNavigationVisible(!hideSlideNavigationOnSubMenu);
        SetProgressVisible(!hideProgressOnSubMenu);

        UpdateSubContinueButton(topicIndex);
        UpdateTopicTextForSubMenu(topicIndex);
    }

    private void ShowNormalSlide(int topicIndex, int slideIndex)
    {
        if (materiTopics == null || materiTopics.Length == 0)
        {
            PlayErrorSound();
            return;
        }

        topicIndex = Mathf.Clamp(topicIndex, 0, materiTopics.Length - 1);

        if (materiTopics[topicIndex].slides == null || materiTopics[topicIndex].slides.Length == 0)
        {
            Debug.LogWarning("Materi belum punya slide: " + materiTopics[topicIndex].topicName);
            PlayErrorSound();
            return;
        }

        slideIndex = Mathf.Clamp(slideIndex, 0, materiTopics[topicIndex].slides.Length - 1);

        currentTopicIndex = topicIndex;
        currentSlideIndex = slideIndex;
        currentSubTopicIndex = -1;
        currentSubSlideIndex = 0;
        isInSubTopic = false;

        HideAllSlides();

        GameObject activeSlide = materiTopics[currentTopicIndex].slides[currentSlideIndex];

        if (activeSlide != null)
            SetActiveWithLog(activeSlide, true);

        SetSlideNavigationVisible(true);
        SetProgressVisible(true);

        UpdateSlideButtons();
        UpdateProgressPerTopic();
        UpdateTopicTextNormal();
    }

    private void ShowSubSlide(int topicIndex, int subTopicIndex, int slideIndex)
    {
        if (!IsValidSubTopic(topicIndex, subTopicIndex))
        {
            PlayErrorSound();
            return;
        }

        GameObject[] slides = materiTopics[topicIndex].subTopics[subTopicIndex].slides;

        if (slides == null || slides.Length == 0)
        {
            Debug.LogWarning("Sub materi belum punya slide: " + materiTopics[topicIndex].subTopics[subTopicIndex].subTopicName);
            PlayErrorSound();
            return;
        }

        slideIndex = Mathf.Clamp(slideIndex, 0, slides.Length - 1);

        currentTopicIndex = topicIndex;
        currentSubTopicIndex = subTopicIndex;
        currentSubSlideIndex = slideIndex;
        currentSlideIndex = 0;
        isInSubTopic = true;

        HideAllSlides();

        GameObject activeSlide = slides[currentSubSlideIndex];

        if (activeSlide != null)
            SetActiveWithLog(activeSlide, true);

        SetSlideNavigationVisible(true);
        SetProgressVisible(true);

        UpdateSlideButtons();
        UpdateProgressPerSubTopic();
        UpdateTopicTextSub();
    }

    public void ShowPageByName(string pageName)
    {
        if (pages == null) return;

        for (int i = 0; i < pages.Length; i++)
        {
            if (IsSamePage(pages[i].pageName, pageName))
            {
                ShowPage(i);
                return;
            }
        }

        Debug.LogWarning("Page tidak ditemukan: " + pageName);
        PlayErrorSound();
    }

    public void ShowPage(int index)
    {
        if (pages == null || pages.Length == 0) return;
        if (index < 0 || index >= pages.Length) return;

        int oldPageIndex = currentPageIndex;
        currentPageIndex = index;

        for (int i = 0; i < pages.Length; i++)
        {
            bool isActive = i == index;

            if (pages[i].pagePanel != null)
                SetActiveWithLog(pages[i].pagePanel, isActive);

            bool isHover = navHoverStates != null &&
                           i < navHoverStates.Length &&
                           navHoverStates[i];

            UpdateNavbarVisual(pages[i], isActive, isHover);
        }

        if (!isInitializing && oldPageIndex != index)
        {
            PlayPageOpenSound();
        }
    }

    // ================= VISUAL UPDATE =================

    private void UpdateNavbarVisual(NavPage page, bool isActive, bool isHover = false)
    {
        if (page == null) return;

        Color bgColor;
        Color iconColor;
        Color textColor;
        Color lineColor;
        float targetScale;

        if (isActive && isHover)
        {
            bgColor = activeHoverBackgroundColor;
            iconColor = activeHoverColor;
            textColor = activeHoverColor;
            lineColor = activeHoverLineColor;
            targetScale = activeHoverScale;
        }
        else if (isActive)
        {
            bgColor = activeBackgroundColor;
            iconColor = activeColor;
            textColor = activeColor;
            lineColor = activeColor;
            targetScale = activeScale;
        }
        else if (isHover)
        {
            bgColor = hoverBackgroundColor;
            iconColor = hoverColor;
            textColor = hoverColor;
            lineColor = hoverLineColor;
            targetScale = hoverScale;
        }
        else
        {
            bgColor = inactiveBackgroundColor;
            iconColor = inactiveIconColor;
            textColor = inactiveTextColor;
            lineColor = inactiveLineColor;
            targetScale = inactiveScale;
        }

        if (page.buttonBackground != null)
            page.buttonBackground.color = bgColor;

        if (page.iconImage != null)
            page.iconImage.color = iconColor;

        if (page.labelText != null)
            page.labelText.color = textColor;

        if (page.dividerLine != null)
            page.dividerLine.color = lineColor;

        if (useScaleEffect && page.button != null)
            page.button.transform.localScale = Vector3.one * targetScale;
    }

    private void UpdateSlideButtons()
    {
        if (sebelumButton != null)
            sebelumButton.interactable = true;

        if (lanjutButton != null)
            lanjutButton.interactable = true;

        if (isInSubTopic)
        {
            if (IsFirstSlideInCurrentSubTopic())
                SetButtonText(sebelumButtonText, textKembaliKeIntro);
            else
                SetButtonText(sebelumButtonText, textSebelumnya);

            if (IsLastSlideInCurrentSubTopic())
                SetButtonText(lanjutButtonText, textFinish);
            else
                SetButtonText(lanjutButtonText, textBerikutnya);
        }
        else
        {
            if (IsFirstSlideInCurrentTopic())
                SetButtonText(sebelumButtonText, textKembaliKeIntro);
            else
                SetButtonText(sebelumButtonText, textSebelumnya);

            if (IsLastSlideInCurrentTopic())
                SetButtonText(lanjutButtonText, textFinish);
            else
                SetButtonText(lanjutButtonText, textBerikutnya);
        }
    }

    private void UpdateProgressPerTopic()
    {
        int total = materiTopics[currentTopicIndex].slides.Length;
        int current = currentSlideIndex + 1;

        float progress = (float)current / total;
        int percent = Mathf.RoundToInt(progress * 100f);

        if (progressFillImage != null)
            progressFillImage.fillAmount = progress;

        if (progressPercentText != null)
            progressPercentText.text = percent + "%";

        if (slideCounterText != null)
            slideCounterText.text = current + " / " + total;
    }

    private void UpdateProgressPerSubTopic()
    {
        GameObject[] slides = materiTopics[currentTopicIndex].subTopics[currentSubTopicIndex].slides;

        int total = slides.Length;
        int current = currentSubSlideIndex + 1;

        float progress = (float)current / total;
        int percent = Mathf.RoundToInt(progress * 100f);

        if (progressFillImage != null)
            progressFillImage.fillAmount = progress;

        if (progressPercentText != null)
            progressPercentText.text = percent + "%";

        if (slideCounterText != null)
            slideCounterText.text = current + " / " + total;
    }

    private void UpdateTopicTextNormal()
    {
        if (topicTitleText != null)
            topicTitleText.text = materiTopics[currentTopicIndex].topicName;

        if (topicSlideCounterText != null)
        {
            int current = currentSlideIndex + 1;
            int total = materiTopics[currentTopicIndex].slides.Length;
            topicSlideCounterText.text = current + " / " + total;
        }
    }

    private void UpdateTopicTextSub()
    {
        if (topicTitleText != null)
        {
            topicTitleText.text =
                materiTopics[currentTopicIndex].topicName + " - " +
                materiTopics[currentTopicIndex].subTopics[currentSubTopicIndex].subTopicName;
        }

        if (topicSlideCounterText != null)
        {
            int current = currentSubSlideIndex + 1;
            int total = materiTopics[currentTopicIndex].subTopics[currentSubTopicIndex].slides.Length;
            topicSlideCounterText.text = current + " / " + total;
        }
    }

    private void UpdateTopicTextForSubMenu(int topicIndex)
    {
        if (topicTitleText != null)
            topicTitleText.text = materiTopics[topicIndex].topicName;

        if (topicSlideCounterText != null)
            topicSlideCounterText.text = "Pilih perangkat";
    }

    private void UpdateMulaiButtonText()
    {
        if (mulaiButtonText == null) return;

        if (IsAllTopicsCompleted())
            mulaiButtonText.text = textUlangi;
        else if (HasAnyTopicCompleted())
            mulaiButtonText.text = textLanjutkan;
        else
            mulaiButtonText.text = textMulai;
    }

    private void UpdateSubContinueButton(int topicIndex)
    {
        if (topicIndex < 0 || topicIndex >= materiTopics.Length) return;

        TMP_Text txt = materiTopics[topicIndex].subContinueButtonText;

        if (txt == null) return;

        if (AreAllSubTopicsCompleted(topicIndex))
        {
            txt.text = textSubSelesai;
        }
        else if (HasAnySubTopicCompleted(topicIndex))
        {
            txt.text = textSubLanjutkan;
        }
        else
        {
            txt.text = textSubMulai;
        }
    }

    // ================= CHECKER =================

    private bool IsFirstSlideInCurrentTopic()
    {
        return currentSlideIndex == 0;
    }

    private bool IsLastSlideInCurrentTopic()
    {
        if (materiTopics == null || materiTopics.Length == 0) return true;
        if (materiTopics[currentTopicIndex].slides == null) return true;

        return currentSlideIndex >= materiTopics[currentTopicIndex].slides.Length - 1;
    }

    private bool IsFirstSlideInCurrentSubTopic()
    {
        return currentSubSlideIndex == 0;
    }

    private bool IsLastSlideInCurrentSubTopic()
    {
        if (!IsValidSubTopic(currentTopicIndex, currentSubTopicIndex)) return true;

        GameObject[] slides = materiTopics[currentTopicIndex].subTopics[currentSubTopicIndex].slides;

        if (slides == null || slides.Length == 0) return true;

        return currentSubSlideIndex >= slides.Length - 1;
    }

    private bool IsValidSubTopic(int topicIndex, int subTopicIndex)
    {
        if (materiTopics == null) return false;
        if (topicIndex < 0 || topicIndex >= materiTopics.Length) return false;
        if (materiTopics[topicIndex].subTopics == null) return false;
        if (subTopicIndex < 0 || subTopicIndex >= materiTopics[topicIndex].subTopics.Length) return false;

        return true;
    }

    private bool HasAnyTopicCompleted()
    {
        if (materiTopics == null) return false;

        for (int i = 0; i < materiTopics.Length; i++)
        {
            if (IsTopicCompleted(i))
                return true;
        }

        return false;
    }

    private bool IsAllTopicsCompleted()
    {
        if (materiTopics == null || materiTopics.Length == 0) return false;

        for (int i = 0; i < materiTopics.Length; i++)
        {
            if (!IsTopicCompleted(i))
                return false;
        }

        return true;
    }

    private bool IsTopicCompleted(int topicIndex)
    {
        if (topicIndex < 0 || topicIndex >= materiTopics.Length) return false;

        if (materiTopics[topicIndex].useSubMenu)
            return AreAllSubTopicsCompleted(topicIndex);

        if (completedTopics == null) return false;
        if (topicIndex < 0 || topicIndex >= completedTopics.Length) return false;

        return completedTopics[topicIndex];
    }

    private bool HasAnySubTopicCompleted(int topicIndex)
    {
        if (completedSubTopics == null) return false;
        if (topicIndex < 0 || topicIndex >= completedSubTopics.Length) return false;

        for (int i = 0; i < completedSubTopics[topicIndex].Length; i++)
        {
            if (completedSubTopics[topicIndex][i])
                return true;
        }

        return false;
    }

    private bool AreAllSubTopicsCompleted(int topicIndex)
    {
        if (completedSubTopics == null) return false;
        if (topicIndex < 0 || topicIndex >= completedSubTopics.Length) return false;
        if (completedSubTopics[topicIndex].Length == 0) return false;

        for (int i = 0; i < completedSubTopics[topicIndex].Length; i++)
        {
            if (!completedSubTopics[topicIndex][i])
                return false;
        }

        return true;
    }

    private int GetFirstIncompleteTopicIndex()
    {
        if (materiTopics == null || materiTopics.Length == 0) return 0;

        for (int i = 0; i < materiTopics.Length; i++)
        {
            if (!IsTopicCompleted(i))
                return i;
        }

        return 0;
    }

    private int GetFirstIncompleteSubTopicIndex(int topicIndex)
    {
        if (completedSubTopics == null) return 0;
        if (topicIndex < 0 || topicIndex >= completedSubTopics.Length) return 0;

        for (int i = 0; i < completedSubTopics[topicIndex].Length; i++)
        {
            if (!completedSubTopics[topicIndex][i])
                return i;
        }

        return 0;
    }

    private void ResetProgressMateri()
    {
        if (completedTopics != null)
        {
            for (int i = 0; i < completedTopics.Length; i++)
                completedTopics[i] = false;
        }

        if (completedSubTopics != null)
        {
            for (int i = 0; i < completedSubTopics.Length; i++)
            {
                for (int j = 0; j < completedSubTopics[i].Length; j++)
                    completedSubTopics[i][j] = false;
            }
        }

        UpdateMulaiButtonText();
    }

    // ================= HIDE / ACTIVE =================

    private void HideAllSlides()
    {
        if (materiTopics == null) return;

        for (int i = 0; i < materiTopics.Length; i++)
        {
            if (materiTopics[i].subMenuSlide != null)
                SetActiveWithLog(materiTopics[i].subMenuSlide, false);

            if (materiTopics[i].slides != null)
            {
                for (int j = 0; j < materiTopics[i].slides.Length; j++)
                {
                    if (materiTopics[i].slides[j] != null)
                        SetActiveWithLog(materiTopics[i].slides[j], false);
                }
            }

            if (materiTopics[i].subTopics != null)
            {
                for (int s = 0; s < materiTopics[i].subTopics.Length; s++)
                {
                    if (materiTopics[i].subTopics[s].slides == null) continue;

                    for (int j = 0; j < materiTopics[i].subTopics[s].slides.Length; j++)
                    {
                        if (materiTopics[i].subTopics[s].slides[j] != null)
                            SetActiveWithLog(materiTopics[i].subTopics[s].slides[j], false);
                    }
                }
            }
        }
    }

    private void SetSlideNavigationVisible(bool active)
    {
        if (sebelumButton != null)
            SetActiveWithLog(sebelumButton.gameObject, active);

        if (lanjutButton != null)
            SetActiveWithLog(lanjutButton.gameObject, active);
    }

    private void SetProgressVisible(bool active)
    {
        if (progressRoot != null)
            SetActiveWithLog(progressRoot, active);
    }

    private void SetButtonText(TMP_Text textComponent, string value)
    {
        if (textComponent != null)
            textComponent.text = value;
    }

    private bool IsProtectedObject(GameObject target)
    {
        if (target == null) return false;
        if (protectedObjects == null) return false;

        for (int i = 0; i < protectedObjects.Length; i++)
        {
            if (protectedObjects[i] == null) continue;

            if (target == protectedObjects[i])
                return true;
        }

        return false;
    }

    private void SetActiveWithLog(GameObject target, bool active, [CallerMemberName] string callerName = "")
    {
        if (target == null)
        {
            if (enablePanelDebugLog)
                Debug.LogWarning("[UI DEBUG] Target NULL dari function: " + callerName);

            return;
        }

        if (!active && IsProtectedObject(target))
        {
            Debug.LogError(
                "[UI DEBUG] DIBATALKAN! Script mencoba mematikan object penting: " +
                GetFullPath(target.transform) +
                " | Dari function: " + callerName +
                "\nCEK INSPECTOR! Object ini kemungkinan salah masuk ke Page Panel / Slides / SubMenuSlide."
            );

            target.SetActive(true);
            return;
        }

        if (enablePanelDebugLog)
        {
            Debug.Log(
                "[UI DEBUG] Function: " + callerName +
                " | Object: " + GetFullPath(target.transform) +
                " | Sebelumnya: " + target.activeSelf +
                " | Jadi: " + active
            );
        }

        target.SetActive(active);
    }

    private string GetFullPath(Transform target)
    {
        if (target == null) return "NULL";

        string path = target.name;

        while (target.parent != null)
        {
            target = target.parent;
            path = target.name + "/" + path;
        }

        return path;
    }

    private bool IsSamePage(string a, string b)
    {
        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }
}