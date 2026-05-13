using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum StatusMisi { Terkunci, Aktif, Selesai }

public class MissionItemUI : MonoBehaviour
{
    [Header("Referensi Komponen UI")]
    public Image ikonStatus;
    public TextMeshProUGUI teksMisi;

    [Header("Aset Ikon (Masukkan gambar dari folder)")]
    public Sprite ikonSelesai;  // Centang Hijau
    public Sprite ikonAktif;    // Lingkaran Kosong
    public Sprite ikonTerkunci; // Gembok Abu-abu

    // Fungsi ini dipanggil oleh MissionManager untuk mengubah tampilan
    public void UpdateTampilan(string namaMisi, StatusMisi status)
    {
        teksMisi.text = namaMisi;

        switch (status)
        {
            case StatusMisi.Selesai:
                ikonStatus.sprite = ikonSelesai;
                ikonStatus.color = Color.white; // Ikon sudah hijau dari gambarnya
                teksMisi.color = Color.white;
                teksMisi.fontStyle = FontStyles.Normal;
                break;

            case StatusMisi.Aktif:
                ikonStatus.sprite = ikonAktif;
                ikonStatus.color = Color.white; 
                teksMisi.color = Color.white;
                teksMisi.fontStyle = FontStyles.Bold; // Di-bold agar siswa fokus ke sini
                break;

            case StatusMisi.Terkunci:
                ikonStatus.sprite = ikonTerkunci;
                ikonStatus.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Warna abu-abu
                teksMisi.color = new Color(0.5f, 0.5f, 0.5f, 1f);   // Teks abu-abu
                teksMisi.fontStyle = FontStyles.Normal;
                break;
        }
    }
}