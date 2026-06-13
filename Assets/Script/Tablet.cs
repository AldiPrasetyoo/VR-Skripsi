using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TabletTabController : MonoBehaviour
{
    [Header("Panel")]
    public GameObject mapPanel;
    public GameObject missionPanel;

    [Header("Button")]
    public Button mapButton;
    public Button missionButton;

    [Header("Button Image")]
    public Image mapButtonImage;
    public Image missionButtonImage;

    [Header("Button Text")]
    public TMP_Text mapButtonText;
    public TMP_Text missionButtonText;

    [Header("Warna")]
    public Color activeButtonColor = new Color(0.1f, 0.45f, 1f, 1f);
    public Color inactiveButtonColor = new Color(0.15f, 0.15f, 0.15f, 1f);
    public Color activeTextColor = Color.white;
    public Color inactiveTextColor = Color.gray;

    private void Start()
    {
        if (mapButton != null)
            mapButton.onClick.AddListener(ShowMap);

        if (missionButton != null)
            missionButton.onClick.AddListener(ShowMission);

        ShowMap();
    }

    public void ShowMap()
    {
        if (mapPanel != null)
            mapPanel.SetActive(true);

        if (missionPanel != null)
            missionPanel.SetActive(false);

        SetButtonState(true);
    }

    public void ShowMission()
    {
        if (mapPanel != null)
            mapPanel.SetActive(false);

        if (missionPanel != null)
            missionPanel.SetActive(true);

        SetButtonState(false);
    }

    private void SetButtonState(bool mapActive)
    {
        if (mapButtonImage != null)
            mapButtonImage.color = mapActive ? activeButtonColor : inactiveButtonColor;

        if (missionButtonImage != null)
            missionButtonImage.color = mapActive ? inactiveButtonColor : activeButtonColor;

        if (mapButtonText != null)
            mapButtonText.color = mapActive ? activeTextColor : inactiveTextColor;

        if (missionButtonText != null)
            missionButtonText.color = mapActive ? inactiveTextColor : activeTextColor;
    }
}