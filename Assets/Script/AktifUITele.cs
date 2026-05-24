using UnityEngine;

public class AktifUITele : MonoBehaviour
{
    [Header("Masukkan Grup Tombol Kesini")]
    public GameObject panelTombol;

    // Saat Player menginjak zona
    private void OnTriggerEnter(Collider other)
    {
        // Mengecek apakah yang menginjak memiliki Tag "Player"
        if (other.CompareTag("Player"))
        {
            if (panelTombol != null)
            {
                Debug.Log("Player masuk zona teleport. Menampilkan tombol.");
                panelTombol.SetActive(true); // Memunculkan tombol
            }
        }
    }

    // Saat Player keluar dari zona
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (panelTombol != null)
            {
                panelTombol.SetActive(false); // Menghilangkan tombol
            }
        }
    }
}