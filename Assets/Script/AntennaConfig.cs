using UnityEngine;

public class AntennaConfig : MonoBehaviour
{
    public string ipAddress;
    public string remoteIP;
    public string mode; // "AP" atau "Station"
    

    public float maxDistance = 50f;

    public bool isConnected;
    public float rssi;

    void Update()
    {
        AutoConnect();
    }

    void AutoConnect()
    {
        AntennaConfig[] antennas =
            FindObjectsOfType<AntennaConfig>();

        isConnected = false;
        rssi = -100f;

        foreach (AntennaConfig other in antennas)
        {
            if (other == this) continue;

            if (other.ipAddress == remoteIP &&
                other.remoteIP == ipAddress)
            {
                if ((mode == "AP" && other.mode == "Station") ||
                    (mode == "Station" && other.mode == "AP"))
                {
                    if (SameNetwork(ipAddress, other.ipAddress))
                    {
                        float distance =
                            Vector3.Distance(
                                transform.position,
                                other.transform.position);

                        if (distance <= maxDistance)
                        {
                            isConnected = true;

                            // RSSI calculation
                            rssi = Mathf.Lerp(
                                -30f,   // sangat dekat
                                -90f,   // sangat jauh
                                distance / maxDistance
                            );

                            other.isConnected = true;
                            other.rssi = rssi;
                        }
                    }
                }
            }
        }
    }

    bool SameNetwork(string ip1, string ip2)
    {
        string[] a = ip1.Split('.');
        string[] b = ip2.Split('.');

        if (a.Length < 3 || b.Length < 3)
            return false;

        return a[0] == b[0] &&
               a[1] == b[1] &&
               a[2] == b[2];
    }
}
