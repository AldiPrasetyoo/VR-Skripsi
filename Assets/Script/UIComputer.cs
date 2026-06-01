using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_ComputerDevice : MonoBehaviour
{
    [Header("Device")]
    public ComputerDevice device;

    // ================= STATUS =================
    [Header("Status UI")]
    public GameObject statusPanel;
    public TMP_Text judulText;
    public TMP_Text connectionText;
    public TMP_Text internetText;
    public TMP_Text portText;
    public TMP_Text ipText;
    public TMP_Text gatewayText;

    // ================= SETTING =================
    [Header("Setting Panel")]
    public GameObject settingPanel;
    public TMP_Text judulSettingText;
    // public TMP_InputField deviceNameInput;
    public TMP_InputField ipInput;
    public TMP_InputField gatewayInput;
    public TMP_InputField netmaskInput;

    // ================= TEST =================
    [Header("Test Result")]
    public TMP_Text testResultText;

    void Start()
    {
        LoadFromDevice();
        statusPanel.SetActive(true);
        settingPanel.SetActive(false);

    }

    void Update()
    {
        UpdateStatusUI();
    }

    // ================= LOAD =================
    void LoadFromDevice()
    {
        if (device == null) return;

        judulText.text = "Device Status (" + device.gameObject.name + ")";
        judulSettingText.text = "Device Settings (" + device.gameObject.name + ")";

        ipInput.text = device.ipAddress;
        gatewayInput.text = device.gateway;
        netmaskInput.text = device.netmask;
    }

    // ================= EDIT =================
    public void OnClickEdit()
    {
        settingPanel.SetActive(true);
        statusPanel.SetActive(false);

        // deviceNameInput.text = device.gameObject.name;
        ipInput.text = device.ipAddress;
        gatewayInput.text = device.gateway;
        netmaskInput.text = device.netmask;
    }

    // ================= APPLY =================
    public void OnClickApply()
    {
        if (device == null) return;

        device.SetIP(ipInput.text);
        // device.SetDeviceName(deviceNameInput.text);
        device.SetGateway(gatewayInput.text);
        device.SetNetmask(netmaskInput.text);

        settingPanel.SetActive(false);
        statusPanel.SetActive(true);

        Debug.Log("⚙️ Config Applied");
    }

    // ================= STATUS =================
void UpdateStatusUI()
{
    if (device == null) return;

    judulText.text = "Device Status (" + device.gameObject.name + ")";

    // ================= CONNECTION =================
    if (!device.isConnected)
    {
        connectionText.text = "Disconnected";
        connectionText.color = Color.red;
    }
    else if (!device.networkValid)
    {
        connectionText.text = "Connected (Invalid)";
        connectionText.color = Color.yellow;
    }
    else
    {
        connectionText.text = "Connected";
        connectionText.color = Color.green;
    }

    // ================= INTERNET =================
    if (device.hasInternet)
    {
        internetText.text = "Internet Access";
        internetText.color = Color.green;
    }
    else if (device.isConnected) // sudah connect tapi belum internet
    {
        internetText.text = "No Internet Access";
        internetText.color = Color.yellow;
    }
    else
    {
        internetText.text = "No Internet Access";
        internetText.color = Color.red;
    }

    // ================= IP & GATEWAY =================
    ipText.text = device.ipAddress;
    gatewayText.text = device.gateway;

    // ================= PORT =================
    if (device.lanConnector != null && device.lanConnector.ConnectedTo != null)
        portText.text = "Port " + device.lanConnector.gameObject.name;
    else
        portText.text = "Port: -";
}

    // ================= TEST =================
    public void OnClickTest()
    {
        if (device == null) return;

        testResultText.text = device.TestConnection();
    }

}