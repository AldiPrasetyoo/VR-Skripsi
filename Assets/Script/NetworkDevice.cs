using UnityEngine;

public class NetworkDevice : MonoBehaviour
{
    [Header("Internet Status")]
    public bool isInternetSource = false;
    public bool hasInternet = false;

    [Header("Power Status")]
    public bool isPowerSource = false;       // True jika dicolok langsung ke stopkontak (misal: PoE Adaptor/Router utama)
    public bool hasPower = false;            // Apakah saat ini sedang menyala?

    [Header("Device Properties")]
    public bool supportPoE = false;          // True jika bisa menerima aliran listrik dari kabel LAN (misal: Antena Nano, Kabel)
    public bool requirePowerToWork = true;   // True untuk PC/Antena. False untuk Kabel (karena kabel tidak butuh listrik untuk sekadar dilewati data)

    public void SetInternet(bool state)
    {
        hasInternet = state;
        Debug.Log($"{name} Internet = {state}");
    }

    public void SetPower(bool state)
    {
        hasPower = state;
        Debug.Log($"{name} Power = {state}");
    }
}