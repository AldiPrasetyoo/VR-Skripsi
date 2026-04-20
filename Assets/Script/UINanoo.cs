using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class UI_WirelessDevice : MonoBehaviour
{
    [Header("Device")]
    public WirelessDevice device;

    [Header("Mode")]
    public TMP_Dropdown modeDropdown;

    [Header("Panels")]
    public GameObject apPanel;
    public GameObject stationPanel;
    public GameObject radioPanel;

    [Header("AP Settings")]
    public InputField ssidInput;
    public InputField passwordInput;
    public TMP_Dropdown channelDropdown;

    [Header("Station Settings")]
    public TMP_Dropdown networkDropdown;
    public InputField stationPasswordInput;

    [Header("Radio Settings")]
    public InputField outputPowerInput;
    public InputField antennaGainInput;
    public InputField beamWidthInput;
    public InputField frequencyInput;
    public TMP_Dropdown channelWidthDropdown;
    public TextMeshProUGUI eirpText;

    private List<WirelessDevice> scanned = new List<WirelessDevice>();

    void Start()
    {
        if (device == null) return;

        modeDropdown.onValueChanged.AddListener(OnModeChanged);

        // Sinkronkan dropdown dengan mode awal device
        modeDropdown.value = (int)device.mode;

        LoadRadioToUI();
        RefreshUI();
    }

    // =========================
    // MODE
    // =========================
    void OnModeChanged(int value)
    {
        device.SetMode(value);
        RefreshUI();
    }

    void RefreshUI()
    {
        bool isAP = device.mode == WirelessDevice.WirelessMode.AccessPoint;
        bool isStation = device.mode == WirelessDevice.WirelessMode.Station;

        apPanel.SetActive(isAP);
        stationPanel.SetActive(isStation);
        radioPanel.SetActive(isAP || isStation);
    }

    // =========================
    // RADIO
    // =========================
    public void ApplyRadio()
    {
        if (device == null) return;

        if (float.TryParse(outputPowerInput.text, out float power))
            device.outputPower = power;

        if (float.TryParse(antennaGainInput.text, out float gain))
            device.antennaGain = gain;

        if (float.TryParse(beamWidthInput.text, out float beam))
            device.beamWidth = beam;

        if (float.TryParse(frequencyInput.text, out float freq))
            device.frequencyMHz = freq;

        if (channelWidthDropdown.options.Count > 0)
            device.channelWidth = int.Parse(
                channelWidthDropdown.options[channelWidthDropdown.value].text
            );

        UpdateEIRP();
    }

    void LoadRadioToUI()
    {
        outputPowerInput.text = device.outputPower.ToString();
        antennaGainInput.text = device.antennaGain.ToString();
        beamWidthInput.text = device.beamWidth.ToString();
        frequencyInput.text = device.frequencyMHz.ToString();
        UpdateEIRP();
    }

    void UpdateEIRP()
    {
        eirpText.text = "EIRP: " + device.GetEIRP().ToString("F1") + " dBm";
    }

    // =========================
    // AP
    // =========================
    public void ApplyAP()
    {
        if (device == null) return;

        device.ssid = ssidInput.text;
        device.password = passwordInput.text;

        if (channelDropdown.options.Count > 0)
            device.channel = int.Parse(
                channelDropdown.options[channelDropdown.value].text
            );
    }

    // =========================
    // STATION
    // =========================
    public void Scan()
    {
        scanned.Clear();
        networkDropdown.ClearOptions();

        List<string> options = new List<string>();

        foreach (var ap in NetworksRegistry.activeAPs)
        {
            if (ap == device) continue; // jangan scan diri sendiri

            scanned.Add(ap);
            options.Add(ap.ssid + " (Ch " + ap.channel + ")");
        }

        if (options.Count == 0)
            options.Add("No Network Found");

        networkDropdown.AddOptions(options);
    }

    public void Connect()
    {
        if (scanned.Count == 0) return;

        int index = networkDropdown.value;
        if (index < 0 || index >= scanned.Count) return;

        device.Connect(
            scanned[index],
            stationPasswordInput.text
        );
    }
}