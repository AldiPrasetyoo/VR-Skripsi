using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UI_WirelessDevice : MonoBehaviour
{

    #region Device
    [Header("Device")]
    public WirelessDevice device;

    private List<WirelessDevice> foundDevices = new List<WirelessDevice>();
    private List<WirelessDevice> scanned = new List<WirelessDevice>();
    bool isUpdating = false;

    [Header("Select Device UI")]
    public GameObject selectDevicePanel;
    public Transform deviceContainer;
    public GameObject deviceItemPrefab;

    #endregion

    #region Navbar
    
    [Header("Navbar Buttons")]
    public Button mainBtn;
    public Button wirelessBtn;
    public Button networkBtn;
    public Button systemBtn;

    [Header("Panels")]
    public GameObject navbarPanel;
    public GameObject mainPanel;
    public GameObject wirelessPanel;
    public GameObject networkPanel;
    public GameObject systemPanel;

    #endregion

    #region Login

    // ================= LOGIN =================
    [Header("Login")]
    public GameObject loginPanel;
    public Toggle agreeToggle;
    public Button loginButton;
    public GameObject mainUI;
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;

    #endregion

    #region UI Elements
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
    public TextMeshProUGUI wlan0MacText;
    public TextMeshProUGUI lan0MacText;
    public TextMeshProUGUI lan1MacText;
    public TextMeshProUGUI lan0dan1Text;
    public TextMeshProUGUI APMacText;
    public TextMeshProUGUI connectionText;

    public Slider signalStrength;
    public TextMeshProUGUI signalStrengthText;
    public TextMeshProUGUI horizonalVerticalText;
    public TextMeshProUGUI noiseFloorText;
    public TextMeshProUGUI transmitCCQText;
    public TextMeshProUGUI TXRXText;

    public TextMeshProUGUI AirMaxQualityText;
    public TextMeshProUGUI AirMaxCapacityText;
    public Slider AIRMaxQuality;
    public Slider AIRMaxCapacity;

    [Header("Signal Bars (10)")]
    public Image[] signalBars;
    public Image[] signalBarsPersenQuality;
    public Image[] signalBarsPersenCapacity;
    public Color offColor = new Color(0.2f, 0.2f, 0.2f, 0.5f); 
    public Color weakColor = Color.red;    
    public Color fairColor = Color.yellow;  
    public Color strongColor = Color.green;

    // ================= WIRELESS =================
    [Header("Wireless")]
    public TMP_Dropdown modeDropdown;

    public Toggle WDSToggle;
    public TMP_InputField SSIDInput;

    public TMP_Dropdown countryCodeDropdown;
    public Button scanButton;
    public TMP_InputField lockToAPInput;
    public Image lockToAPInputText;
    public TMP_Dropdown IEE80211Dropdown;
    public TMP_Dropdown channelWidthDropdown;
    public TMP_Dropdown channelShiftDropdown;
    public Image channelShiftDropdownText;
    public TMP_Dropdown frequencyDropdown;
    public Image frequencyDropdownText;
    public TMP_Dropdown extensionChannelDropdown;
    public Image extensionChannelDropdownText;

    public Toggle freqScanToggle;
    public Toggle calculateEIRPToggle;
    public Image calculateEIRPToggleText;
    public Toggle autoEIRPToggle;
    public Image autoEIRPToggleText;

    public TMP_InputField antennaGainInput;
    public Slider outputPowerSlider;

    public TMP_InputField outputPowerText;

    public TMP_Dropdown dataRateModuleDropdown;
    public TMP_Dropdown maxTXRateDropdown;
    public Toggle autoMaxTXRateToggle;

    public TMP_Text securityDropdownText;
    public TMP_Text WPAAuthDropdownText;
    public TMP_Text passwordText;

    public TMP_Text WPAttk2;
    public TMP_Text Passttk2;

    public TMP_Dropdown securityDropdown;
    public TMP_Dropdown WPAAuthDropdown;
    public TMP_InputField passwordTMP_InputField;

    public Button changeButton;

    // ================= STATION SUPPORT =================
    [Header("Station UI")]
    public TMP_Dropdown networkDropdown;

    private Coroutine scanRoutine;

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

    #endregion


    #region Unity Methods
    void Update()
    {
        if (mainPanel.activeSelf && device != null)
        {
            signalStrengthText.text = device.currentSignal.ToString("F2") + " dBm";
            noiseFloorText.text = device.noiseFloor.ToString("F2") + " dBm";
            horizonalVerticalText.text = device.horizontalSignal.ToString("F2") + " / " + device.verticalSignal.ToString("F2") + " dBm";
            TXRXText.text = device.txRate.ToString("F2") + " / " + device.rxRate.ToString("F2") + " Mbps";
            AirMaxQualityText.text = device.airMaxQuality.ToString("F0") + " %";
            AirMaxCapacityText.text = device.airMaxCapacity.ToString("F0") + " %";

            UpdateSignalBars(device.currentSignal);
            UpdateSignalBarsPersenQuality(device.airMaxQuality);
            UpdateSignalBarsPersenCapacity(device.airMaxCapacity);
        }
    }
    // ================= START =================

    void Start()
    {   
        selectDevicePanel.SetActive(true);
        loginPanel.SetActive(false);
        mainUI.SetActive(false);
        
        StartScanLoop();
    }

    void InitDevice()
{
    if (device == null) return;

    LoadAllUIFromDevice();

    // ================= BIND UI =================
    modeDropdown.onValueChanged.RemoveAllListeners();
    modeDropdown.onValueChanged.AddListener(OnModeChanged);
    modeDropdown.SetValueWithoutNotify((int)device.mode);

    outputPowerSlider.onValueChanged.RemoveAllListeners();
    outputPowerSlider.onValueChanged.AddListener(OnSliderChanged);

    outputPowerText.onEndEdit.RemoveAllListeners();
    outputPowerText.onEndEdit.AddListener(OnInputChanged);

    nameDeviceInput.text = device.deviceName;

    if (agreeToggle != null)
    {
        agreeToggle.onValueChanged.RemoveAllListeners();
        agreeToggle.onValueChanged.AddListener(OnToggleChanged);
        OnToggleChanged(agreeToggle.isOn);
    }

    SyncUI(outputPowerSlider.value);

    RefreshWirelessUI();
    RefreshNetworkUI();
    RefreshMainUI();
}

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

    #endregion
 
    #region Device Management

    void StartScanLoop()
{
    if (scanRoutine != null)
        StopCoroutine(scanRoutine);

    scanRoutine = StartCoroutine(ScanLoop());
}

IEnumerator ScanLoop()
{
    while (true)
    {
        ScanDevices();
        yield return new WaitForSeconds(1f); // refresh tiap 1 detik
    }
}

    public void AddDeviceToUI(WirelessDevice d)
    {
    if (foundDevices.Contains(d)) return;

    foundDevices.Add(d);

    GameObject item = Instantiate(deviceItemPrefab, deviceContainer);

    deviceItem ui = item.GetComponent<deviceItem>();
    ui.Setup(d, this);
    }

    void ScanDevices()
{
    foundDevices.Clear();

    foreach (Transform child in deviceContainer)
        Destroy(child.gameObject);

    WirelessDevice[] devices = FindObjectsOfType<WirelessDevice>();

    foreach (var d in devices)
    {
        AddDeviceToUI(d);
    }
}

    public void SelectDevice(WirelessDevice d)
    {
    device = d;

    selectDevicePanel.SetActive(false);
    loginPanel.SetActive(true);

    InitDevice();

    }

    #endregion

    #region Authentication

    public void Login()
    {
        if (agreeToggle != null && !agreeToggle.isOn)
            return;

        if (usernameInput.text == "ubnt" && passwordInput.text == "ubnt")
        {
            navbarPanel.SetActive(true);
            loginPanel.SetActive(false);
            mainUI.SetActive(true);
             
            usernameInput.text = "";
            passwordInput.text = "";
            agreeToggle.isOn = false;

            ShowMain();
        }
    }


    public void Logout()
    {
    mainUI.SetActive(false);
    navbarPanel.SetActive(false);
    loginPanel.SetActive(false);
    selectDevicePanel.SetActive(true);

    usernameInput.text = "";
    passwordInput.text = "";
    agreeToggle.isOn = false;

    }
    
    void OnToggleChanged(bool state)
    {
        if (loginButton != null)
            loginButton.interactable = state;
    }

    #endregion

    #region UI Navigation

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

    public Color32 warnaAwal = new Color32(21, 30, 39, 255);

    void SetActiveButton(Button activeBtn)
    {
    mainBtn.image.color = warnaAwal;
    wirelessBtn.image.color = warnaAwal;
    networkBtn.image.color = warnaAwal;
    systemBtn.image.color = warnaAwal;

    activeBtn.image.color = Color.blue;
    }

    #endregion

    #region refresh UI

    void RefreshMainUI()
    {
        if (device == null) return;
        deviceNameText.text = device.deviceName;
        networkModeText.text = networkModeDropdown.options[networkModeDropdown.value].text;;
        wirelessModeText.text = device.WDS 
                ? $"{device.mode} (WDS)" 
                : device.mode.ToString();
        SSIDText.text = device.ssid;
        SecurityText.text = device.securityMode.ToString();
        channelFrequencyText.text = device.frequency + " MHz";
        channelWidthText.text = device.channelWidth.ToString();
        frequencyBandText.text = device.GetFrequencyBand().ToString();
        distanceText.text = device.CalculateDistance().ToString("F2") + " m";
        TXPowerText.text = device.outputPower.ToString("F2") + " dBm";
        wlan0MacText.text = device.wlan0Mac;
        lan0MacText.text = device.lan0Mac;
        lan1MacText.text = device.lan1Mac;
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
            signalStrengthText.text = device.currentSignal.ToString("F2") + " dBm";
            horizonalVerticalText.text = device.horizontalSignal.ToString("F2") + " / " + device.verticalSignal.ToString("F2") + " dBm";
            TXRXText.text = device.txRate.ToString("F2") + " / " + device.rxRate.ToString("F2") + " Mbps";
        }
    }

    public void UpdateSignalBars(float currentDbm)
    {
        if (signalBars == null || signalBars.Length == 0) return;

        int activeBars = 0;

        if (currentDbm >= -45f) activeBars = 10;      
        else if (currentDbm >= -50f) activeBars = 9;  
        else if (currentDbm >= -55f) activeBars = 8;  
        else if (currentDbm >= -60f) activeBars = 7;  
        else if (currentDbm >= -65f) activeBars = 6;  
        else if (currentDbm >= -70f) activeBars = 5;  
        else if (currentDbm >= -75f) activeBars = 4;  
        else if (currentDbm >= -80f) activeBars = 3;  
        else if (currentDbm >= -85f) activeBars = 2;  
        else if (currentDbm >= -90f) activeBars = 1;  
        else activeBars = 0;                          

        Color currentColor;
        if (activeBars <= 3) currentColor = weakColor;
        else if (activeBars <= 6) currentColor = fairColor;
        else currentColor = strongColor;

        for (int i = 0; i < signalBars.Length; i++)
        {
            if (i < activeBars) signalBars[i].color = currentColor;
            else signalBars[i].color = offColor;
        }
    }
    
    public void UpdateSignalBarsPersenQuality(float currentSignalPersen)
    {
        if (signalBarsPersenQuality == null || signalBarsPersenQuality.Length == 0) return;

        int activeBars = 0;

        if (currentSignalPersen >= 90f) activeBars = 10;
        else if (currentSignalPersen >= 80f) activeBars = 9;
        else if (currentSignalPersen >= 70f) activeBars = 8;
        else if (currentSignalPersen >= 60f) activeBars = 7;
        else if (currentSignalPersen >= 50f) activeBars = 6;
        else if (currentSignalPersen >= 40f) activeBars = 5;
        else if (currentSignalPersen >= 30f) activeBars = 4;
        else if (currentSignalPersen >= 20f) activeBars = 3;
        else if (currentSignalPersen >= 10f) activeBars = 2;
        else activeBars = 0;

        Color currentColor;
        if (activeBars <= 3) currentColor = weakColor;
        else if (activeBars <= 6) currentColor = fairColor;
        else currentColor = strongColor;

        for (int i = 0; i < signalBarsPersenQuality.Length; i++)
        {
            if (i < activeBars) signalBarsPersenQuality[i].color = currentColor;
            else signalBarsPersenQuality[i].color = offColor;
        }
    }

    public void UpdateSignalBarsPersenCapacity(float currentSignalPersen)
    {
        if (signalBarsPersenCapacity == null || signalBarsPersenCapacity.Length == 0) return;

        int activeBars = 0;

        if (currentSignalPersen >= 90f) activeBars = 10;
        else if (currentSignalPersen >= 80f) activeBars = 9;
        else if (currentSignalPersen >= 70f) activeBars = 8;
        else if (currentSignalPersen >= 60f) activeBars = 7;
        else if (currentSignalPersen >= 50f) activeBars = 6;
        else if (currentSignalPersen >= 40f) activeBars = 5;
        else if (currentSignalPersen >= 30f) activeBars = 4;
        else if (currentSignalPersen >= 20f) activeBars = 3;
        else if (currentSignalPersen >= 10f) activeBars = 2;
        else activeBars = 0;

        Color currentColor;
        if (activeBars <= 3) currentColor = weakColor;
        else if (activeBars <= 6) currentColor = fairColor;
        else currentColor = strongColor;

        for (int i = 0; i < signalBarsPersenCapacity.Length; i++)
        {
            if (i < activeBars) signalBarsPersenCapacity[i].color = currentColor;
            else signalBarsPersenCapacity[i].color = offColor;
        }
    }

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
    if (networkDropdown != null)
        networkDropdown.gameObject.SetActive(isStation);

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

    #endregion

    #region UI Event Handlers


    void LoadAllUIFromDevice()
{
    if (device == null) return;

    // ================= WIRELESS =================
    modeDropdown.SetValueWithoutNotify((int)device.mode);

    SSIDInput.text = device.ssid;

    IEE80211Dropdown.SetValueWithoutNotify((int)device.ieee80211Mode);
    channelWidthDropdown.SetValueWithoutNotify((int)device.channelWidth);
    channelShiftDropdown.SetValueWithoutNotify((int)device.channelShift);

    antennaGainInput.text = device.antennaGain.ToString("F0");

    outputPowerSlider.value = device.outputPower;
    outputPowerText.text = device.outputPower.ToString("F0");

    WDSToggle.isOn = device.WDS;
    freqScanToggle.isOn = device.freqScan;
    calculateEIRPToggle.isOn = device.calculateEIRP;
    autoEIRPToggle.isOn = device.autoEIRP;

    dataRateModuleDropdown.SetValueWithoutNotify((int)device.dataRateModule);
    maxTXRateDropdown.SetValueWithoutNotify((int)device.maxTXRate);
    autoMaxTXRateToggle.isOn = device.autoMaxTXRate;

    securityDropdown.SetValueWithoutNotify((int)device.securityMode);
    WPAAuthDropdown.SetValueWithoutNotify((int)device.wpaAuthMode);
    passwordTMP_InputField.text = device.password;

    lockToAPInput.text = device.lockToAPSSID;

    // ================= NETWORK =================
    networkModeDropdown.SetValueWithoutNotify((int)device.networkMode);
    disableNetworkDropdown.SetValueWithoutNotify(device.disableNetwork ? 1 : 0);

    ipAddressInput.text = device.ipAddress;
    netmaskInput.text = device.netmask;
    gatewayInput.text = device.gateway;
    primaryDNSInput.text = device.primaryDNS;
    secondaryDNSInput.text = device.secondaryDNS;
    mtuInput.text = device.mtu.ToString();

    // ================= SYSTEM =================
    nameDeviceInput.text = device.deviceName;
}

    void OnModeChanged(int value)
    {
        device.SetMode(value);

        RefreshWirelessUI();
        RefreshNetworkUI();
        RefreshMainUI();

        if (device.mode == WirelessDevice.WirelessMode.Station){
            StartAutoScan();
            RefreshWirelessUI();
        }
        else
            StopAutoScan();
    }

    public void OnSecurityModeChanged(int dropdownIndex){
        int securityModeIndex = securityDropdown.value;
    if (securityModeIndex == 0)
    {   
        WPAAuthDropdownText.gameObject.SetActive(false);
        WPAttk2.gameObject.SetActive(false);
        WPAAuthDropdown.gameObject.SetActive(false);
        passwordText.gameObject.SetActive(false);
        Passttk2.gameObject.SetActive(false);
        passwordTMP_InputField.gameObject.SetActive(false);
    }
    else if (securityModeIndex == 1)
    {
        WPAAuthDropdownText.gameObject.SetActive(true);
        WPAttk2.gameObject.SetActive(true);
        WPAAuthDropdown.gameObject.SetActive(true);
    }
    }

    public void OnWPAAuthModeChanged(int dropdownIndex){
        int wpaAuthModeIndex = WPAAuthDropdown.value;
        if (wpaAuthModeIndex == 0) 
        {
            passwordText.gameObject.SetActive(false);
            Passttk2.gameObject.SetActive(false);
            passwordTMP_InputField.gameObject.SetActive(false);
        }
        else
        {
            passwordText.gameObject.SetActive(true);
            Passttk2.gameObject.SetActive(true);
            passwordTMP_InputField.gameObject.SetActive(true);
        }
    }

    public void Connect()
{
    if (scanned.Count == 0)
    {
        return;
    }

    int index = networkDropdown.value;

    if (index < 0 || index >= scanned.Count)
    {
        return;
    }

    var ap = scanned[index];

    device.Connect(ap, passwordTMP_InputField.text);

    RefreshWirelessUI();
    RefreshMainUI();
}

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

    #endregion

    #region Change Handlers

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
        Connect();
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
        device.SetDeviceName(nameDeviceInput.text);
        nameDeviceInput.text = device.deviceName;
        RefreshMainUI();
    }

    #endregion

    #region Campur

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

    #endregion

}