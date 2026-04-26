using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UI_WirelessDevice : MonoBehaviour
{
    [Header("Device")]
    public WirelessDevice device;

    [Header("Navbar Buttons")]
    public Button mainBtn;
    public Button wirelessBtn;
    public Button networkBtn;
    public Button systemBtn;

    // ================= LOGIN =================
    [Header("Login")]
    public GameObject loginPanel;
    public Toggle agreeToggle;
    public Button loginButton;
    public GameObject mainUI;
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    // ================= NAVBAR =================
    [Header("Panels")]
    public GameObject navbarPanel;
    public GameObject mainPanel;
    public GameObject wirelessPanel;
    public GameObject networkPanel;
    public GameObject systemPanel;

    // ================= MAIN UI =================
    [Header("Main UI")]
    public TextMeshProUGUI deviceNameText;
    public TextMeshProUGUI networkModeText;
    public TextMeshProUGUI wirelessModeText;
    public TextMeshProUGUI SSIDText;
    public TextMeshProUGUI SecurityText;
    public TextMeshProUGUI channelFrequencyText;
    public TextMeshProUGUI channelWidthText;
    public TextMeshProUGUI frequencyBandText;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI TXPowerText;
    public TextMeshProUGUI lan0dan1Text;
    public TextMeshProUGUI APMacText;
    public TextMeshProUGUI connectionText;

    public Slider signalStrength;
    public TextMeshProUGUI signalStrengthText;
    public TextMeshProUGUI horizonalVerticalText;
    public TextMeshProUGUI noiseFloorText;
    public TextMeshProUGUI transmitCCQText;
    public TextMeshProUGUI TXRXText;

    public Slider AIRMaxQuality;
    public Slider AIRMaxCapacity;

    // ================= WIRELESS =================
    [Header("Wireless")]
    public TMP_Dropdown modeDropdown;

    public Toggle WDSToggle;
    public TMP_InputField SSIDInput;
    public TMP_Dropdown hasilScanDropdown;

    public TMP_Dropdown countryCodeDropdown;
    public Button scanButton;
    public TMP_InputField lockToAPInput;
    public TMP_Text lockToAPInputText;
    public TMP_Dropdown IEE80211Dropdown;
    public TMP_Dropdown channelWidthDropdown;
    public TMP_Dropdown channelShiftDropdown;
    public TMP_Text channelShiftDropdownText;
    public TMP_Dropdown frequencyDropdown;
    public TMP_Text frequencyDropdownText;
    public TMP_Dropdown extensionChannelDropdown;
    public TMP_Text extensionChannelDropdownText;

    public Toggle freqScanToggle;
    public Toggle calculateEIRPToggle;
    public TMP_Text calculateEIRPToggleText;
    public Toggle autoEIRPToggle;
    public TMP_Text autoEIRPToggleText;

    public TMP_InputField antennaGainInput;
    public Slider outputPowerSlider;

    public TMP_InputField outputPowerText;

    public TMP_Dropdown dataRateModuleDropdown;
    public TMP_Dropdown maxTXRateDropdown;
    public Toggle autoMaxTXRateToggle;

    public TMP_Text securityDropdownText;
    public TMP_Text WPAAuthDropdownText;
    public TMP_Text passwordText;

    public TMP_Dropdown securityDropdown;
    public TMP_Dropdown WPAAuthDropdown;
    public TMP_InputField passwordTMP_InputField;

    public Button changeButton;

    // ================= STATION SUPPORT =================
    [Header("Station UI")]
    public TMP_Dropdown networkDropdown;
    public TMP_InputField stationPasswordInput;

    private Coroutine scanRoutine;
    private List<WirelessDevice> scanned = new List<WirelessDevice>();

    // ================= NETWORK =================
    [Header("Network UI")]
    public TMP_Dropdown networkModeDropdown;
    public TMP_Dropdown disableNetworkDropdown;
    public TMP_Dropdown configureModeDropdown;
    public TMP_Dropdown managementIPAddressDropdown;

    public TMP_InputField ipAddressInput;
    public TMP_InputField netmaskInput;
    public TMP_InputField gatewayInput;
    public TMP_InputField primaryDNSInput;
    public TMP_InputField secondaryDNSInput;
    public TMP_InputField mtuInput;

    public Toggle autoIPToggle;

    public Button changeNetworkButton;

    // ================= SYSTEM =================
    [Header("System")]
    public TMP_InputField nameDeviceInput;
    public Button changeSystemButton;

    void Start()
    {
        loginPanel.SetActive(true);
        mainUI.SetActive(false);

        if (device == null) return;

        modeDropdown.onValueChanged.AddListener(OnModeChanged);
        modeDropdown.value = (int)device.mode;

        RefreshWirelessUI();
        RefreshNetworkUI();
        RefreshMainUI();

        if (agreeToggle != null)
        {
            agreeToggle.onValueChanged.AddListener(OnToggleChanged);
            OnToggleChanged(agreeToggle.isOn);
        }

        outputPowerSlider.onValueChanged.AddListener(OnSliderChanged);
        outputPowerText.onEndEdit.AddListener(OnInputChanged);

        // sync awal
        SyncUI(outputPowerSlider.value);
    }

    bool isUpdating = false;

void OnSliderChanged(float value)
{
    if (isUpdating) return;

    isUpdating = true;
    SyncUI(value);
    isUpdating = false;
}

void OnInputChanged(string text)
{
    if (isUpdating) return;

    float value;
    if (float.TryParse(text, out value))
    {
        value = Mathf.Clamp(value, outputPowerSlider.minValue, outputPowerSlider.maxValue);

        isUpdating = true;
        SyncUI(value);
        isUpdating = false;
    }
}

void SyncUI(float value)
{
    outputPowerSlider.value = value;
    outputPowerText.text = value.ToString("F0"); // tanpa desimal
}

    void SetActiveButton(Button activeBtn)
    {
    // semua jadi abu dulu
    mainBtn.image.color = Color.white;
    wirelessBtn.image.color = Color.white;
    networkBtn.image.color = Color.white;
    systemBtn.image.color = Color.white;

    // yang aktif jadi hitam
    activeBtn.image.color = Color.grey;
    }

    // ================= LOGIN =================
    public void Login()
    {
        if (agreeToggle != null && !agreeToggle.isOn)
            return;

        if (usernameInput.text == "ubnt" && passwordInput.text == "ubnt")
        {
            navbarPanel.SetActive(true);
            loginPanel.SetActive(false);
            mainUI.SetActive(true);
            ShowMain();
        }
    }

    void OnToggleChanged(bool state)
    {
        if (loginButton != null)
            loginButton.interactable = state;
    }

    // ================= NAVBAR =================
    public void ShowMain()
    {
        mainPanel.SetActive(true);
        wirelessPanel.SetActive(false);
        networkPanel.SetActive(false);
        systemPanel.SetActive(false);

        SetActiveButton(mainBtn);
    }

    public void ShowWireless()
    {
        mainPanel.SetActive(false);
        wirelessPanel.SetActive(true);
        networkPanel.SetActive(false);
        systemPanel.SetActive(false);

        SetActiveButton(wirelessBtn);

        RefreshWirelessUI();
    }

    public void ShowNetwork()
    {
        mainPanel.SetActive(false);
        wirelessPanel.SetActive(false);
        networkPanel.SetActive(true);
        systemPanel.SetActive(false);

        SetActiveButton(networkBtn);

        RefreshNetworkUI();
    }

        public void ShowSystem()
    {
        mainPanel.SetActive(false);
        wirelessPanel.SetActive(false);
        networkPanel.SetActive(false);
        systemPanel.SetActive(true);

        SetActiveButton(systemBtn);

    }

    // ================= MODE =================
    void OnModeChanged(int value)
    {
        device.SetMode(value);

        RefreshWirelessUI();
        RefreshNetworkUI();
        RefreshMainUI();

        if (device.mode == WirelessDevice.WirelessMode.Station)
            StartAutoScan();
        else
            StopAutoScan();
    }

    // ================= MAIN =================
    void RefreshMainUI()
    {
        if (device == null) return;
        deviceNameText.text = device.deviceName;
        networkModeText.text = networkModeDropdown.options[networkModeDropdown.value].text;;
        wirelessModeText.text = device.mode.ToString();
        SSIDText.text = device.ssid;
        SecurityText.text = device.securityMode.ToString();
        channelFrequencyText.text = device.frequency + " MHz";
        channelWidthText.text = device.channelWidth.ToString();
        frequencyBandText.text = device.GetFrequencyBand().ToString();
        distanceText.text = device.CalculateDistance().ToString("F2") + " m";
        TXPowerText.text = device.outputPower.ToString("F2") + " dBm";
        lan0dan1Text.text = device.lan0Status + " / " + device.lan1Status;
        AIRMaxQuality.value = device.airMaxQuality;
        AIRMaxCapacity.value = device.airMaxCapacity;
        noiseFloorText.text = device.noiseFloor.ToString("F2") + " dBm";
        transmitCCQText.text = device.transmitCCQ.ToString("F2") + " %";

        if (device.mode == WirelessDevice.WirelessMode.AccessPoint)
        {
            APMacText.text = device.GetAPMacAddress();
            connectionText.text = device.GetConnectedStationsCount() + " Station(s) Connected";
        }
        else
        {
            signalStrength.value = device.currentSignal;
            signalStrengthText.text = device.currentSignal.ToString("F2") + " dBm";
            horizonalVerticalText.text = "H: " + device.horizontalSignal.ToString("F2") + " dBm / V: " + device.verticalSignal.ToString("F2") + " dBm";
            TXRXText.text = "TX: " + device.txRate.ToString("F2") + " Mbps / RX: " + device.rxRate.ToString("F2") + " Mbps";
        }
    }

    // ================= WIRELESS (AP vs STATION TOGGLE UI) =================
    void RefreshWirelessUI()
    {
    bool isAP = device.mode == WirelessDevice.WirelessMode.AccessPoint;
    bool isStation = device.mode == WirelessDevice.WirelessMode.Station;

    // =========================================================
    // 🔵 COMMON (always visible)
    // =========================================================
    if (wirelessModeText != null)
        wirelessModeText.gameObject.SetActive(true);

    if (WDSToggle != null)
        WDSToggle.gameObject.SetActive(true);

    if (countryCodeDropdown != null)
        countryCodeDropdown.gameObject.SetActive(true);

    if (IEE80211Dropdown != null)
        IEE80211Dropdown.gameObject.SetActive(true);

    if (channelWidthDropdown != null)
        channelWidthDropdown.gameObject.SetActive(true);

    if (freqScanToggle != null)
        freqScanToggle.gameObject.SetActive(true);

    if (antennaGainInput != null)
        antennaGainInput.gameObject.SetActive(true);

    if (outputPowerSlider != null)
        outputPowerSlider.gameObject.SetActive(true);

    if (dataRateModuleDropdown != null)
        dataRateModuleDropdown.gameObject.SetActive(true);

    if (maxTXRateDropdown != null)
        maxTXRateDropdown.gameObject.SetActive(true);

    if (securityDropdown != null)
        securityDropdown.gameObject.SetActive(true);

if (device.securityMode == WirelessDevice.SecurityMode.WPA2_AES)
{
    if (WPAAuthDropdown != null)
    {
        WPAAuthDropdownText.gameObject.SetActive(true);
        WPAAuthDropdown.gameObject.SetActive(true);
    }

    if (device.wpaAuthMode == WirelessDevice.WPAAuthMode.PSK)
    {
        if (passwordTMP_InputField != null)
        {
            passwordText.gameObject.SetActive(true);
            passwordTMP_InputField.gameObject.SetActive(true);
        }
    }
    else
    {
        if (passwordTMP_InputField != null)
        {
            passwordText.gameObject.SetActive(false);
            passwordTMP_InputField.gameObject.SetActive(false);
        }
    }
}
else
{
    if (WPAAuthDropdown != null)
    {
        WPAAuthDropdownText.gameObject.SetActive(false);
        WPAAuthDropdown.gameObject.SetActive(false);
    }

    if (passwordTMP_InputField != null)
    {
        passwordText.gameObject.SetActive(false);
        passwordTMP_InputField.gameObject.SetActive(false);
    }
}

   if (changeButton != null)
        changeButton.gameObject.SetActive(true);

    // =========================================================
    // 🔴 AP MODE
    // =========================================================
    if (SSIDInput != null) SSIDInput.gameObject.SetActive(isAP);

    if(frequencyDropdownText != null)
        frequencyDropdownText.gameObject.SetActive(isAP);
        
    if (frequencyDropdown != null)
        frequencyDropdown.gameObject.SetActive(isAP);

    if (extensionChannelDropdownText != null)
        extensionChannelDropdownText.gameObject.SetActive(isAP);
        
    if (extensionChannelDropdown != null)
        extensionChannelDropdown.gameObject.SetActive(isAP);

    if (extensionChannelDropdownText != null)
        extensionChannelDropdownText.gameObject.SetActive(isAP);

    if (calculateEIRPToggle != null)
        calculateEIRPToggle.gameObject.SetActive(isAP);

    if (calculateEIRPToggleText != null)
        calculateEIRPToggleText.gameObject.SetActive(isAP);

    // =========================================================
    // 🟢 STATION MODE
    // =========================================================
    if(hasilScanDropdown != null)
        hasilScanDropdown.gameObject.SetActive(isStation);

    if (lockToAPInput != null)
        lockToAPInput.gameObject.SetActive(isStation);

    if (lockToAPInputText != null)
        lockToAPInputText.gameObject.SetActive(isStation);

    if (channelShiftDropdown != null)
        channelShiftDropdown.gameObject.SetActive(isStation);

    if (channelShiftDropdownText != null)
        channelShiftDropdownText.gameObject.SetActive(isStation);

    if (autoEIRPToggleText != null)
        autoEIRPToggleText.gameObject.SetActive(isStation);

    if (autoEIRPToggle != null)
        autoEIRPToggle.gameObject.SetActive(isStation);

    if (scanButton != null)
        scanButton.gameObject.SetActive(isStation);

    }

    public void OnManagementIPChanged(int dropdownIndex)
    {
        int dropManagementIPIndex = managementIPAddressDropdown.value;
    if (dropManagementIPIndex == 0)
    {
        ipAddressInput.interactable = false;
        netmaskInput.interactable = false;
        gatewayInput.interactable = false;
        primaryDNSInput.interactable = false;
        secondaryDNSInput.interactable = false;
    }
    else
    {
        ipAddressInput.interactable = true;
        netmaskInput.interactable = true;
        gatewayInput.interactable = true;
        primaryDNSInput.interactable = true;
        secondaryDNSInput.interactable = true;
    }
    }

    public void OnSecurityModeChanged(int dropdownIndex){
        int securityModeIndex = securityDropdown.value;
    if (securityModeIndex == 0) // Open
    {   
        WPAAuthDropdownText.gameObject.SetActive(false);
        passwordText.gameObject.SetActive(false);
        WPAAuthDropdown.gameObject.SetActive(false);
        passwordTMP_InputField.gameObject.SetActive(false);
    }
    else if (securityModeIndex == 1) // WPA2_AES
    {
        WPAAuthDropdownText.gameObject.SetActive(true);
        WPAAuthDropdown.gameObject.SetActive(true);
    }
    }

    public void OnWPAAuthModeChanged(int dropdownIndex){
        int wpaAuthModeIndex = WPAAuthDropdown.value;
        if (wpaAuthModeIndex == 0) // None
        {
            passwordText.gameObject.SetActive(false);
            passwordTMP_InputField.gameObject.SetActive(false);
        }
        else
        {
            passwordText.gameObject.SetActive(true);
            passwordTMP_InputField.gameObject.SetActive(true);
        }
    }

    // ================= NETWORK =================
    void RefreshNetworkUI()
    {
    if (device == null) return;

    networkModeDropdown.value = (int)device.networkMode;
    disableNetworkDropdown.value = device.disableNetwork ? 1 : 0;
    ipAddressInput.text = device.ipAddress;
    netmaskInput.text = device.netmask;
    gatewayInput.text = device.gateway;
    primaryDNSInput.text = device.primaryDNS;
    secondaryDNSInput.text = device.secondaryDNS;
    mtuInput.text = device.mtu.ToString();
    }

    // ================= AUTO SCAN =================
    void StartAutoScan()
    {
        if (scanRoutine != null)
            StopCoroutine(scanRoutine);

        scanRoutine = StartCoroutine(AutoScan());
    }

    void StopAutoScan()
    {
        if (scanRoutine != null)
            StopCoroutine(scanRoutine);
    }

    IEnumerator AutoScan()
    {
        while (true)
        {
            Scan();
            yield return new WaitForSeconds(2f);
        }
    }

    // ================= SCAN =================
    public void Scan()
    {
        scanned.Clear();

        if (networkDropdown == null) return;

        networkDropdown.ClearOptions();

        List<string> options = new List<string>();

        foreach (var ap in NetworksRegistry.activeAPs)
        {
            if (ap == device) continue;

            scanned.Add(ap);
            options.Add(ap.ssid + " (Ch " + ap.channel + ")");
        }

        if (options.Count == 0)
            options.Add("No Network Found");

        networkDropdown.AddOptions(options);

        RefreshWirelessUI();
    }

public void Connect()
{
    Debug.Log("=== CONNECT FUNCTION MASUK ===");
    if (scanned.Count == 0)
    {
        Debug.Log("❌ Tidak ada AP");
        return;
    }

    int index = networkDropdown.value;

    if (index < 0 || index >= scanned.Count)
    {
        Debug.Log("❌ Index invalid");
        return;
    }

    var ap = scanned[index];

    Debug.Log("Mencoba connect ke: " + ap.ssid);

    device.Connect(ap, stationPasswordInput.text);

    if (device.connectedAP != null)
    {
        Debug.Log("✅ CONNECT BERHASIL");
    }
    else
    {
        Debug.Log("❌ CONNECT GAGAL");
    }

    RefreshWirelessUI();
    RefreshMainUI();
}

    public void ChangeWireless()
    {
        device.WDS = WDSToggle.isOn;
        device.countryCode = countryCodeDropdown.options[countryCodeDropdown.value].text;
        device.ieee80211Mode = (WirelessDevice.IEEE80211Mode)IEE80211Dropdown.value;
        device.channelWidth = (WirelessDevice.ChannelWidth)channelWidthDropdown.value;
        device.freqScan = freqScanToggle.isOn;
        device.antennaGain = float.Parse(antennaGainInput.text);
        device.outputPower = outputPowerSlider.value;
        device.dataRateModule = (WirelessDevice.DataRateModule)dataRateModuleDropdown.value;
        device.maxTXRate = (WirelessDevice.MaxTXRate)maxTXRateDropdown.value;
        device.autoMaxTXRate = autoMaxTXRateToggle.isOn;
        device.securityMode = (WirelessDevice.SecurityMode)securityDropdown.value;
        device.wpaAuthMode = (WirelessDevice.WPAAuthMode)WPAAuthDropdown.value;
        device.password = passwordTMP_InputField.text;

        if (device.mode == WirelessDevice.WirelessMode.AccessPoint)
        {
            device.ssid = SSIDInput.text;
            device.frequency = frequencyDropdown.value == 0 ? 2412 : 5180; // contoh: 2412 MHz untuk channel 1, 5180 MHz untuk channel 36
            device.extensionChannel = (WirelessDevice.ExtensionChannel)extensionChannelDropdown.value;
            device.calculateEIRP = calculateEIRPToggle.isOn;
        }
        else
        {
            device.lockToAPSSID = lockToAPInput.text;
            device.channelShift = (WirelessDevice.ChannelShift)channelShiftDropdown.value;
            device.autoEIRP = autoEIRPToggle.isOn;
            device.ssid = networkDropdown.options[networkDropdown.value].text.Split(' ')[0]; // Ambil SSID dari dropdown hasil scan
        }
        RefreshWirelessUI();
        RefreshMainUI();
        Connect(); // Coba connect ulang setelah setting diubah
        Debug.Log("Wireless settings updated");
    }

    public void changeNetwork()
    {
        device.ipAddress = ipAddressInput.text;
        device.netmask = netmaskInput.text;
        device.gateway = gatewayInput.text;
        device.primaryDNS = primaryDNSInput.text;
        device.secondaryDNS = secondaryDNSInput.text;
        int mtu;
        if (int.TryParse(mtuInput.text, out mtu))        {
            device.mtu = mtu;
        }

        RefreshNetworkUI();
    }

    public void changeSystem()
    {
        device.deviceName = nameDeviceInput.text;

        RefreshMainUI();
    }
}