using UnityEngine;
using System.Collections.Generic;

public class TrafficPodSpawner : MonoBehaviour
{
    [Header("Prefab Mobil Traffic")]
    public GameObject trafficPodPrefab;

    [Header("Mobil Utama Sebagai Pusat Spawn")]
    public Transform ridePod;

    [Header("Jumlah Traffic Sekali Spawn")]
    public int jumlahTraffic = 8;

    [Header("Jarak Sebaran")]
    public float jarakDepan = 12f;
    public float jarakBelakang = 8f;
    public float jarakSamping = 3f;

    [Header("Random Tambahan")]
    public float randomZ = 3f;
    public float randomX = 0.5f;

    [Header("Kecepatan Traffic")]
    public float minSpeed = 2f;
    public float maxSpeed = 5f;

    [Header("Waktu Hidup Traffic")]
    public float trafficLifeTime = 15f;

    private List<GameObject> spawnedTraffic = new List<GameObject>();
    private bool sudahSpawn = false;

    public void StartSpawn()
    {
        if (sudahSpawn) return;

        sudahSpawn = true;

        SpawnSemuaTraffic();

        Debug.Log("Semua traffic langsung spawn karena player sudah di mobil");
    }

    private void SpawnSemuaTraffic()
    {
        if (trafficPodPrefab == null)
        {
            Debug.LogWarning("Traffic Pod Prefab belum dimasukkan!");
            return;
        }

        if (ridePod == null)
        {
            Debug.LogWarning("Ride Pod belum dimasukkan!");
            return;
        }

        for (int i = 0; i < jumlahTraffic; i++)
        {
            SpawnTraffic(i);
        }
    }

    public void ResetSpawner()
    {
        HapusSemuaTraffic();
        sudahSpawn = false;

        Debug.Log("Traffic spawner di-reset");
    }

    private void SpawnTraffic(int index)
    {
        int posisi = index % 4;

        Vector3 spawnPosition = ridePod.position;
        Vector3 moveDirection = ridePod.forward;

        if (posisi == 0)
        {
            // Depan
            spawnPosition += ridePod.forward * jarakDepan;
            spawnPosition += ridePod.right * Random.Range(-randomX, randomX);
            moveDirection = ridePod.forward;
        }
        else if (posisi == 1)
        {
            // Belakang
            spawnPosition -= ridePod.forward * jarakBelakang;
            spawnPosition += ridePod.right * Random.Range(-randomX, randomX);
            moveDirection = ridePod.forward;
        }
        else if (posisi == 2)
        {
            // Kanan
            spawnPosition += ridePod.right * jarakSamping;
            spawnPosition += ridePod.forward * Random.Range(-randomZ, randomZ);
            moveDirection = ridePod.forward;
        }
        else if (posisi == 3)
        {
            // Kiri
            spawnPosition -= ridePod.right * jarakSamping;
            spawnPosition += ridePod.forward * Random.Range(-randomZ, randomZ);
            moveDirection = ridePod.forward;
        }

        // Biar tidak numpuk kalau jumlah traffic banyak
        float tambahanJarak = (index / 4) * 4f;
        spawnPosition += ridePod.forward * tambahanJarak;

        GameObject traffic = Instantiate(
            trafficPodPrefab,
            spawnPosition,
            Quaternion.LookRotation(moveDirection)
        );

        TrafficPodMovement movement = traffic.GetComponent<TrafficPodMovement>();

        if (movement != null)
        {
            movement.moveDirection = moveDirection;
            movement.speed = Random.Range(minSpeed, maxSpeed);
            movement.lifeTime = trafficLifeTime;
        }

        spawnedTraffic.Add(traffic);
    }

    private void BersihkanListKosong()
    {
        for (int i = spawnedTraffic.Count - 1; i >= 0; i--)
        {
            if (spawnedTraffic[i] == null || !spawnedTraffic[i].activeSelf)
            {
                spawnedTraffic.RemoveAt(i);
            }
        }
    }

    public void HilangkanTrafficAcak(int jumlah)
    {
        BersihkanListKosong();

        if (spawnedTraffic.Count == 0) return;

        int jumlahAktual = Mathf.Clamp(jumlah, 1, spawnedTraffic.Count);

        for (int i = 0; i < jumlahAktual; i++)
        {
            BersihkanListKosong();

            if (spawnedTraffic.Count == 0) return;

            int randomIndex = Random.Range(0, spawnedTraffic.Count);
            GameObject target = spawnedTraffic[randomIndex];

            if (target != null)
            {
                spawnedTraffic.RemoveAt(randomIndex);
                Destroy(target);

                Debug.Log("Traffic hilang acak: " + target.name);
            }
        }
    }

    public void HapusSemuaTraffic()
    {
        for (int i = spawnedTraffic.Count - 1; i >= 0; i--)
        {
            if (spawnedTraffic[i] != null)
                Destroy(spawnedTraffic[i]);
        }

        spawnedTraffic.Clear();
    }
}