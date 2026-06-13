using UnityEngine;
using System.Collections;

public class DisturbTrigger : MonoBehaviour
{
    [Header("Efek Gangguan")]
    public GameObject disturbanceEffect;

    [Header("Mobil Utama")]
    public PodMovement podMovement;

    [Header("Traffic Spawner")]
    public TrafficPodSpawner trafficSpawner;

    [Header("Hapus Traffic Acak")]
    public int jumlahTrafficDihapus = 2;
    public float delayHapusTraffic = 0.2f;

    [Header("Object Muncul Sementara Saat Guncangan")]
    public GameObject[] objectMunculSementara;
    public float durasiMuncul = 3f;

    private bool triggered = false;

    private void Start()
    {
        ResetDisturb();
    }

    public void ResetDisturb()
    {
        triggered = false;


        for (int i = 0; i < objectMunculSementara.Length; i++)
        {
            if (objectMunculSementara[i] != null)
                objectMunculSementara[i].SetActive(false);
        }

        Debug.Log("DisturbTrigger di-reset");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("ZoneDisturb kena oleh: " + other.name + " | Root: " + other.transform.root.name);

        if (triggered) return;

        // Abaikan XR Origin / Player
        if (other.transform.root.CompareTag("Player"))
        {
            Debug.Log("Yang masuk XR Origin, diabaikan");
            return;
        }

        PodMovement podYangKena = other.GetComponentInParent<PodMovement>();

        if (podYangKena == null)
        {
            Debug.Log("Yang masuk bukan mobil utama");
            return;
        }

        // Penting: hanya mobil utama yang boleh trigger gangguan
        if (podYangKena != podMovement)
        {
            Debug.Log("Yang masuk bukan PodMovement mobil utama, diabaikan");
            return;
        }

        triggered = true;

        if (disturbanceEffect != null)
            disturbanceEffect.SetActive(true);

        podMovement.StartShake();

        StartCoroutine(TampilkanObjectSementara());
        StartCoroutine(HapusTrafficSetelahDelay());

        Debug.Log("Gangguan aktif, mobil utama berguncang, traffic akan dihapus acak");
    }

    private IEnumerator TampilkanObjectSementara()
    {
        for (int i = 0; i < objectMunculSementara.Length; i++)
        {
            if (objectMunculSementara[i] != null)
                objectMunculSementara[i].SetActive(true);
        }

        yield return new WaitForSeconds(durasiMuncul);

        for (int i = 0; i < objectMunculSementara.Length; i++)
        {
            if (objectMunculSementara[i] != null)
                objectMunculSementara[i].SetActive(false);
        }
    }

    private IEnumerator HapusTrafficSetelahDelay()
    {
        yield return new WaitForSeconds(delayHapusTraffic);

        if (trafficSpawner != null)
        {
            trafficSpawner.HilangkanTrafficAcak(jumlahTrafficDihapus);
            Debug.Log("Traffic dihapus acak sebanyak: " + jumlahTrafficDihapus);
        }
        else
        {
            Debug.LogWarning("Traffic Spawner belum dimasukkan!");
        }
    }
}