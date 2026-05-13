using UnityEngine;

public class SensorGedung : MonoBehaviour
{
    [Tooltip("ID Zona: 0 untuk Gedung Utama, 1 untuk Gedung Cabang, 2 untuk Ruang Server")]
    public int idZonaGedung;

    private void OnTriggerEnter(Collider other)
    {
        // Jika yang menyentuh kotak ini adalah pemain
        if (other.CompareTag("Player"))
        {
            ManagerMisi.Instance.PindahZona(idZonaGedung);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Keluar gedung, kembali ke mode persentase (-1)
            ManagerMisi.Instance.PindahZona(-1); 
        }
    }
}