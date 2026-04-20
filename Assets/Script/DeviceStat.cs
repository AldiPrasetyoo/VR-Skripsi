using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ConfigDeviceStation : MonoBehaviour
{
    public StationDevice targetStation;

    [Header("UI")]
    public TMP_Dropdown networkDropdown;
    public InputField passwordInput;
    public InputField beamAngleInput;
    public TextMeshProUGUI statusText;

    private List<AccessPoint> scannedAPs = new List<AccessPoint>();

    // =====================
    // SCAN NETWORK
    // =====================
    public void Scan()
    {
        networkDropdown.ClearOptions();
        scannedAPs.Clear();

        List<string> options = new List<string>();

        foreach (var ap in NetworkRegistry.activeAPs)
        {
            scannedAPs.Add(ap);
            options.Add(ap.ssid + " (Ch " + ap.channel + ")");
        }

        networkDropdown.AddOptions(options);
        statusText.text = "Scan Complete";
    }

    // =====================
    // APPLY SETTING + CONNECT
    // =====================
    public void Connect()
    {
        if (scannedAPs.Count == 0)
        {
            statusText.text = "No Network Found";
            return;
        }

        int index = networkDropdown.value;
        AccessPoint selectedAP = scannedAPs[index];

        targetStation.beamAngle = float.Parse(beamAngleInput.text);

        targetStation.ConnectToAP(selectedAP, passwordInput.text);

        statusText.text = "Connecting...";
    }

    public void Disconnect()
    {
        targetStation.Disconnect();
        statusText.text = "Disconnected";
    }
}

