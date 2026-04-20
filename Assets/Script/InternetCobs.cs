using UnityEngine;

public class InternetCobs : MonoBehaviour
{
    public HPhysic.Connector end1;
    public HPhysic.Connector end2;

    private bool internetActive = false;

    private void Update()
{
    if (end1 == null || end2 == null) return;

    // Debug.Log("End1: " + end1.IsConnected + 
    //           " | End2: " + end2.IsConnected);

    bool isActive = end1.IsConnected && end2.IsConnected;

    if (isActive != internetActive)
    {
        internetActive = isActive;

        // if (internetActive)
        //     Debug.Log("🌐 INTERNET AKTIF");
        // else
        //     Debug.Log("❌ INTERNET TERPUTUS");
    }
}
}