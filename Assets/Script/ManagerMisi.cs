using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Wajib ditambahkan untuk memanggil komponen Image (Bar)
using TMPro;

[System.Serializable]
public class SkenarioZona
{
    public string namaZona;
    [TextArea(2,3)] public string[] daftarMisi;
    [HideInInspector] public int misiSelesai = 0; 

    [Header("Referensi UI Progress Bar Zona Ini")]
    [Tooltip("Tarik Image garis biru milik zona ini yang tipe Filled")]
    public Image barProgressZona; 
    [Tooltip("Tarik teks persentase (misal '75%') milik zona ini")]
    public TextMeshProUGUI teksPersenZona; 
}

public class ManagerMisi : MonoBehaviour
{
    public static ManagerMisi Instance;

    [Header("Referensi UI Panel Misi (Dalam Gedung)")]
    public GameObject panelDaftarMisi; 
    public TextMeshProUGUI teksHeaderGedung; 
    public Transform daftarMisiContainer;
    public GameObject itemMisiPrefab;
    
    [Header("Referensi UI Dashboard Persentase (Luar Gedung)")]
    public GameObject panelPersentase; 
    
    [Header("UI Progress Keseluruhan (Global)")]
    public Image barProgressGlobal;
    public TextMeshProUGUI teksPersenGlobal;

    [Header("Data Misi (Isi di Inspector)")]
    public SkenarioZona[] semuaZona; 
    
    private int zonaAktifID = -1; 
    private List<MissionItemUI> listUIAktif = new List<MissionItemUI>();

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        PindahZona(-1); 
    }

    public void PindahZona(int idZonaBaru)
    {
        zonaAktifID = idZonaBaru;

        if (zonaAktifID == -1)
        {
            panelDaftarMisi.SetActive(false); 
            panelPersentase.SetActive(true);  
            UpdateDashboardPersentase(); // Update angka dan bar saat buka dashboard
        }
        else
        {
            panelPersentase.SetActive(false);
            panelDaftarMisi.SetActive(true);
            RenderUIZona(zonaAktifID);
        }
    }

    private void RenderUIZona(int idZona)
    {
        SkenarioZona zona = semuaZona[idZona];
        teksHeaderGedung.text = zona.namaZona.ToUpper(); 

        foreach (Transform child in daftarMisiContainer) Destroy(child.gameObject);
        listUIAktif.Clear();

        for (int i = 0; i < zona.daftarMisi.Length; i++)
        {
            GameObject obj = Instantiate(itemMisiPrefab, daftarMisiContainer);
            MissionItemUI ui = obj.GetComponent<MissionItemUI>();
            listUIAktif.Add(ui);

            if (i < zona.misiSelesai) ui.UpdateTampilan(zona.daftarMisi[i], StatusMisi.Selesai);
            else if (i == zona.misiSelesai) ui.UpdateTampilan(zona.daftarMisi[i], StatusMisi.Aktif);
            else ui.UpdateTampilan(zona.daftarMisi[i], StatusMisi.Terkunci);
        }
    }

    public void MisiSelesai()
    {
        if (zonaAktifID == -1) return; 

        SkenarioZona zona = semuaZona[zonaAktifID];
        
        if (zona.misiSelesai < zona.daftarMisi.Length)
        {
            zona.misiSelesai++; 
            RenderUIZona(zonaAktifID); 
            UpdateDashboardPersentase(); 
        }
    }

    // Fungsi baru untuk menghitung dan mengisi bar UI desain baru kamu
    private void UpdateDashboardPersentase()
    {
        float totalMisiGlobal = 0;
        float totalSelesaiGlobal = 0;

        // Loop untuk menghitung dan mengupdate bar per-Gedung
        foreach (var zona in semuaZona)
        {
            float totalMisiZona = zona.daftarMisi.Length;
            float selesaiZona = zona.misiSelesai;

            // Hitung persen zona ini (0.0 sampai 1.0)
            float persenZona = (totalMisiZona == 0) ? 0 : (selesaiZona / totalMisiZona);

            // Update UI Bar dan Teks khusus zona ini
            if (zona.barProgressZona != null) zona.barProgressZona.fillAmount = persenZona;
            if (zona.teksPersenZona != null) zona.teksPersenZona.text = Mathf.RoundToInt(persenZona * 100f) + "%";

            // Tambahkan ke akumulasi global
            totalMisiGlobal += totalMisiZona;
            totalSelesaiGlobal += selesaiZona;
        }

        // Hitung dan update UI Global (Progress Keseluruhan)
        float persenGlobal = (totalMisiGlobal == 0) ? 0 : (totalSelesaiGlobal / totalMisiGlobal);
        
        if (barProgressGlobal != null) barProgressGlobal.fillAmount = persenGlobal;
        if (teksPersenGlobal != null) teksPersenGlobal.text = Mathf.RoundToInt(persenGlobal * 100f) + "%";
    }
}