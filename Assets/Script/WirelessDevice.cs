using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkDevice))]
public class WirelessDevice : MonoBehaviour
{
    // ================= ENUM =================
    public enum WirelessMode { Station, AccessPoint, APRepeater }
    public enum IEEE80211Mode { AN_MIXED }
    public enum ChannelWidth { Auto, CW20Mhz, CW40Mhz }
    public enum ChannelShift { None }
    public enum ExtensionChannel { None }
    public enum DataRateModule { Default }
    public enum MaxTXRate { MCS15_270_300 }
    public enum SecurityMode { None, WPA2_AES }
    public enum WPAAuthMode { None, PSK }
    public enum NetworkMode { Bridge }

    // ================= MODE =================
    public WirelessMode mode = WirelessMode.Station;

    private NetworkDevice networkDevice;
    private bool lastInternetState = false;

    // ================= GENERAL =================
    public string deviceName = "NanoStation";
    public bool isConnected;

    public WirelessDevice connectedAP;
    public List<WirelessDevice> connectedClients = new List<WirelessDevice>();

    public string ipAddress = "0.0.0.0";
    public string gateway = "-";

    // ================= NETWORK =================
    public NetworkMode networkMode;
    public bool disableNetwork;

    public string netmask = "255.255.255.0";
    public string primaryDNS = "8.8.8.8";
    public string secondaryDNS = "8.8.4.4";
    public int mtu = 1500;

    public bool useDHCP = true;       // DHCP / Static
    public bool isDHCPServer = true;  // khusus AP

    // ================= DHCP =================
    public string dhcpStartIP = "192.168.1.2";
    public string dhcpEndIP = "192.168.1.254";
    private int dhcpCounter = 2;

    // ================= RADIO =================
    public float outputPower = 10f;
    public float antennaGain = 16f;
    public float frequency = 2400f;
    public float beamWidth = 60f;

    public ChannelWidth channelWidth;
    public ChannelShift channelShift;

    public float currentSignal;
    public float currentEIRP;

    // ================= SIGNAL =================
    public float horizontalSignal;
    public float verticalSignal;
    public float noiseFloor = -95f;

    public float txRate;
    public float rxRate;
    public float transmitCCQ = 100f;

    // ================= AIRMAX =================
    public float airMaxQuality = 100f;
    public float airMaxCapacity = 100f;

    // ================= LAN =================
    public string lan0Status = "Connected";
    public string lan1Status = "Connected";

    // ================= WIRELESS =================
    public string ssid = "";
    public string password = "";

    public bool WDS;
    public string countryCode;

    public IEEE80211Mode ieee80211Mode;
    public ExtensionChannel extensionChannel;
    public int channel = 5;

    public bool freqScan;
    public bool calculateEIRP;
    public bool autoEIRP;

    public DataRateModule dataRateModule;
    public MaxTXRate maxTXRate;
    public bool autoMaxTXRate;

    public SecurityMode securityMode;
    public WPAAuthMode wpaAuthMode;

    // ================= STATION =================
    public string lockToAPSSID;

    // ================= CONST =================
    const float DisconnectThreshold = -90f;
    const float MaxEIRP_Indonesia = 36f;

    // ================= UNITY =================
    void Awake()
    {
        networkDevice = GetComponent<NetworkDevice>();
    }

    void OnEnable()
    {
        if (mode == WirelessMode.AccessPoint)
            NetworksRegistry.RegisterAP(this);
    }

    void OnDisable()
    {
        NetworksRegistry.UnregisterAP(this);
    }

    void Update()
    {
        currentEIRP = GetEIRP();

        if (mode != WirelessMode.Station)
            return;

        if (connectedAP == null)
        {
            isConnected = false;
            SetInternet(false);
            return;
        }

        currentSignal = CalculateSignal(connectedAP);

        horizontalSignal = currentSignal + Random.Range(-2f, 2f);
        verticalSignal = currentSignal + Random.Range(-2f, 2f);

        txRate = Random.Range(50f, 150f);
        rxRate = Random.Range(50f, 150f);

        if (currentSignal < DisconnectThreshold)
        {
            Debug.Log($"🔴 DISCONNECTED (Signal weak: {currentSignal:F1} dBm)");
            Disconnect();
            return;
        }

        isConnected = true;

        bool internetState = connectedAP.networkDevice.hasInternet;
        SetInternet(internetState);
    }

    // ================= MODE =================
    public void SetMode(int modeIndex)
    {
        NetworksRegistry.UnregisterAP(this);
        Disconnect();

        mode = (WirelessMode)modeIndex;

        if (mode == WirelessMode.AccessPoint)
        {
            NetworksRegistry.RegisterAP(this);

            if (useDHCP)
            {
                ipAddress = "192.168.0." + Random.Range(2, 254);
                gateway = "192.168.0.1";
                Debug.Log($"🌐 AP DHCP IP → {ipAddress}");
            }
            else
            {
                ipAddress = "192.168.1.1";
                gateway = "-";
                Debug.Log("🟢 AP Static IP → 192.168.1.1");
            }
        }
        else
        {
            if (useDHCP)
                ipAddress = "0.0.0.0";

            Debug.Log("🟡 Mode Station aktif");
        }
    }

    // ================= DHCP =================
    string GenerateDHCPIP()
    {
        string[] parts = ipAddress.Split('.');
        string baseIP = parts[0] + "." + parts[1] + "." + parts[2] + ".";
        string ip = baseIP + dhcpCounter;

        dhcpCounter++;
        if (dhcpCounter > 254) dhcpCounter = 2;

        return ip;
    }

    // ================= CONNECTION =================
    public void Connect(WirelessDevice ap, string inputPassword)
    {
        if (mode != WirelessMode.Station) return;
        if (ap == null) return;
        if (ap.mode != WirelessMode.AccessPoint) return;

        if (!string.IsNullOrEmpty(lockToAPSSID) && ap.ssid != lockToAPSSID)
        {
            Debug.Log("❌ SSID tidak sesuai lock");
            return;
        }

        if (ap.securityMode != SecurityMode.None && ap.password != inputPassword)
        {
            Debug.Log("❌ Password salah");
            return;
        }

        if (connectedAP != null)
            connectedAP.connectedClients.Remove(this);

        connectedAP = ap;
        ap.connectedClients.Add(this);

        isConnected = true;

        // ===== IP LOGIC =====
        if (useDHCP)
        {
            if (ap.isDHCPServer)
            {
                ipAddress = ap.GenerateDHCPIP();
                gateway = ap.ipAddress;
                primaryDNS = ap.primaryDNS;
                secondaryDNS = ap.secondaryDNS;

                Debug.Log($"📡 DHCP → {ipAddress}");
            }
            else
            {
                Debug.Log("❌ AP tidak punya DHCP");
                ipAddress = "0.0.0.0";
            }
        }
        else
        {
            Debug.Log($"📡 Static IP → {ipAddress}");
        }

        SetInternet(ap.networkDevice.hasInternet);
    }

    public void Disconnect()
    {
        if (connectedAP != null)
            connectedAP.connectedClients.Remove(this);

        connectedAP = null;
        isConnected = false;

        if (useDHCP)
        {
            ipAddress = "0.0.0.0";
            gateway = "-";
        }

        SetInternet(false);
    }

    // ================= INTERNET =================
    void SetInternet(bool state)
    {
        networkDevice.hasInternet = state;

        if (lastInternetState != state)
        {
            lastInternetState = state;

            if (state && connectedAP != null)
                Debug.Log($"🟢 GOT INTERNET via {connectedAP.deviceName}");
            else
                Debug.Log("🔴 LOST INTERNET");
        }
    }

    // ================= SIGNAL =================
    public float GetEIRP()
    {
        float eirp = outputPower + antennaGain;

        if (calculateEIRP && eirp > MaxEIRP_Indonesia)
        {
            outputPower -= (eirp - MaxEIRP_Indonesia);
            eirp = MaxEIRP_Indonesia;
        }

        return eirp;
    }

    float CalculateSignal(WirelessDevice ap)
    {
        float distance = Vector3.Distance(transform.position, ap.transform.position);
        if (distance < 0.1f) distance = 0.1f;

        float fspl =
            20f * Mathf.Log10(distance) +
            20f * Mathf.Log10(frequency) -
            27.55f;

        float txLoss = ap.CalculateAngleLoss(transform);
        float rxLoss = CalculateAngleLoss(ap.transform);

        float receivedPower =
            ap.GetEIRP()
            + antennaGain
            - fspl
            - txLoss
            - rxLoss;

        return receivedPower;
    }

    float CalculateAngleLoss(Transform target)
    {
        Vector3 dir = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dir);

        if (angle <= beamWidth / 2f)
        {
            float ratio = angle / (beamWidth / 2f);
            return ratio * ratio * 10f;
        }

        return 25f;
    }

    // ================= EXTRA =================
    public string GetFrequencyBand()
    {
        return frequency < 3000 ? "2.4 GHz" : "5 GHz";
    }

    public float CalculateDistance()
    {
        if (connectedAP == null) return 0f;
        return Vector3.Distance(transform.position, connectedAP.transform.position);
    }

    public int GetConnectedStationsCount()
    {
        return connectedClients.Count;
    }

    public string GetAPMacAddress()
    {
        return "00:11:22:33:44:55";
    }
}