using UnityEngine;
using System.Collections.Generic;

public class AntennaDevice : MonoBehaviour
{
    public enum DeviceMode { AP, Station }
    public enum SecurityMode { Open, WPA2 }

    [Header("Device Mode")]
    public DeviceMode mode;

    [Header("Wireless - AP")]
    public string ssid ;
    public SecurityMode security;
    public string password ;
    public int channel = 1;
    public float outputPower = 20f;

    [Header("Wireless - Station")]
    public string selectedSSID;
    public string enteredPassword;

    [Header("Network")]
    public bool useDHCP = true;
    public string ipAddress = "192.168.1.2";
    public string netmask = "255.255.255.0";
    public string gateway = "192.168.1.1";
    public string dns = "8.8.8.8";

    [Header("Signal")]
    public float maxDistance = 100f;
    public bool isConnected;
    public float rssi;
    public float ccq;
    public float txRate;
    public float rxRate;

    void Update()
    {
        if (mode == DeviceMode.Station)
            TryConnect();
    }

    void TryConnect()
    {
        AntennaDevice[] devices =
            FindObjectsOfType<AntennaDevice>();

        isConnected = false;

        foreach (var ap in devices)
        {
            if (ap.mode != DeviceMode.AP) continue;
            if (ap.ssid != selectedSSID) continue;

            if (ap.security == SecurityMode.WPA2 &&
                ap.password != enteredPassword)
                continue;

            float distance =
                Vector3.Distance(transform.position, ap.transform.position);

            if (distance > maxDistance) continue;

            isConnected = true;

            rssi = Mathf.Lerp(-30, -90,
                distance / maxDistance);

            ccq = Mathf.Clamp(100 + rssi, 0, 100);

            txRate = Mathf.Lerp(300, 10,
                distance / maxDistance);

            rxRate = txRate;

            if (useDHCP)
            {
                ipAddress =
                    GenerateDHCPIP(ap.ipAddress);
                gateway = ap.ipAddress;
            }

            break;
        }
    }

    string GenerateDHCPIP(string apIP)
    {
        string[] parts = apIP.Split('.');
        if (parts.Length < 3) return "0.0.0.0";

        return parts[0] + "." +
               parts[1] + "." +
               parts[2] + "." +
               Random.Range(2, 200);
    }
}