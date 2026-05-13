using UnityEngine;
using HPhysic;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkDevice))]
public class WirelessDevice : MonoBehaviour
{
    [Header("Obstacle Settings")]
    public LayerMask obstacleLayer;

    // ================= ENUM =================
    public enum WirelessMode { None, Station, AccessPoint }
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

    public Sprite deviceIcon;

    [HideInInspector] public NetworkDevice networkDevice;
    private bool lastInternetState = false;

    // ================= GENERAL =================
    public string deviceName;
    public bool isConnected;

    public WirelessDevice connectedAP;
    public List<WirelessDevice> connectedClients = new List<WirelessDevice>();

    public string ipAddress = "0.0.0.0";
    public string gateway = "0.0.0.0";

    // 🔥 MAC ADDRESS
    public string wlan0Mac;
    public string lan0Mac;
    public string lan1Mac;

    // ================= NETWORK =================
    public NetworkMode networkMode;
    public bool disableNetwork;

    public string netmask = "0.0.0.0";
    public string primaryDNS = "0.0.0.0";
    public string secondaryDNS = "0.0.0.0";
    public int mtu = 1500;

    // ================= POWER =================
    public bool hasPower;

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

    public float latency;
    public float packetLoss;

    // ================= AIRMAX =================
    public float airMaxQuality = 100f;
    public float airMaxCapacity = 100f;

    // ================= LAN =================
    public string lan0Status = "Unplugged";
    public string lan1Status = "Unplugged";

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

    // ================= Kabel =================

    [Header("PORT REFERENCES")]
    public Connector lan0Connector;
    public Connector lan1Connector;

    // ================= UNITY =================
    void Awake()
    {
        networkDevice = GetComponent<NetworkDevice>();

        deviceName = gameObject.name;

        string baseMac = GenerateMacAddress();
        wlan0Mac = baseMac;
        lan0Mac = IncrementMac(baseMac, 5);
        lan1Mac = IncrementMac(baseMac, 6);
    }

    public void SetDeviceName(string newName)
    {
        deviceName = newName;
        gameObject.name = newName;
    }

    string IncrementMac(string mac, int increment)
    {
        string[] parts = mac.Split(':');
        int lastByte = System.Convert.ToInt32(parts[5], 16);
        lastByte = (lastByte + increment) % 256;
        parts[5] = lastByte.ToString("X2");
        return string.Join(":", parts);
    }

    string GenerateMacAddress()
    {
        byte[] mac = new byte[6];

        mac[0] = 0x04;
        mac[1] = 0x18;
        mac[2] = 0xD6;

        System.Random rand = new System.Random();
        mac[3] = (byte)rand.Next(0, 256);
        mac[4] = (byte)rand.Next(0, 256);
        mac[5] = (byte)rand.Next(0, 256);

        return System.BitConverter.ToString(mac).Replace("-", ":");
    }

    // CATATAN: Pastikan class NetworksRegistry milikmu tidak bermasalah
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
        UpdateLANStatus();
        currentEIRP = GetEIRP();

        hasPower = networkDevice.hasPower;

        if (!hasPower)
        {
            isConnected = false;
            currentSignal = -120f;
            txRate = 0; rxRate = 0; transmitCCQ = 0; latency = 0; packetLoss = 100;

            if (mode == WirelessMode.Station)
                SetStationInternetBridge(false);

            return;
        }

        noiseFloor = -95f + Random.Range(-5f, 5f);

        // 2. === ACCESS POINT MODE ===
        if (mode == WirelessMode.AccessPoint)
        {
            return;
        }

        // 3. === STATION MODE ===
        if (connectedAP == null)
        {
            isConnected = false;
            SetStationInternetBridge(false);
            return;
        }

        currentSignal = CalculateSignal(connectedAP);
        horizontalSignal = currentSignal + Random.Range(-2f, 2f);
        verticalSignal = currentSignal + Random.Range(-2f, 2f);

        float signalQuality = Mathf.InverseLerp(-90f, -40f, currentSignal);
        float noiseImpact = Mathf.InverseLerp(-100f, -70f, noiseFloor);
        float baseRate = (channelWidth == ChannelWidth.CW40Mhz) ? 300f : 150f;
        float distance = Vector3.Distance(transform.position, connectedAP.transform.position);
        float distanceFactor = Mathf.Clamp01(1f - (distance / 200f));
        float qualityFactor = Mathf.Clamp01(signalQuality * distanceFactor * (1f - noiseImpact));

        transmitCCQ = qualityFactor * 100f;

        float snr = currentSignal - noiseFloor;
        float snrFactor = Mathf.InverseLerp(0f, 40f, snr);

        txRate = baseRate * snrFactor;
        rxRate = baseRate * snrFactor;

        latency = 1f + (1f - qualityFactor) * 100f + Random.Range(0f, 5f);
        packetLoss = Mathf.Clamp01(1f - qualityFactor) * 100f;

        if (currentSignal < DisconnectThreshold)
        {
            Debug.Log($"🔴 DISCONNECTED (Signal {currentSignal:F1} dBm)");
            Disconnect();
            return;
        }

        isConnected = true;

        bool validNetwork = IsSameSubnet(ipAddress, connectedAP.ipAddress, netmask) && gateway == connectedAP.ipAddress;
        
        bool apHasInternet = connectedAP.hasPower && connectedAP.networkDevice.hasInternet;

        bool internetState = validNetwork && apHasInternet;

        SetStationInternetBridge(internetState);
    }

    void UpdateLANStatus()
    {
        lan0Status = (lan0Connector != null && lan0Connector.ConnectedTo != null) ? "Connected" : "Unplugged";
        lan1Status = (lan1Connector != null && lan1Connector.ConnectedTo != null) ? "Connected" : "Unplugged";
    }
    
    public void SetMode(int modeIndex)
    {
        NetworksRegistry.UnregisterAP(this);
        Disconnect();

        mode = (WirelessMode)modeIndex;

        if (mode == WirelessMode.AccessPoint)
            NetworksRegistry.RegisterAP(this);
    }

    float GetObstacleLoss(WirelessDevice ap)
    {
        Vector3 dir = (ap.transform.position - transform.position);
        float distance = dir.magnitude;

        Ray ray = new Ray(transform.position, dir.normalized);

        RaycastHit[] hits = Physics.RaycastAll(ray, distance, obstacleLayer);

        float totalLoss = 0f;

        foreach (var hit in hits)
        {
            ObstacleMaterial obstacle = hit.collider.GetComponent<ObstacleMaterial>();

            if (obstacle != null)
            {
                float loss = obstacle.GetLoss();
                totalLoss += loss;

                Debug.Log($"🧱 {hit.collider.name} | {obstacle.materialType} | Loss: {loss} dB");
            }
        }

        return totalLoss;
    }

    public void Connect(WirelessDevice ap, string inputPassword)
    {
        if (!hasPower)
        {
            Debug.Log("❌ Device tidak punya power (PoE)");
            return;
        }

        if (!ap.hasPower)
        {
            Debug.Log("❌ AP mati (tidak ada power)");
            return;
        }

        if (mode != WirelessMode.Station) return;
        if (ap == null) return;
        if (ap.mode != WirelessMode.AccessPoint) return;

        if (!string.IsNullOrEmpty(lockToAPSSID) && ap.ssid != lockToAPSSID)
            return;

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
    }

    public void Disconnect()
    {
        if (connectedAP != null)
            connectedAP.connectedClients.Remove(this);

        connectedAP = null;
        isConnected = false;

        ipAddress = "0.0.0.0";
        gateway = "0.0.0.0";

        SetStationInternetBridge(false);
    }

    void SetStationInternetBridge(bool state)
    {
        if (lastInternetState != state)
        {
            lastInternetState = state;

            if (state && connectedAP != null)
                Debug.Log($"🟢 {deviceName} GOT INTERNET via {connectedAP.deviceName} (Wireless Bridge Active)");
            else
                Debug.Log($"🔴 {deviceName} LOST INTERNET (Wireless Bridge Closed)");

            if (InternetManager.Instance != null)
                InternetManager.Instance.RefreshNetworkState();
        }
    }

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

    bool IsSameSubnet(string ip1, string ip2, string netmask)
    {
        string[] ipA = ip1.Split('.');
        string[] ipB = ip2.Split('.');
        string[] mask = netmask.Split('.');

        if (ipA.Length != 4 || ipB.Length != 4 || mask.Length != 4) return false;

        for (int i = 0; i < 4; i++)
        {
            int a = int.Parse(ipA[i]) & int.Parse(mask[i]);
            int b = int.Parse(ipB[i]) & int.Parse(mask[i]);

            if (a != b)
                return false;
        }

        return true;
    }

    float GetChannelInterference(WirelessDevice ap)
    {
        int channelDiff = Mathf.Abs(channel - ap.channel);

        if (channelDiff == 0) return 15f;
        if (channelDiff <= 2) return 8f;
        return 0f;
    }

    float CalculateSignal(WirelessDevice ap)
    {
        float distance = Vector3.Distance(transform.position, ap.transform.position);
        if (distance < 0.1f) distance = 0.1f;

        float distanceKM = distance / 1000f;


        float fspl =
            20f * Mathf.Log10(distanceKM) +
            20f * Mathf.Log10(frequency) -
            27.55f;

        float txLoss = ap.CalculateAngleLoss(transform);
        float rxLoss = CalculateAngleLoss(ap.transform);

        float interference = GetChannelInterference(ap);

        float obstacleLoss = GetObstacleLoss(ap);

        float receivedPower =
            ap.GetEIRP()
            + antennaGain
            - fspl
            - txLoss
            - rxLoss
            - interference
            - obstacleLoss;

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
        return wlan0Mac;
    }
}