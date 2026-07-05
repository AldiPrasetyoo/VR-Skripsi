using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UI_WirelessDevice : MonoBehaviour
{
    #region Device
    [Header("Device Reference")]
    public WirelessDevice device;

    private List<WirelessDevice> foundDevices = new List<WirelessDevice>();
    private List<WirelessDevice> scanned = new List<WirelessDevice>();
    private bool isUpdating = false;

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
    [Header("Login Components")]
    public GameObject loginPanel;
    public Toggle agreeToggle;
    public Button loginButton;
    public GameObject mainUI;
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    #endregion

    #region UI Elements
    [Header("Main UI - Monitor")]
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

    [Header("Wireless Configuration")]
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

    [Header("Wireless Security UI Elements")]
    public TMP_Text securityDropdownText;
    public TMP_Text WPAAuthDropdownText;
    public TMP_Text passwordText;
    public TMP_Text WPAttk2;
    public TMP_Text Passttk2;
    public TMP_Dropdown securityDropdown;
    public TMP_Dropdown WPAAuthDropdown;
    public TMP_InputField passwordTMP_InputField;
    public Button changeButton;

    [Header("Station Mode UI Only")]
    public TMP_Dropdown networkDropdown;
    private Coroutine scanRoutine;
    private Coroutine autoScanRoutine;

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

    [Header("System UI")]
    public TMP_InputField nameDeviceInput;
    public Button changeSystemButton;

    [Header("Theme Color")]
    public Color32 warnaAwal = new Color32(21, 30, 39, 255);
    #endregion

    #region Unity Methods
    void Start()
    {
        // UI ini tidak memilih device sendiri lagi.
        // Device akan dikirim dari ComputerHomeUI setelah browser berhasil akses IP Nano.

        SetPanel(selectDevicePanel, false);
        SetPanel(loginPanel, true);
        SetPanel(mainUI, false);
        SetPanel(navbarPanel, false);
        SetPanel(mainPanel, false);
        SetPanel(wirelessPanel, false);
        SetPanel(networkPanel, false);
        SetPanel(systemPanel, false);

        if (scanButton != null)
        {
            scanButton.onClick.RemoveAllListeners();
            scanButton.onClick.AddListener(OnScanButtonClicked);
        }

        if (agreeToggle != null)
        {
            agreeToggle.onValueChanged.RemoveAllListeners();
            agreeToggle.onValueChanged.AddListener(OnToggleChanged);
            OnToggleChanged(agreeToggle.isOn);
        }

        // Tidak ada StartScanLoop() di sini.
        // Select device sudah tidak dipakai untuk akses dari komputer.
    }

    void Update()
    {
        if (mainPanel != null && mainPanel.activeSelf && device != null)
        {
            if (signalStrengthText != null)
                signalStrengthText.text = device.currentSignal.ToString("F2") + " dBm";

            if (noiseFloorText != null)
                noiseFloorText.text = device.noiseFloor.ToString("F2") + " dBm";

            if (horizonalVerticalText != null)
                horizonalVerticalText.text = device.horizontalSignal.ToString("F2") + " / " + device.verticalSignal.ToString("F2") + " dBm";

            if (TXRXText != null)
                TXRXText.text = device.txRate.ToString("F2") + " / " + device.rxRate.ToString("F2") + " Mbps";

            if (AirMaxQualityText != null)
                AirMaxQualityText.text = device.airMaxQuality.ToString("F0") + " %";

            if (AirMaxCapacityText != null)
                AirMaxCapacityText.text = device.airMaxCapacity.ToString("F0") + " %";

            UpdateSignalBars(device.currentSignal);
            UpdateSignalBarsPersenQuality(device.airMaxQuality);
            UpdateSignalBarsPersenCapacity(device.airMaxCapacity);
        }
    }

    void InitDevice()
    {
        if (device == null) return;

        LoadAllUIFromDevice();

        if (modeDropdown != null)
        {
            modeDropdown.onValueChanged.RemoveAllListeners();
            modeDropdown.onValueChanged.AddListener(OnModeChanged);
            modeDropdown.SetValueWithoutNotify((int)device.mode);
        }

        if (outputPowerSlider != null)
        {
            outputPowerSlider.onValueChanged.RemoveAllListeners();
            outputPowerSlider.onValueChanged.AddListener(OnSliderChanged);
        }

        if (outputPowerText != null)
        {
            outputPowerText.onEndEdit.RemoveAllListeners();
            outputPowerText.onEndEdit.AddListener(OnInputChanged);
        }

        if (nameDeviceInput != null)
            nameDeviceInput.text = device.deviceName;

        if (agreeToggle != null)
        {
            agreeToggle.onValueChanged.RemoveAllListeners();
            agreeToggle.onValueChanged.AddListener(OnToggleChanged);
            OnToggleChanged(agreeToggle.isOn);
        }

        if (outputPowerSlider != null)
            SyncUI(outputPowerSlider.value);

        RefreshWirelessUI();
        RefreshNetworkUI();
        RefreshMainUI();
    }

    private void SetPanel(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
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

        if (float.TryParse(text, out float value))
        {
            if (outputPowerSlider != null)
                value = Mathf.Clamp(value, outputPowerSlider.minValue, outputPowerSlider.maxValue);

            isUpdating = true;
            SyncUI(value);
            isUpdating = false;
        }
    }

    void SyncUI(float value)
    {
        if (outputPowerSlider != null)
            outputPowerSlider.value = value;

        if (outputPowerText != null)
            outputPowerText.text = value.ToString("F0");
    }
    #endregion

    #region Open From Computer
    public void OpenFromComputer(WirelessDevice targetDevice)
    {
        if (targetDevice == null)
        {
            Debug.LogWarning("Target Nano dari komputer kosong. UI Wireless tidak bisa dibuka.");
            return;
        }

        // Stop semua scan pemilihan device lama.
        if (scanRoutine != null)
        {
            StopCoroutine(scanRoutine);
            scanRoutine = null;
        }

        StopAutoScan();

        // Ini inti alur baru:
        // device langsung menjadi Nano yang ditemukan dari browser komputer.
        device = targetDevice;

        SetPanel(selectDevicePanel, false);
        SetPanel(navbarPanel, false);
        SetPanel(mainUI, false);
        SetPanel(mainPanel, false);
        SetPanel(wirelessPanel, false);
        SetPanel(networkPanel, false);
        SetPanel(systemPanel, false);

        SetPanel(loginPanel, true);

        if (usernameInput != null)
            usernameInput.text = "";

        if (passwordInput != null)
            passwordInput.text = "";

        if (agreeToggle != null)
        {
            agreeToggle.isOn = false;
            agreeToggle.onValueChanged.RemoveAllListeners();
            agreeToggle.onValueChanged.AddListener(OnToggleChanged);
            OnToggleChanged(false);
        }

        InitDevice();

        Debug.Log("UI Nano dibuka langsung dari komputer untuk device: " + device.gameObject.name);
    }
    #endregion

    #region Device Management Lama
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
            yield return new WaitForSeconds(1f);
        }
    }

    public void AddDeviceToUI(WirelessDevice d)
    {
        if (d == null) return;
        if (foundDevices.Contains(d)) return;
        if (deviceItemPrefab == null || deviceContainer == null) return;

        foundDevices.Add(d);

        GameObject item = Instantiate(deviceItemPrefab, deviceContainer);
        deviceItem ui = item.GetComponent<deviceItem>();

        if (ui != null)
            ui.Setup(d, this);
    }

    void ScanDevices()
    {
        foundDevices.Clear();

        if (deviceContainer != null)
        {
            foreach (Transform child in deviceContainer)
                Destroy(child.gameObject);
        }

        WirelessDevice[] devices = FindObjectsOfType<WirelessDevice>();

        foreach (WirelessDevice d in devices)
        {
            AddDeviceToUI(d);
        }
    }

    public void SelectDevice(WirelessDevice d)
    {
        // Fungsi lama tetap dibiarkan supaya tidak merusak script deviceItem.
        // Tapi alur utama sekarang pakai OpenFromComputer().
        if (d == null) return;

        device = d;

        SetPanel(selectDevicePanel, false);
        SetPanel(loginPanel, true);

        InitDevice();
    }
    #endregion

    #region Authentication
    public void Login()
    {
        if (device == null)
        {
            Debug.LogWarning("Belum ada target Nano. Buka UI dari ComputerHomeUI dulu.");
            return;
        }

        if (agreeToggle != null && !agreeToggle.isOn)
            return;

        if (usernameInput != null && passwordInput != null)
        {
            if (usernameInput.text == "ubnt" && passwordInput.text == "ubnt")
            {
                SetPanel(navbarPanel, true);
                SetPanel(loginPanel, false);
                SetPanel(mainUI, true);

                usernameInput.text = "";
                passwordInput.text = "";

                if (agreeToggle != null)
                    agreeToggle.isOn = false;

                ShowMain();
            }
        }
    }

    public void Logout()
    {
        StopAutoScan();

        // Jangan device = null.
        // Supaya logout tetap kembali ke login Nano yang sama,
        // bukan balik ke select device.

        SetPanel(mainUI, false);
        SetPanel(navbarPanel, false);
        SetPanel(loginPanel, true);
        SetPanel(selectDevicePanel, false);

        SetPanel(mainPanel, false);
        SetPanel(wirelessPanel, false);
        SetPanel(networkPanel, false);
        SetPanel(systemPanel, false);

        if (usernameInput != null)
            usernameInput.text = "";

        if (passwordInput != null)
            passwordInput.text = "";

        if (agreeToggle != null)
        {
            agreeToggle.isOn = false;
            OnToggleChanged(false);
        }
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
        if (device == null) return;

        SetPanel(mainPanel, true);
        SetPanel(wirelessPanel, false);
        SetPanel(networkPanel, false);
        SetPanel(systemPanel, false);

        SetActiveButton(mainBtn);
        RefreshMainUI();
    }

    public void ShowWireless()
    {
        if (device == null) return;

        SetPanel(mainPanel, false);
        SetPanel(wirelessPanel, true);
        SetPanel(networkPanel, false);
        SetPanel(systemPanel, false);

        SetActiveButton(wirelessBtn);
        RefreshWirelessUI();
    }

    public void ShowNetwork()
    {
        if (device == null) return;

        SetPanel(mainPanel, false);
        SetPanel(wirelessPanel, false);
        SetPanel(networkPanel, true);
        SetPanel(systemPanel, false);

        SetActiveButton(networkBtn);
        RefreshNetworkUI();
    }

    public void ShowSystem()
    {
        if (device == null) return;

        SetPanel(mainPanel, false);
        SetPanel(wirelessPanel, false);
        SetPanel(networkPanel, false);
        SetPanel(systemPanel, true);

        SetActiveButton(systemBtn);
    }

    void SetActiveButton(Button activeBtn)
    {
        if (mainBtn != null && mainBtn.image != null)
            mainBtn.image.color = warnaAwal;

        if (wirelessBtn != null && wirelessBtn.image != null)
            wirelessBtn.image.color = warnaAwal;

        if (networkBtn != null && networkBtn.image != null)
            networkBtn.image.color = warnaAwal;

        if (systemBtn != null && systemBtn.image != null)
            systemBtn.image.color = warnaAwal;

        if (activeBtn != null && activeBtn.image != null)
            activeBtn.image.color = Color.blue;
    }
    #endregion

    #region Refresh UI Logics
    void RefreshMainUI()
    {
        if (device == null) return;

        if (deviceNameText != null)
            deviceNameText.text = device.deviceName;

        if (networkModeText != null && networkModeDropdown != null && networkModeDropdown.options.Count > networkModeDropdown.value)
            networkModeText.text = networkModeDropdown.options[networkModeDropdown.value].text;

        if (wirelessModeText != null)
            wirelessModeText.text = device.WDS ? $"{device.mode} (WDS)" : device.mode.ToString();

        if (SSIDText != null)
            SSIDText.text = device.ssid;

        if (SecurityText != null)
            SecurityText.text = device.securityMode.ToString();

        if (channelFrequencyText != null)
            channelFrequencyText.text = device.frequency + " MHz";

        if (channelWidthText != null)
            channelWidthText.text = device.channelWidth.ToString();

        if (frequencyBandText != null)
            frequencyBandText.text = device.GetFrequencyBand();

        if (distanceText != null)
            distanceText.text = device.CalculateDistance().ToString("F2") + " m";

        if (TXPowerText != null)
            TXPowerText.text = device.outputPower.ToString("F2") + " dBm";

        if (wlan0MacText != null)
            wlan0MacText.text = device.wlan0Mac;

        if (lan0MacText != null)
            lan0MacText.text = device.lan0Mac;

        if (lan1MacText != null)
            lan1MacText.text = device.lan1Mac;

        if (lan0dan1Text != null)
            lan0dan1Text.text = device.lan0Status + " / " + device.lan1Status;

        if (AIRMaxQuality != null)
            AIRMaxQuality.value = device.airMaxQuality;

        if (AIRMaxCapacity != null)
            AIRMaxCapacity.value = device.airMaxCapacity;

        if (noiseFloorText != null)
            noiseFloorText.text = device.noiseFloor.ToString("F2") + " dBm";

        if (transmitCCQText != null)
            transmitCCQText.text = device.transmitCCQ.ToString("F2") + " %";

        if (device.mode == WirelessDevice.WirelessMode.AccessPoint)
        {
            if (APMacText != null)
                APMacText.text = device.GetAPMacAddress();

            if (connectionText != null)
                connectionText.text = device.GetConnectedStationsCount() + " Station(s)";
        }
        else
        {
            if (signalStrengthText != null)
                signalStrengthText.text = device.currentSignal.ToString("F2") + " dBm";

            if (horizonalVerticalText != null)
                horizonalVerticalText.text = device.horizontalSignal.ToString("F2") + " / " + device.verticalSignal.ToString("F2") + " dBm";

            if (TXRXText != null)
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

        Color currentColor = activeBars <= 3 ? weakColor : activeBars <= 6 ? fairColor : strongColor;

        for (int i = 0; i < signalBars.Length; i++)
        {
            signalBars[i].color = i < activeBars ? currentColor : offColor;
        }
    }

    public void UpdateSignalBarsPersenQuality(float currentSignalPersen)
    {
        if (signalBarsPersenQuality == null || signalBarsPersenQuality.Length == 0) return;

        int activeBars = Mathf.Clamp((int)(currentSignalPersen / 10f), 0, 10);
        Color currentColor = activeBars <= 3 ? weakColor : activeBars <= 6 ? fairColor : strongColor;

        for (int i = 0; i < signalBarsPersenQuality.Length; i++)
        {
            signalBarsPersenQuality[i].color = i < activeBars ? currentColor : offColor;
        }
    }

    public void UpdateSignalBarsPersenCapacity(float currentSignalPersen)
    {
        if (signalBarsPersenCapacity == null || signalBarsPersenCapacity.Length == 0) return;

        int activeBars = Mathf.Clamp((int)(currentSignalPersen / 10f), 0, 10);
        Color currentColor = activeBars <= 3 ? weakColor : activeBars <= 6 ? fairColor : strongColor;

        for (int i = 0; i < signalBarsPersenCapacity.Length; i++)
        {
            signalBarsPersenCapacity[i].color = i < activeBars ? currentColor : offColor;
        }
    }

    void RefreshWirelessUI()
    {
        if (device == null) return;

        bool isAP = device.mode == WirelessDevice.WirelessMode.AccessPoint;
        bool isStation = device.mode == WirelessDevice.WirelessMode.Station;

        if (WDSToggle != null) WDSToggle.gameObject.SetActive(true);
        if (countryCodeDropdown != null) countryCodeDropdown.gameObject.SetActive(true);
        if (IEE80211Dropdown != null) IEE80211Dropdown.gameObject.SetActive(true);
        if (channelWidthDropdown != null) channelWidthDropdown.gameObject.SetActive(true);
        if (freqScanToggle != null) freqScanToggle.gameObject.SetActive(true);
        if (antennaGainInput != null) antennaGainInput.gameObject.SetActive(true);
        if (outputPowerSlider != null) outputPowerSlider.gameObject.SetActive(true);
        if (dataRateModuleDropdown != null) dataRateModuleDropdown.gameObject.SetActive(true);
        if (maxTXRateDropdown != null) maxTXRateDropdown.gameObject.SetActive(true);
        if (securityDropdown != null) securityDropdown.gameObject.SetActive(true);
        if (changeButton != null) changeButton.gameObject.SetActive(true);

        if (SSIDInput != null) SSIDInput.gameObject.SetActive(isAP);
        if (frequencyDropdownText != null) frequencyDropdownText.gameObject.SetActive(isAP);
        if (frequencyDropdown != null) frequencyDropdown.gameObject.SetActive(isAP);
        if (extensionChannelDropdownText != null) extensionChannelDropdownText.gameObject.SetActive(isAP);
        if (extensionChannelDropdown != null) extensionChannelDropdown.gameObject.SetActive(isAP);
        if (calculateEIRPToggle != null) calculateEIRPToggle.gameObject.SetActive(isAP);
        if (calculateEIRPToggleText != null) calculateEIRPToggleText.gameObject.SetActive(isAP);

        if (networkDropdown != null) networkDropdown.gameObject.SetActive(isStation);
        if (lockToAPInput != null) lockToAPInput.gameObject.SetActive(isStation);
        if (lockToAPInputText != null) lockToAPInputText.gameObject.SetActive(isStation);
        if (channelShiftDropdown != null) channelShiftDropdown.gameObject.SetActive(isStation);
        if (channelShiftDropdownText != null) channelShiftDropdownText.gameObject.SetActive(isStation);
        if (autoEIRPToggleText != null) autoEIRPToggleText.gameObject.SetActive(isStation);
        if (autoEIRPToggle != null) autoEIRPToggle.gameObject.SetActive(isStation);
        if (scanButton != null) scanButton.gameObject.SetActive(isStation);
    }

    void RefreshNetworkUI()
    {
        if (device == null) return;

        if (networkModeDropdown != null)
            networkModeDropdown.value = (int)device.networkMode;

        if (disableNetworkDropdown != null)
            disableNetworkDropdown.value = device.disableNetwork ? 1 : 0;

        if (ipAddressInput != null)
            ipAddressInput.text = device.ipAddress;

        if (netmaskInput != null)
            netmaskInput.text = device.netmask;

        if (gatewayInput != null)
            gatewayInput.text = device.gateway;

        if (primaryDNSInput != null)
            primaryDNSInput.text = device.primaryDNS;

        if (secondaryDNSInput != null)
            secondaryDNSInput.text = device.secondaryDNS;

        if (mtuInput != null)
            mtuInput.text = device.mtu.ToString();
    }
    #endregion

    #region UI Event Handlers
    void LoadAllUIFromDevice()
    {
        if (device == null) return;

        if (modeDropdown != null)
            modeDropdown.SetValueWithoutNotify((int)device.mode);

        if (SSIDInput != null)
            SSIDInput.text = device.ssid;

        if (IEE80211Dropdown != null)
            IEE80211Dropdown.SetValueWithoutNotify((int)device.ieee80211Mode);

        if (channelWidthDropdown != null)
            channelWidthDropdown.SetValueWithoutNotify((int)device.channelWidth);

        if (channelShiftDropdown != null)
            channelShiftDropdown.SetValueWithoutNotify((int)device.channelShift);

        if (antennaGainInput != null)
            antennaGainInput.text = device.antennaGain.ToString("F0");

        if (outputPowerSlider != null)
            outputPowerSlider.value = device.outputPower;

        if (outputPowerText != null)
            outputPowerText.text = device.outputPower.ToString("F0");

        if (WDSToggle != null)
            WDSToggle.isOn = device.WDS;

        if (freqScanToggle != null)
            freqScanToggle.isOn = device.freqScan;

        if (calculateEIRPToggle != null)
            calculateEIRPToggle.isOn = device.calculateEIRP;

        if (autoEIRPToggle != null)
            autoEIRPToggle.isOn = device.autoEIRP;

        if (dataRateModuleDropdown != null)
            dataRateModuleDropdown.SetValueWithoutNotify((int)device.dataRateModule);

        if (maxTXRateDropdown != null)
            maxTXRateDropdown.SetValueWithoutNotify((int)device.maxTXRate);

        if (autoMaxTXRateToggle != null)
            autoMaxTXRateToggle.isOn = device.autoMaxTXRate;

        if (securityDropdown != null)
            securityDropdown.SetValueWithoutNotify((int)device.securityMode);

        if (WPAAuthDropdown != null)
            WPAAuthDropdown.SetValueWithoutNotify((int)device.wpaAuthMode);

        if (passwordTMP_InputField != null)
            passwordTMP_InputField.text = device.password;

        if (lockToAPInput != null)
            lockToAPInput.text = device.lockToAPSSID;

        if (networkModeDropdown != null)
            networkModeDropdown.SetValueWithoutNotify((int)device.networkMode);

        if (disableNetworkDropdown != null)
            disableNetworkDropdown.SetValueWithoutNotify(device.disableNetwork ? 1 : 0);

        if (ipAddressInput != null)
            ipAddressInput.text = device.ipAddress;

        if (netmaskInput != null)
            netmaskInput.text = device.netmask;

        if (gatewayInput != null)
            gatewayInput.text = device.gateway;

        if (primaryDNSInput != null)
            primaryDNSInput.text = device.primaryDNS;

        if (secondaryDNSInput != null)
            secondaryDNSInput.text = device.secondaryDNS;

        if (mtuInput != null)
            mtuInput.text = device.mtu.ToString();

        if (nameDeviceInput != null)
            nameDeviceInput.text = device.deviceName;
    }

    void OnModeChanged(int value)
    {
        if (device == null) return;

        device.SetMode(value);

        StopAutoScan();

        RefreshWirelessUI();
        RefreshNetworkUI();
        RefreshMainUI();

        if (device.mode == WirelessDevice.WirelessMode.Station)
        {
            if (networkDropdown != null)
            {
                networkDropdown.ClearOptions();
                List<string> defaultOpt = new List<string> { "Click 'Scan' to find AP..." };
                networkDropdown.AddOptions(defaultOpt);
            }
        }
    }

    void OnScanButtonClicked()
    {
        if (device == null || device.mode != WirelessDevice.WirelessMode.Station) return;

        Scan();
        StartAutoScan();
    }

    public void OnSecurityModeChanged(int dropdownIndex)
    {
        if (securityDropdown == null) return;

        int securityModeIndex = securityDropdown.value;
        bool isSecure = securityModeIndex == 1;

        if (WPAAuthDropdownText != null) WPAAuthDropdownText.gameObject.SetActive(isSecure);
        if (WPAttk2 != null) WPAttk2.gameObject.SetActive(isSecure);
        if (WPAAuthDropdown != null) WPAAuthDropdown.gameObject.SetActive(isSecure);

        if (!isSecure)
        {
            if (passwordText != null) passwordText.gameObject.SetActive(false);
            if (Passttk2 != null) Passttk2.gameObject.SetActive(false);
            if (passwordTMP_InputField != null) passwordTMP_InputField.gameObject.SetActive(false);
        }
    }

    public void OnWPAAuthModeChanged(int dropdownIndex)
    {
        if (WPAAuthDropdown == null) return;

        int wpaAuthModeIndex = WPAAuthDropdown.value;
        bool isPSK = wpaAuthModeIndex == 1;

        if (passwordText != null) passwordText.gameObject.SetActive(isPSK);
        if (Passttk2 != null) Passttk2.gameObject.SetActive(isPSK);
        if (passwordTMP_InputField != null) passwordTMP_InputField.gameObject.SetActive(isPSK);
    }

    public void Connect()
    {
        if (device == null) return;
        if (scanned.Count == 0 || networkDropdown == null) return;

        int index = networkDropdown.value;
        if (index < 0 || index >= scanned.Count) return;

        WirelessDevice ap = scanned[index];
        device.Connect(ap, passwordTMP_InputField != null ? passwordTMP_InputField.text : "");

        RefreshWirelessUI();
        RefreshMainUI();
    }

    public void Scan()
    {
        scanned.Clear();

        if (networkDropdown == null || device == null) return;

        networkDropdown.ClearOptions();
        List<string> options = new List<string>();

        foreach (WirelessDevice ap in NetworksRegistry.activeAPs)
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
        if (device == null) return;

        if (WDSToggle != null)
            device.WDS = WDSToggle.isOn;

        if (countryCodeDropdown != null && countryCodeDropdown.options.Count > countryCodeDropdown.value)
            device.countryCode = countryCodeDropdown.options[countryCodeDropdown.value].text;

        if (IEE80211Dropdown != null)
            device.ieee80211Mode = (WirelessDevice.IEEE80211Mode)IEE80211Dropdown.value;

        if (channelWidthDropdown != null)
            device.channelWidth = (WirelessDevice.ChannelWidth)channelWidthDropdown.value;

        if (freqScanToggle != null)
            device.freqScan = freqScanToggle.isOn;

        if (antennaGainInput != null && float.TryParse(antennaGainInput.text, out float antennaGain))
            device.antennaGain = antennaGain;

        if (outputPowerSlider != null)
            device.outputPower = outputPowerSlider.value;

        if (dataRateModuleDropdown != null)
            device.dataRateModule = (WirelessDevice.DataRateModule)dataRateModuleDropdown.value;

        if (maxTXRateDropdown != null)
            device.maxTXRate = (WirelessDevice.MaxTXRate)maxTXRateDropdown.value;

        if (autoMaxTXRateToggle != null)
            device.autoMaxTXRate = autoMaxTXRateToggle.isOn;

        if (securityDropdown != null)
            device.securityMode = (WirelessDevice.SecurityMode)securityDropdown.value;

        if (WPAAuthDropdown != null)
            device.wpaAuthMode = (WirelessDevice.WPAAuthMode)WPAAuthDropdown.value;

        if (passwordTMP_InputField != null)
            device.password = passwordTMP_InputField.text;

        if (device.mode == WirelessDevice.WirelessMode.AccessPoint)
        {
            if (SSIDInput != null)
                device.ssid = SSIDInput.text;

            if (frequencyDropdown != null)
                device.frequency = frequencyDropdown.value == 0 ? 2412f : 5180f;

            if (extensionChannelDropdown != null)
                device.extensionChannel = (WirelessDevice.ExtensionChannel)extensionChannelDropdown.value;

            if (calculateEIRPToggle != null)
                device.calculateEIRP = calculateEIRPToggle.isOn;
        }
        else
        {
            if (lockToAPInput != null)
                device.lockToAPSSID = lockToAPInput.text;

            if (channelShiftDropdown != null)
                device.channelShift = (WirelessDevice.ChannelShift)channelShiftDropdown.value;

            if (autoEIRPToggle != null)
                device.autoEIRP = autoEIRPToggle.isOn;

            if (scanned.Count > 0 && networkDropdown != null && networkDropdown.value < scanned.Count)
            {
                device.ssid = scanned[networkDropdown.value].ssid;
            }
        }

        RefreshWirelessUI();
        RefreshMainUI();
        Connect();
    }

    public void changeNetwork()
    {
        if (device == null) return;

        if (ipAddressInput != null)
            device.ipAddress = ipAddressInput.text;

        if (netmaskInput != null)
            device.netmask = netmaskInput.text;

        if (gatewayInput != null)
            device.gateway = gatewayInput.text;

        if (primaryDNSInput != null)
            device.primaryDNS = primaryDNSInput.text;

        if (secondaryDNSInput != null)
            device.secondaryDNS = secondaryDNSInput.text;

        if (mtuInput != null && int.TryParse(mtuInput.text, out int mtu))
            device.mtu = mtu;

        RefreshNetworkUI();
        RefreshMainUI();
    }

    public void changeSystem()
    {
        if (device == null) return;

        if (nameDeviceInput != null)
        {
            device.SetDeviceName(nameDeviceInput.text);
            nameDeviceInput.text = device.deviceName;
        }

        RefreshMainUI();
    }
    #endregion

    #region AutoScan Coroutines
    void StartAutoScan()
    {
        if (autoScanRoutine != null)
            StopCoroutine(autoScanRoutine);

        autoScanRoutine = StartCoroutine(AutoScan());
    }

    void StopAutoScan()
    {
        if (autoScanRoutine != null)
        {
            StopCoroutine(autoScanRoutine);
            autoScanRoutine = null;
        }
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