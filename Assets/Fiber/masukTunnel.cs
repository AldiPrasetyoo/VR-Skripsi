using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class TunnelPortal : MonoBehaviour
{
    [Header("Teleport Provider")]
    public TeleportationProvider teleportProvider;

    [Header("XR Origin")]
    public GameObject xrOrigin;

    [Header("Titik Duduk di Mobil")]
    public Transform seatPoint;

    [Header("Mobil / Pod Movement")]
    public PodMovement podMovement;

    [Header("Traffic Spawner")]
    public TrafficPodSpawner trafficPodSpawner;

    [Header("Object Yang Dimatikan Setelah Teleport")]
    public GameObject[] objectYangDimatikan;

    [Header("Komponen Movement Yang Dimatikan")]
    public Behaviour[] komponenYangDimatikan;

    [Header("Delay Setelah Teleport")]
    public float delaySetelahTeleport = 0.3f;

    [Header("Portal Bisa Dipakai Ulang")]
    public bool bisaDipakaiUlang = true;
    public float cooldownPortal = 1f;

    [Header("Disturb Trigger")]
    public DisturbTrigger disturbTrigger;

    private bool sedangProses = false;

    private void OnTriggerEnter(Collider other)
    {
        if (sedangProses) return;

        Debug.Log("Trigger kena oleh: " + other.name);

        if (other.transform.root.CompareTag("Player"))
        {
            sedangProses = true;

            Debug.Log("Player terdeteksi, teleport ke mobil");

            LakukanTeleportKeMobil();

            if (bisaDipakaiUlang)
                StartCoroutine(ResetPortalCooldown());
        }
        else
        {
            Debug.Log("Bukan Player. Root = " + other.transform.root.name);
        }
    }

    private IEnumerator ResetPortalCooldown()
    {
        yield return new WaitForSeconds(cooldownPortal);
        sedangProses = false;
    }

    private void LakukanTeleportKeMobil()
    {
        if (teleportProvider == null)
        {
            Debug.LogWarning("Teleport Provider belum dimasukkan!");
            return;
        }

        if (xrOrigin == null)
        {
            Debug.LogWarning("XR Origin belum dimasukkan!");
            return;
        }

        if (seatPoint == null)
        {
            Debug.LogWarning("Seat Point belum dimasukkan!");
            return;
        }

        // Reset mobil ke awal sebelum player dipindahkan ke kursi
        if (podMovement != null)
        {
            podMovement.ResetPod();
        }

        // Reset traffic kalau ada
        if (trafficPodSpawner != null)
        {
            trafficPodSpawner.ResetSpawner();
        }

        if (disturbTrigger != null)
        {
            disturbTrigger.ResetDisturb();
        }

        if (!teleportProvider.enabled)
            teleportProvider.enabled = true;

        TeleportRequest request = new TeleportRequest()
        {
            destinationPosition = seatPoint.position,
            destinationRotation = seatPoint.rotation
        };

        teleportProvider.QueueTeleportRequest(request);

        StartCoroutine(MulaiMobilSetelahTeleport());
    }

    private IEnumerator MulaiMobilSetelahTeleport()
    {
        yield return new WaitForSeconds(delaySetelahTeleport);

        MatikanObjectSetelahTeleport();
        MatikanKomponenMovement();

        if (podMovement != null)
        {
            podMovement.SetPassenger(xrOrigin);
            podMovement.StartMove();

            if (trafficPodSpawner != null)
                trafficPodSpawner.StartSpawn();

            Debug.Log("Teleport selesai, XR Origin ikut mobil, mobil mulai jalan, traffic spawn");
        }
        else
        {
            Debug.LogWarning("Pod Movement belum dimasukkan!");
        }
    }

    private void MatikanObjectSetelahTeleport()
    {
        for (int i = 0; i < objectYangDimatikan.Length; i++)
        {
            if (objectYangDimatikan[i] != null)
                objectYangDimatikan[i].SetActive(false);
        }
    }

    private void MatikanKomponenMovement()
    {
        for (int i = 0; i < komponenYangDimatikan.Length; i++)
        {
            if (komponenYangDimatikan[i] != null)
                komponenYangDimatikan[i].enabled = false;
        }
    }
}