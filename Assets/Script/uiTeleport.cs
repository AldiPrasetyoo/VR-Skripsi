using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleportManager : MonoBehaviour
{
    [Header("Komponen Utama")]
    public TeleportationProvider teleportProvider;

    [Header("Daftar Titik Tujuan")]
    public Transform titikAtap;
    public Transform titikRuangRakit;
    public Transform titikHalaman;

    // Fungsi untuk dipanggil oleh Tombol 1
    public void KeAtap()
    {
        LakukanTeleport(titikAtap);
    }

    // Fungsi untuk dipanggil oleh Tombol 2
    public void KeRuangRakit()
    {
        LakukanTeleport(titikRuangRakit);
    }

    // Fungsi untuk dipanggil oleh Tombol 3
    public void KeHalaman()
    {
        LakukanTeleport(titikHalaman);
    }

    // Fungsi inti yang memproses perpindahan (tidak perlu dipanggil manual oleh tombol)
    private void LakukanTeleport(Transform tujuan)
    {
        if (teleportProvider != null && tujuan != null)
        {
            TeleportRequest request = new TeleportRequest()
            {
                destinationPosition = tujuan.position,
                destinationRotation = tujuan.rotation
            };
            teleportProvider.QueueTeleportRequest(request);
        }
        else
        {
            Debug.LogWarning("Ada titik tujuan atau Teleport Provider yang belum dimasukkan!");
        }
    }
}