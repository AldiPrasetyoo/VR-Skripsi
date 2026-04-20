using UnityEngine;

public class AccessPoint : MonoBehaviour
{
    [Header("Runtime Config")]
    public string ssid;
    public string password;
    public float txPower;       // dBm
    public float maxDistance;   // meter
    public int channel;         // 36,40,44,dst
    public float frequency = 5f; // 5 GHz only

    private void OnEnable()
    {
        NetworkRegistry.activeAPs.Add(this);
    }

    private void OnDisable()
    {
        NetworkRegistry.activeAPs.Remove(this);
    }
}