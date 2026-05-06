using UnityEngine;
using HPhysic;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkDevice))]
public class SwitchDevice : MonoBehaviour
{
    [Header("Ports")]
    public List<Connector> ports = new List<Connector>();

    [HideInInspector] public NetworkDevice networkDevice;

    public int activePorts;

    void Awake()
    {
        networkDevice = GetComponent<NetworkDevice>();
        ports = new List<Connector>(GetComponentsInChildren<Connector>());
    }

void Update()
{
    UpdatePorts();
    ReceiveInternet();
    DistributeInternet();
}

    // ================= PORT STATUS =================
    void UpdatePorts()
    {
        activePorts = 0;

        foreach (var port in ports)
        {
            if (port != null && port.ConnectedTo != null)
                activePorts++;
        }
    }

    // ================= INTERNET FLOW =================
    void DistributeInternet()
    {
        if (!networkDevice.hasInternet) return;

        foreach (var port in ports)
        {
            if (port == null || port.ConnectedTo == null) continue;

            // ambil device yang terhubung
            var target = port.ConnectedTo.GetComponentInParent<NetworkDevice>();

            if (target != null)
            {
                target.hasInternet = true;
            }
        }
    }

    void ReceiveInternet()
{
    networkDevice.hasInternet = false;

    foreach (var port in ports)
    {
        if (port == null || port.ConnectedTo == null) continue;

        var source = port.ConnectedTo.GetComponentInParent<NetworkDevice>();

        if (source != null && source.hasInternet)
        {
            networkDevice.hasInternet = true;
            return; // cukup satu sumber
        }
    }
}
}