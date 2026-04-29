using UnityEngine;
using System.Collections.Generic;

public class InternetManager : MonoBehaviour
{
    public static InternetManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void RefreshNetworkState()
    {
        NetworkDevice[] devices = FindObjectsOfType<NetworkDevice>();

        // 1. Reset semua status internet dan set power ke default
        foreach (var d in devices)
        {
            d.SetInternet(false);
            // Jika dia dicolok ke tembok (isPowerSource), dia otomatis punya power
            d.SetPower(d.isPowerSource); 
        }

        // 2. Sebarkan Power (PoE) dari sumber listrik
        foreach (var d in devices)
        {
            if (d.isPowerSource)
                SpreadPower(d);
        }

        // 3. Setelah listrik menyebar, sebarkan Internet
        foreach (var d in devices)
        {
            if (d.isInternetSource)
                SpreadInternet(d);
        }
    }

    void SpreadPower(NetworkDevice start)
    {
        Queue<NetworkDevice> queue = new();
        HashSet<NetworkDevice> visited = new();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            current.SetPower(true);

            NetworkPort[] ports = current.GetComponentsInChildren<NetworkPort>();

            foreach (var p in ports)
            {
                if (p.ConnectedPort != null)
                {
                    var next = p.ConnectedPort.ParentDevice;

                    // LISTRIK HANYA BISA MENYEBERANG JIKA DEVICE TARGET MENDUKUNG PoE
                    if (next != null && !visited.Contains(next) && next.supportPoE)
                    {
                        visited.Add(next);
                        queue.Enqueue(next);
                    }
                }
            }
        }
    }

    void SpreadInternet(NetworkDevice start)
    {
        Queue<NetworkDevice> queue = new();
        HashSet<NetworkDevice> visited = new();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            // JIKA DEVICE INI BUTUH LISTRIK TAPI SEDANG MATI, HENTIKAN INTERNET DI SINI
            if (current.requirePowerToWork && !current.hasPower)
            {
                continue; 
            }

            current.SetInternet(true);

            NetworkPort[] ports = current.GetComponentsInChildren<NetworkPort>();

            foreach (var p in ports)
            {
                if (p.ConnectedPort != null)
                {
                    var next = p.ConnectedPort.ParentDevice;

                    if (next != null && !visited.Contains(next))
                    {
                        visited.Add(next);
                        queue.Enqueue(next);
                    }
                }
            }
        }
    }
}