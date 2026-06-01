using UnityEngine;
using HPhysic;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkDevice))]
public class ComputerDevice : MonoBehaviour
{
    [HideInInspector] public NetworkDevice networkDevice;

    // ================= STATUS =================
    [Header("Status")]
    public bool isConnected;
    public bool physicalLinkActive;

    public bool ipConfigValid;
    public bool networkValid;
    public bool gatewayReachable;
    public bool dnsValid;
    public bool hasInternet;
    public bool ipConflict;

    [TextArea]
    public string networkMessage;

    // ================= IP CONFIG =================
    [Header("IPv4 Configuration")]
    public string ipAddress = "0.0.0.0";
    public string gateway = "0.0.0.0";
    public string netmask = "255.255.255.0";
    public string dns = "8.8.8.8";

    // ================= LAN =================
    [Header("LAN Port")]
    public Connector lanConnector;
    public string lanStatus = "Unplugged";

    private static readonly List<ComputerDevice> allComputers = new List<ComputerDevice>();

    private void Awake()
    {
        networkDevice = GetComponent<NetworkDevice>();

        if (!allComputers.Contains(this))
        {
            allComputers.Add(this);
        }
    }

    private void OnDestroy()
    {
        if (allComputers.Contains(this))
        {
            allComputers.Remove(this);
        }
    }

    private void Update()
    {
        UpdateLANStatus();
        UpdatePhysicalLink();
        ValidateNetwork();
        UpdateInternetState();
    }

    // ================= LAN STATUS =================

    private void UpdateLANStatus()
    {
        if (lanConnector != null && lanConnector.ConnectedTo != null)
        {
            lanStatus = "Connected";
        }
        else
        {
            lanStatus = "Unplugged";
        }
    }

    private void UpdatePhysicalLink()
    {
        physicalLinkActive = lanStatus == "Connected";
        isConnected = physicalLinkActive;
    }

    // ================= VALIDASI NETWORK =================

    private void ValidateNetwork()
    {
        ipConfigValid = false;
        networkValid = false;
        gatewayReachable = false;
        dnsValid = false;
        ipConflict = false;
        networkMessage = "";

        if (!physicalLinkActive)
        {
            networkMessage = "Cable unplugged.";
            return;
        }

        if (!TryParseIP(ipAddress, out uint ip))
        {
            networkMessage = "Invalid IP address.";
            return;
        }

        if (!TryParseIP(netmask, out uint mask))
        {
            networkMessage = "Invalid subnet mask.";
            return;
        }

        if (ip == 0)
        {
            networkMessage = "IP address cannot be 0.0.0.0.";
            return;
        }

        if (!IsValidSubnetMask(mask))
        {
            networkMessage = "Invalid subnet mask.";
            return;
        }

        if (!IsUsableHostAddress(ip, mask))
        {
            networkMessage = "IP address is network or broadcast address.";
            return;
        }

        if (HasIPConflict(ip))
        {
            ipConflict = true;
            networkMessage = "IP conflict detected.";
            return;
        }

        // IP lokal valid.
        // Ini cukup untuk akses Nano satu subnet.
        ipConfigValid = true;
        networkValid = true;
        networkMessage = "Local network configuration valid.";

        // ================= GATEWAY =================
        if (!string.IsNullOrWhiteSpace(gateway) && gateway.Trim() != "0.0.0.0")
        {
            if (!TryParseIP(gateway, out uint gw))
            {
                networkValid = false;
                networkMessage = "Invalid gateway address.";
                return;
            }

            if (!IsUsableHostAddress(gw, mask))
            {
                networkValid = false;
                networkMessage = "Gateway is network or broadcast address.";
                return;
            }

            if (gw == ip)
            {
                networkValid = false;
                networkMessage = "Gateway cannot be same as computer IP.";
                return;
            }

            if (!IsSameSubnet(ip, gw, mask))
            {
                networkValid = false;
                networkMessage = "Gateway is not in the same subnet.";
                return;
            }

            gatewayReachable = IsGatewayReachable(gw);
        }

        // ================= DNS =================
        if (!string.IsNullOrWhiteSpace(dns) && dns.Trim() != "0.0.0.0")
        {
            if (TryParseIP(dns, out uint dnsIP))
            {
                if (IsUsableHostAddress(dnsIP, mask))
                {
                    dnsValid = true;
                }
            }
        }

        if (gatewayReachable && dnsValid)
        {
            networkMessage = "Network valid. Gateway and DNS configured.";
        }
        else if (gatewayReachable && !dnsValid)
        {
            networkMessage = "Gateway reachable. DNS not valid or not configured.";
        }
        else
        {
            networkMessage = "Local network valid. Gateway not reachable.";
        }
    }

    // ================= INTERNET =================

    private void UpdateInternetState()
    {
        bool internet =
            physicalLinkActive &&
            ipConfigValid &&
            networkValid &&
            gatewayReachable &&
            dnsValid &&
            networkDevice != null &&
            networkDevice.hasInternet;

        hasInternet = internet;
    }

    // ================= TEST CONNECTION =================

    public string TestConnection()
    {
        if (!physicalLinkActive)
        {
            return "Cable Unplugged ❌";
        }

        if (!ipConfigValid)
        {
            return "IP Configuration Invalid ❌\n" + networkMessage;
        }

        if (ipConflict)
        {
            return "IP Conflict ❌";
        }

        if (hasInternet)
        {
            return "Internet Access ✅";
        }

        if (networkValid && !gatewayReachable)
        {
            return "Local Network Ready ✅\nGateway Not Reachable ❌";
        }

        if (networkValid && gatewayReachable && !dnsValid)
        {
            return "Gateway Reachable ✅\nDNS Invalid ❌";
        }

        return "Connected, No Internet Access ❌\n" + networkMessage;
    }

    // ================= DIPANGGIL COMPUTERHOMEUI =================

    public void SetIP(string ip)
    {
        ipAddress = ip.Trim();
    }

    public void SetGateway(string gw)
    {
        gateway = gw.Trim();
    }

    public void SetNetmask(string mask)
    {
        netmask = mask.Trim();
    }

    public void SetDNS(string newDns)
    {
        dns = newDns.Trim();
    }

    // ================= AKSES LOKAL =================

    public bool CanReachLocalIP(string targetIP, out string message)
    {
        message = "";

        if (!physicalLinkActive)
        {
            message = "Kabel LAN komputer belum tersambung.";
            return false;
        }

        if (!ipConfigValid)
        {
            message = "Konfigurasi IP komputer belum valid.";
            return false;
        }

        if (!TryParseIP(ipAddress, out uint pcIP))
        {
            message = "IP komputer tidak valid.";
            return false;
        }

        if (!TryParseIP(netmask, out uint mask))
        {
            message = "Subnet mask komputer tidak valid.";
            return false;
        }

        if (!TryParseIP(targetIP, out uint target))
        {
            message = "IP tujuan tidak valid.";
            return false;
        }

        if (!IsSameSubnet(pcIP, target, mask))
        {
            message = "Komputer dan perangkat tujuan tidak berada pada subnet yang sama.";
            return false;
        }

        message = "Perangkat berada dalam subnet yang sama.";
        return true;
    }

    // ================= IP HELPER =================

    public bool TryParseIP(string ip, out uint result)
    {
        result = 0;

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

            result = (result << 8) + (uint)value;
        }

        return true;
    }

    public bool IsSameSubnet(uint ip1, uint ip2, uint mask)
    {
        return GetNetworkAddress(ip1, mask) == GetNetworkAddress(ip2, mask);
    }

    public uint GetNetworkAddress(uint ip, uint mask)
    {
        return ip & mask;
    }

    public uint GetBroadcastAddress(uint ip, uint mask)
    {
        return (ip & mask) | ~mask;
    }

    public bool IsUsableHostAddress(uint ip, uint mask)
    {
        uint network = GetNetworkAddress(ip, mask);
        uint broadcast = GetBroadcastAddress(ip, mask);

        return ip != network && ip != broadcast;
    }

    private bool IsValidSubnetMask(uint mask)
    {
        if (mask == 0 || mask == uint.MaxValue)
        {
            return false;
        }

        bool foundZero = false;

        for (int i = 31; i >= 0; i--)
        {
            bool bitIsOne = (mask & (1u << i)) != 0;

            if (!bitIsOne)
            {
                foundZero = true;
            }
            else if (foundZero)
            {
                return false;
            }
        }

        return true;
    }

    // ================= IP CONFLICT =================

    private bool HasIPConflict(uint myIP)
    {
        foreach (ComputerDevice other in allComputers)
        {
            if (other == null) continue;
            if (other == this) continue;
            if (!other.physicalLinkActive) continue;

            if (!other.TryParseIP(other.ipAddress, out uint otherIP))
            {
                continue;
            }

            if (otherIP == myIP)
            {
                return true;
            }
        }

        return false;
    }

    // ================= GATEWAY =================

    private bool IsGatewayReachable(uint gatewayIP)
    {
        foreach (ComputerDevice other in allComputers)
        {
            if (other == null) continue;
            if (other == this) continue;
            if (!other.physicalLinkActive) continue;

            if (!other.TryParseIP(other.ipAddress, out uint otherIP))
            {
                continue;
            }

            if (otherIP == gatewayIP)
            {
                return true;
            }
        }

        if (networkDevice != null && networkDevice.hasInternet)
        {
            return true;
        }

        return false;
    }
}