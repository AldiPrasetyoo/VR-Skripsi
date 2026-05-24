using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class TeleportManager : MonoBehaviour
{
    [Header("Komponen Utama")]
    public TeleportationProvider teleportProvider;

    [Header("Daftar Titik Tujuan")]
    public Transform titikAtap;
    public Transform titikRuangRakit;
    public Transform titikHalaman;

    [Header("Pintu Lift Sesuai Titik Teleport")]
    public DoorSlideTrigger pintuLiftAtap;
    public DoorSlideTrigger pintuLiftRuangRakit;
    public DoorSlideTrigger pintuLiftHalaman;

    [Header("Delay Buka Pintu Setelah Teleport")]
    public float delayBukaPintu = 0.3f;

    public void KeAtap()
    {
        LakukanTeleport(titikAtap, pintuLiftAtap);
    }

    public void KeRuangRakit()
    {
        LakukanTeleport(titikRuangRakit, pintuLiftRuangRakit);
    }

    public void KeHalaman()
    {
        LakukanTeleport(titikHalaman, pintuLiftHalaman);
    }

    private void LakukanTeleport(Transform tujuan, DoorSlideTrigger pintuYangDibuka)
    {
        if (teleportProvider == null)
        {
            Debug.LogWarning("Teleport Provider belum dimasukkan!");
            return;
        }

        if (tujuan == null)
        {
            Debug.LogWarning("Titik tujuan teleport belum dimasukkan!");
            return;
        }

        TeleportRequest request = new TeleportRequest()
        {
            destinationPosition = tujuan.position,
            destinationRotation = tujuan.rotation
        };

        teleportProvider.QueueTeleportRequest(request);

        StartCoroutine(BukaPintuSetelahTeleport(pintuYangDibuka));
    }

    private IEnumerator BukaPintuSetelahTeleport(DoorSlideTrigger pintuYangDibuka)
    {
        yield return new WaitForSeconds(delayBukaPintu);

        if (pintuYangDibuka != null)
        {
            Debug.Log("Teleport selesai. Membuka pintu lift: " + pintuYangDibuka.name);
            pintuYangDibuka.OpenDoor();
        }
        else
        {
            Debug.LogWarning("Pintu lift untuk titik teleport ini belum dimasukkan!");
        }
    }
}