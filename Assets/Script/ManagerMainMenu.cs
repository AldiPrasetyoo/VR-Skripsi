using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuSettingManager : MonoBehaviour
{
    [Header("Panel")]
    public GameObject settingPanel;
    public GameObject[] panelsKiri; // panel PLAY, SETTING, EXIT / menu kiri

    [Header("Graphic")]
    public TMP_Dropdown qualityDropdown;

    [Header("Volume")]
    public Slider volumeSlider;
    public TextMeshProUGUI volumeText;

    private const string QualityKey = "GameQuality";
    private const string VolumeKey = "GameVolume";

    private void Start()
    {
        settingPanel.SetActive(false);

        int savedQuality = PlayerPrefs.GetInt(QualityKey, QualitySettings.GetQualityLevel());
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);

        QualitySettings.SetQualityLevel(savedQuality);
        AudioListener.volume = savedVolume;

        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
            qualityDropdown.value = savedQuality;
            qualityDropdown.RefreshShownValue();
            qualityDropdown.onValueChanged.AddListener(SetQuality);
        }

        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        UpdateVolumeText(savedVolume);
    }

    public void OpenSetting()
    {
        settingPanel.SetActive(true);

        foreach (GameObject panel in panelsKiri)
        {
            if (panel != null)
                panel.SetActive(false);
        }
    }

    public void CloseSetting()
    {
        settingPanel.SetActive(false);

        foreach (GameObject panel in panelsKiri)
        {
            if (panel != null)
                panel.SetActive(true);
        }
    }

    public void SetQuality(int index)
    {
        GameSettingManager.Instance.SetQuality(index);
        PlayerPrefs.SetInt(QualityKey, index);
        PlayerPrefs.Save();
    }

    public void SetVolume(float value)
    {
        GameSettingManager.Instance.SetVolume(value);
        PlayerPrefs.SetFloat(VolumeKey, value);
        PlayerPrefs.Save();

        UpdateVolumeText(value);
    }

    public void ResetDefault()
    {
        int defaultQuality = QualitySettings.names.Length - 1;
        float defaultVolume = 1f;

        QualitySettings.SetQualityLevel(defaultQuality);
        AudioListener.volume = defaultVolume;

        if (qualityDropdown != null)
        {
            qualityDropdown.value = defaultQuality;
            qualityDropdown.RefreshShownValue();
        }

        if (volumeSlider != null)
            volumeSlider.value = defaultVolume;

        PlayerPrefs.SetInt(QualityKey, defaultQuality);
        PlayerPrefs.SetFloat(VolumeKey, defaultVolume);
        PlayerPrefs.Save();

        UpdateVolumeText(defaultVolume);
    }

    private void UpdateVolumeText(float value)
    {
        if (volumeText != null)
            volumeText.text = Mathf.RoundToInt(value * 100f) + "%";
    }
}