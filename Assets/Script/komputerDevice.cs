using UnityEngine;
using HPhysic;

[RequireComponent(typeof(NetworkDevice))]
public class ComputerDevice : MonoBehaviour
{
    // ================= GENERAL =================
    public string deviceName;

    [HideInInspector] public NetworkDevice networkDevice;

    public bool isConnected;   // kabel ke switch
    public bool hasInternet;

    // ================= NETWORK CONFIG (STATIC) =================
    [Header("STATIC IP CONFIG")]
    public string ipAddress = "0.0.0.0";
    public string gateway = "0.0.0.0";
    public string netmask = "255.255.255.0";

    // ================= STATUS =================
    public bool networkValid;

    // ================= LAN =================
    [Header("PORT")]
    public Connector lanConnector;

    public string lanStatus = "Unplugged";

    // ================= UNITY =================
    void Awake()
    {
        networkDevice = GetComponent<NetworkDevice>();
        deviceName = gameObject.name;
    }

    void Update()
    {
        UpdateLANStatus();
        HandleConnection();
        ValidateNetwork();
        UpdateInternetState();
    }

    // ================= 🔌 LAN STATUS =================
    void UpdateLANStatus()
    {
        lanStatus = (lanConnector != null && lanConnector.ConnectedTo != null)
            ? "Connected"
            : "Unplugged";
    }

    // ================= 🔗 CONNECTION =================
    void HandleConnection()
    {
        isConnected = lanStatus == "Connected";
    }

    // ================= 🧠 VALIDASI NETWORK =================
    void ValidateNetwork()
    {
        if (!isConnected)
        {
            networkValid = false;
            return;
        }

        // IP kosong
        if (ipAddress == "0.0.0.0" || gateway == "0.0.0.0")
        {
            networkValid = false;
            return;
        }

        // subnet tidak sama
        if (!IsSameSubnet(ipAddress, gateway, netmask))
        {
            networkValid = false;
            return;
        }

        networkValid = true;
    }

    // ================= 🌍 INTERNET =================
    void UpdateInternetState()
    {
        bool internet = networkValid && networkDevice.hasInternet;

        if (hasInternet != internet)
        {
            hasInternet = internet;

            if (hasInternet)
                Debug.Log($"🟢 {deviceName} CONNECTED TO INTERNET");
            else
                Debug.Log($"🔴 {deviceName} NO INTERNET");
        }
    }

    // ================= 🔍 SUBNET =================
    bool IsSameSubnet(string ip1, string ip2, string netmask)
    {
        string[] a = ip1.Split('.');
        string[] b = ip2.Split('.');
        string[] m = netmask.Split('.');

        if (a.Length != 4 || b.Length != 4 || m.Length != 4) return false;

        for (int i = 0; i < 4; i++)
        {
            int ipA = int.Parse(a[i]) & int.Parse(m[i]);
            int ipB = int.Parse(b[i]) & int.Parse(m[i]);

            if (ipA != ipB)
                return false;
        }

        return true;
    }

    // ================= 🧪 TEST BUTTON =================
    public string TestConnection()
    {
        if (!isConnected)
            return "Cable Unplugged ❌";

        if (!networkValid)
            return "Network Invalid ❌";

        if (hasInternet)
            return "Ping Success ✅";

        return "Request Time Out ❌";
    }

    // ================= ⚙️ CONFIG DARI UI =================
    public void SetIP(string ip)
    {
        ipAddress = ip;
    }

    public void SetGateway(string gw)
    {
        gateway = gw;
    }

    public void SetNetmask(string mask)
    {
        netmask = mask;
    }

    public void SetDeviceName(string newName)
    {
    gameObject.name = newName;
    }
}