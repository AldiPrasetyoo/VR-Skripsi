using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeviceAP : MonoBehaviour
{
    public AccessPoint targetAP;

    [Header("Legacy UI Input")]
    public InputField ssidInput;
    public InputField passwordInput;
    public InputField txPowerInput;
    public InputField maxDistanceInput;
    public TMP_Dropdown channelDropdown;
    public TextMeshProUGUI statusText;

    public void ApplySettings()
    {
        if (targetAP == null) return;

        targetAP.ssid = ssidInput.text;
        targetAP.password = passwordInput.text;
        targetAP.txPower = float.Parse(txPowerInput.text);
        targetAP.maxDistance = float.Parse(maxDistanceInput.text);
        targetAP.channel = int.Parse(channelDropdown.options[channelDropdown.value].text);

        statusText.text = "AP CONFIGURED (5 GHz)";
    }
}