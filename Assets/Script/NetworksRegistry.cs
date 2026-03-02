using System.Collections.Generic;

public static class NetworksRegistry
{
    public static List<WirelessDevice> activeAPs = new List<WirelessDevice>();

    public static void RegisterAP(WirelessDevice ap)
    {
        if (!activeAPs.Contains(ap))
            activeAPs.Add(ap);
    }

    public static void UnregisterAP(WirelessDevice ap)
    {
        if (activeAPs.Contains(ap))
            activeAPs.Remove(ap);
    }
}