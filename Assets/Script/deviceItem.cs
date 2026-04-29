using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class deviceItem : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI deviceNameText;
    public Button button;

    private WirelessDevice device;
    private UI_WirelessDevice manager;

    public void Setup(WirelessDevice d, UI_WirelessDevice m)
    {
        device = d;
        manager = m;

        deviceNameText.text = d.deviceName;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        manager.SelectDevice(device);
    }
}