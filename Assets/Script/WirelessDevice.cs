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
    [Header("Mode Configuration")]
    public WirelessMode mode = WirelessMode.None;
    public Sprite deviceIcon;

    [HideInInspector] public NetworkDevice networkDevice;
    private bool lastInternetState = false;

    // ================= GENERAL =================
    [Header("General Info")]
    public string deviceName;
    public bool isConnected;

    public WirelessDevice connectedAP;
    public List<WirelessDevice> connectedClients = new List<WirelessDevice>();

    public string ipAddress = "192.168.1.20";
    public string gateway = "192.168.1.1";

    // ================= HARDWARE ADDRESS =================
    [Header("Hardware Addresses")]
    public string wlan0Mac;
    public string lan0Mac;
    public string lan1Mac;

    // ================= NETWORK =================
    [Header("Network Settings")]
    public NetworkMode networkMode = NetworkMode.Bridge;
    public bool disableNetwork;

    public string netmask = "255.255.255.0";
    public string primaryDNS = "8.8.8.8";
    public string secondaryDNS = "8.8.4.4";
    public int mtu = 1500;

    // ================= POWER =================
    [Header("Status Variables")]
    public bool hasPower;

    // ================= STATUS UNTUK COMPUTER HOME UI =================
    [Header("Computer Access Status")]
    public bool physicalLinkActive;
    public bool ipConfigValid;

    private static readonly List<WirelessDevice> allWirelessDevices = new List<WirelessDevice>();

    // ================= RADIO =================
    [Header("Radio Settings - NanoStation M5 Indonesia")]
    [Range(0f, 27f)] public float outputPower = 10f;
    public float antennaGain = 16f;

    [Tooltip("Untuk AP: pilih frequency. Untuk Station: otomatis ikut frequency AP.")]
    public float frequency = 5745f;

    [Tooltip("Otomatis mengikuti frequency. 5745=149, 5765=153, 5785=157, 5805=161, 5825=165")]
    public int channel = 149;

    public float beamWidth = 60f;

    public ChannelWidth channelWidth = ChannelWidth.CW20Mhz;
    public ChannelShift channelShift;

    private static readonly float[] indonesiaNanoM5Frequencies =
    {
        5745f,
        5765f,
        5785f,
        5805f,
        5825f
    };

    private static readonly int[] indonesiaNanoM5Channels =
    {
        149,
        153,
        157,
        161,
        165
    };

    private const float FrequencyTolerance = 0.5f;

    // ================= REALTIME DIAGNOSTICS =================
    [Header("Realtime Diagnostics")]
    public float currentSignal = -120f;
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
    [Header("AirMax Status")]
    public float airMaxQuality = 100f;
    public float airMaxCapacity = 100f;

    // ================= LAN =================
    [Header("LAN Status")]
    public string lan0Status = "Unplugged";
    public string lan1Status = "Unplugged";

    // ================= WIRELESS =================
    [Header("Wireless Security")]
    public string ssid = "AIRNET-AP";
    public string password = "";

    public bool WDS = true;
    public string countryCode = "Indonesia";

    public IEEE80211Mode ieee80211Mode;
    public ExtensionChannel extensionChannel;

    public bool freqScan;
    public bool calculateEIRP = true;
    public bool autoEIRP;

    public DataRateModule dataRateModule;
    public MaxTXRate maxTXRate;
    public bool autoMaxTXRate;

    public SecurityMode securityMode;
    public WPAAuthMode wpaAuthMode;

    // ================= STATION =================
    [Header("Station Settings")]
    public string lockToAPSSID;

    // ================= CONST =================
    private const float DisconnectThreshold = -90f;
    private const float MaxEIRP_Indonesia = 36f;

    // ================= KABEL =================
    [Header("Port References")]
    public Connector lan0Connector;
    public Connector lan1Connector;

    // ================= UNITY METHODS =================

    void Awake()
    {
        networkDevice = GetComponent<NetworkDevice>();

        if (string.IsNullOrEmpty(deviceName))
        {
            deviceName = gameObject.name;
        }

        string baseMac = GenerateMacAddress();
        wlan0Mac = baseMac;
        lan0Mac = IncrementMac(baseMac, 1);
        lan1Mac = IncrementMac(baseMac, 2);

        SyncFrequencyAndChannel();
    }

    void OnValidate()
    {
        SyncFrequencyAndChannel();
    }

    void OnEnable()
    {
        if (!allWirelessDevices.Contains(this))
        {
            allWirelessDevices.Add(this);
        }

        if (mode == WirelessMode.AccessPoint)
        {
            NetworksRegistry.RegisterAP(this);
        }
    }

    void OnDisable()
    {
        if (allWirelessDevices.Contains(this))
        {
            allWirelessDevices.Remove(this);
        }

        NetworksRegistry.UnregisterAP(this);
        Disconnect();
    }

    void Update()
    {
        UpdateLANStatus();

        hasPower = networkDevice != null && networkDevice.hasPower;

        UpdateComputerAccessStatus();

        // AP menjaga frequency/channel sesuai pilihan Indonesia.
        // Station akan mengikuti AP kalau sudah connect.
        if (mode == WirelessMode.AccessPoint)
        {
            SyncFrequencyAndChannel();
        }
        else if (mode == WirelessMode.Station && connectedAP != null)
        {
            FollowAPFrequency();
        }

        currentEIRP = GetEIRP();

        // JIKA MATI / TIDAK ADA DAYA POE
        if (!hasPower)
        {
            ResetRadioStats();

            if (mode == WirelessMode.Station && isConnected)
            {
                Disconnect();
            }

            return;
        }

        noiseFloor = -95f + Random.Range(-1.5f, 1.5f);

        // ================= ACCESS POINT MODE =================
        if (mode == WirelessMode.AccessPoint)
        {
            isConnected = true;
            return;
        }

        // ================= STATION MODE =================
        if (connectedAP == null)
        {
            ResetRadioStats();
            SetStationInternetBridge(false);
            return;
        }

        // Station selalu ikut frequency/channel AP selama terkoneksi.
        FollowAPFrequency();

        currentSignal = CalculateSignal(connectedAP);
        horizontalSignal = currentSignal + Random.Range(-1f, 1f);
        verticalSignal = currentSignal + Random.Range(-1f, 1f);

        float signalQuality = Mathf.InverseLerp(-90f, -45f, currentSignal);
        float noiseImpact = Mathf.InverseLerp(-100f, -70f, noiseFloor);
        float baseRate = (channelWidth == ChannelWidth.CW40Mhz) ? 300f : 150f;

        float distance = Vector3.Distance(transform.position, connectedAP.transform.position);
        float distanceFactor = Mathf.Clamp01(1f - (distance / 2500f));
        float qualityFactor = Mathf.Clamp01(signalQuality * distanceFactor * (1f - noiseImpact));

        transmitCCQ = qualityFactor * 100f;
        airMaxQuality = transmitCCQ;
        airMaxCapacity = qualityFactor * 90f + Random.Range(0f, 10f);

        float snr = currentSignal - noiseFloor;
        float snrFactor = Mathf.InverseLerp(10f, 40f, snr);

        txRate = baseRate * snrFactor;
        rxRate = baseRate * snrFactor;

        latency = 1f + (1f - qualityFactor) * 80f + Random.Range(1f, 4f);
        packetLoss = Mathf.Clamp01(1f - qualityFactor) * 100f;

        if (currentSignal < DisconnectThreshold)
        {
            Debug.Log($"🔴 DISCONNECTED: Sinyal terlalu drop ({currentSignal:F1} dBm)");
            Disconnect();
            return;
        }

        isConnected = true;

        bool validNetwork = IsSameSubnet(ipAddress, connectedAP.ipAddress, netmask);
        bool apHasInternet = connectedAP.networkDevice != null && connectedAP.networkDevice.hasInternet;
        bool internetState = validNetwork && apHasInternet;

        SetStationInternetBridge(internetState);
    }

    // ================= FREQUENCY / CHANNEL INDONESIA =================

    public void SetIndonesiaFrequencyByIndex(int index)
    {
        index = Mathf.Clamp(index, 0, indonesiaNanoM5Frequencies.Length - 1);

        frequency = indonesiaNanoM5Frequencies[index];
        channel = indonesiaNanoM5Channels[index];

        Debug.Log($"📡 {deviceName} Frequency diset ke {frequency:0} MHz | Channel {channel}");
    }

    public void SetFrequencyMHz(float frequencyMHz)
    {
        int index = GetClosestIndonesiaFrequencyIndex(frequencyMHz);

        frequency = indonesiaNanoM5Frequencies[index];
        channel = indonesiaNanoM5Channels[index];

        Debug.Log($"📡 {deviceName} Frequency diset ke {frequency:0} MHz | Channel {channel}");
    }

    public int GetIndonesiaFrequencyIndex()
    {
        for (int i = 0; i < indonesiaNanoM5Frequencies.Length; i++)
        {
            if (Mathf.Abs(frequency - indonesiaNanoM5Frequencies[i]) <= FrequencyTolerance)
            {
                return i;
            }
        }

        return GetClosestIndonesiaFrequencyIndex(frequency);
    }

    public string GetFrequencyWithChannelText()
    {
        return $"{frequency:0} MHz / CH {channel}";
    }

    private void SyncFrequencyAndChannel()
    {
        int index = GetIndonesiaFrequencyIndex();

        frequency = indonesiaNanoM5Frequencies[index];
        channel = indonesiaNanoM5Channels[index];

        countryCode = "Indonesia";
    }

    private void FollowAPFrequency()
    {
        if (connectedAP == null)
        {
            return;
        }

        connectedAP.SyncFrequencyAndChannel();

        frequency = connectedAP.frequency;
        channel = connectedAP.channel;
    }

    private int GetClosestIndonesiaFrequencyIndex(float targetFrequency)
    {
        int closestIndex = 0;
        float closestDistance = Mathf.Abs(targetFrequency - indonesiaNanoM5Frequencies[0]);

        for (int i = 1; i < indonesiaNanoM5Frequencies.Length; i++)
        {
            float distance = Mathf.Abs(targetFrequency - indonesiaNanoM5Frequencies[i]);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    // ================= LOGIC PERANGKAT =================

    void UpdateLANStatus()
    {
        lan0Status = (lan0Connector != null && lan0Connector.ConnectedTo != null) ? "Connected" : "Unplugged";
        lan1Status = (lan1Connector != null && lan1Connector.ConnectedTo != null) ? "Connected" : "Unplugged";
    }

    void UpdateComputerAccessStatus()
    {
        bool lanConnected =
            (lan0Connector != null && lan0Connector.ConnectedTo != null) ||
            (lan1Connector != null && lan1Connector.ConnectedTo != null);

        physicalLinkActive = lanConnected && hasPower;

        ipConfigValid =
            IsValidIP(ipAddress) &&
            IsValidIP(netmask) &&
            ipAddress.Trim() != "0.0.0.0" &&
            netmask.Trim() != "0.0.0.0";
    }

    public void SetMode(int modeIndex)
    {
        NetworksRegistry.UnregisterAP(this);
        Disconnect();

        mode = (WirelessMode)modeIndex;

        if (mode == WirelessMode.AccessPoint)
        {
            SyncFrequencyAndChannel();
            NetworksRegistry.RegisterAP(this);
        }
    }

    public void Connect(WirelessDevice ap, string inputPassword)
    {
        if (!hasPower || ap == null || !ap.hasPower) return;
        if (mode != WirelessMode.Station || ap.mode != WirelessMode.AccessPoint) return;

        // Station tidak perlu set frequency manual.
        // Station otomatis ikut frequency dan channel AP.
        ap.SyncFrequencyAndChannel();

        frequency = ap.frequency;
        channel = ap.channel;

        if (!string.IsNullOrEmpty(lockToAPSSID) && ap.ssid != lockToAPSSID)
        {
            Debug.LogWarning($"❌ Gagal menyambung: SSID tidak cocok. LockToAP={lockToAPSSID}, AP={ap.ssid}");
            return;
        }

        if (ap.securityMode != SecurityMode.None && ap.password != inputPassword)
        {
            Debug.LogWarning("❌ Gagal menyambung: Keamanan WPA salah kunci rahasia.");
            return;
        }

        if (connectedAP != null)
        {
            connectedAP.connectedClients.Remove(this);
        }

        connectedAP = ap;

        if (!ap.connectedClients.Contains(this))
        {
            ap.connectedClients.Add(this);
        }

        isConnected = true;

        Debug.Log(
            $"🟢 {deviceName} CONNECTED ke {ap.deviceName} | " +
            $"Station ikut AP: {frequency:0} MHz | CH {channel}"
        );
    }

    public void Disconnect()
    {
        if (connectedAP != null)
        {
            connectedAP.connectedClients.Remove(this);
        }

        connectedAP = null;
        isConnected = false;
        SetStationInternetBridge(false);
    }

    private void ResetRadioStats()
    {
        currentSignal = -120f;
        horizontalSignal = -120f;
        verticalSignal = -120f;
        txRate = 0;
        rxRate = 0;
        transmitCCQ = 0;
        airMaxQuality = 0;
        airMaxCapacity = 0;
        latency = 0;
        packetLoss = 100;
    }

    void SetStationInternetBridge(bool state)
    {
        if (lastInternetState != state)
        {
            lastInternetState = state;

            if (state && connectedAP != null)
            {
                Debug.Log($"🟢 {deviceName} TERHUBUNG INTERNET -> Jembatan Nirkabel WDS Aktif via {connectedAP.deviceName}");
            }
            else
            {
                Debug.Log($"🔴 {deviceName} INTERNET TERPUTUS -> Jembatan Nirkabel Mati");
            }

            if (InternetManager.Instance != null)
            {
                InternetManager.Instance.RefreshNetworkState();
            }
        }
    }

    // ================= KALKULASI RADIO =================

    public float GetEIRP()
    {
        float eirp = outputPower + antennaGain;

        if (calculateEIRP && eirp > MaxEIRP_Indonesia)
        {
            eirp = MaxEIRP_Indonesia;
        }

        return eirp;
    }

    public float CalculateSignal(WirelessDevice ap)
    {
        float distance = Vector3.Distance(transform.position, ap.transform.position);

        if (distance < 1f)
        {
            distance = 1f;
        }

        float usedFrequency = ap != null ? ap.frequency : frequency;

        float fspl =
            (20f * Mathf.Log10(distance)) +
            (20f * Mathf.Log10(usedFrequency)) -
            27.55f;

        float txLoss = ap.CalculateAngleLoss(transform);
        float rxLoss = CalculateAngleLoss(ap.transform);
        float interference = GetChannelInterference(ap);
        float obstacleLoss = GetObstacleLoss(ap);

        float receivedPower =
            ap.GetEIRP() +
            antennaGain -
            fspl -
            txLoss -
            rxLoss -
            interference -
            obstacleLoss;

        return Mathf.Clamp(receivedPower, -120f, -30f);
    }

    float CalculateAngleLoss(Transform target)
    {
        Vector3 dir = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dir);

        if (angle <= beamWidth / 2f)
        {
            float ratio = angle / (beamWidth / 2f);
            return ratio * ratio * 12f;
        }

        return 30f;
    }

    float GetObstacleLoss(WirelessDevice ap)
    {
        Vector3 direction = ap.transform.position - transform.position;
        float distance = direction.magnitude;

        Ray ray = new Ray(transform.position, direction.normalized);
        RaycastHit[] hits = Physics.RaycastAll(ray, distance, obstacleLayer);

        float totalLoss = 0f;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject == gameObject || hit.collider.gameObject == ap.gameObject)
            {
                continue;
            }

            ObstacleMaterial obstacle = hit.collider.GetComponent<ObstacleMaterial>();

            if (obstacle != null)
            {
                float loss = obstacle.GetLoss();
                totalLoss += loss;
                Debug.Log($"🧱 Hambatan: {hit.collider.name} | Redaman: {loss} dB");
            }
        }

        return totalLoss;
    }

    float GetChannelInterference(WirelessDevice ap)
    {
        if (ap == null)
        {
            return 0f;
        }

        int channelDiff = Mathf.Abs(channel - ap.channel);

        if (channelDiff == 0) return 0f;
        if (channelDiff <= 4) return 6f;
        if (channelDiff <= 8) return 3f;

        return 0f;
    }

    // ================= AKSES DARI COMPUTER HOME UI =================

    public static WirelessDevice FindNanoByIP(string targetIP)
    {
        if (string.IsNullOrWhiteSpace(targetIP))
        {
            return null;
        }

        targetIP = targetIP.Trim();

        foreach (WirelessDevice device in allWirelessDevices)
        {
            if (device == null) continue;
            if (string.IsNullOrWhiteSpace(device.ipAddress)) continue;

            if (device.ipAddress.Trim() == targetIP)
            {
                return device;
            }
        }

        return null;
    }

    // ================= FUNGSI UTILITAS =================

    public string GetFrequencyBand()
    {
        return "5 GHz";
    }

    public float CalculateDistance()
    {
        if (connectedAP == null)
        {
            return 0f;
        }

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

    public void SetDeviceName(string newName)
    {
        deviceName = newName;
        gameObject.name = newName;
    }

    private string IncrementMac(string mac, int increment)
    {
        string[] parts = mac.Split(':');

        if (parts.Length != 6)
        {
            return mac;
        }

        int lastByte = System.Convert.ToInt32(parts[5], 16);
        lastByte = (lastByte + increment) % 256;
        parts[5] = lastByte.ToString("X2");

        return string.Join(":", parts);
    }

    private string GenerateMacAddress()
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

    private bool IsSameSubnet(string ip1, string ip2, string netmask)
    {
        string[] ipA = ip1.Split('.');
        string[] ipB = ip2.Split('.');
        string[] mask = netmask.Split('.');

        if (ipA.Length != 4 || ipB.Length != 4 || mask.Length != 4)
        {
            return false;
        }

        for (int i = 0; i < 4; i++)
        {
            if (!int.TryParse(ipA[i], out int aPart)) return false;
            if (!int.TryParse(ipB[i], out int bPart)) return false;
            if (!int.TryParse(mask[i], out int maskPart)) return false;

            int a = aPart & maskPart;
            int b = bPart & maskPart;

            if (a != b)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsValidIP(string ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
        {
            return false;
        }

        string[] parts = ip.Trim().Split('.');

        if (parts.Length != 4)
        {
            return false;
        }

        for (int i = 0; i < 4; i++)
        {
            if (!int.TryParse(parts[i], out int value))
            {
                return false;
            }

            if (value < 0 || value > 255)
            {
                return false;
            }
        }

        return true;
    }
}