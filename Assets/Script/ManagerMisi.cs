using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class SkenarioZona
{
    public string namaZona;

    [HideInInspector] public string[] daftarMisi;
    [HideInInspector] public int misiSelesai = 0;

    // Device ini otomatis diisi saat NanoStation dipasang ke tower
    [HideInInspector] public WirelessDevice wirelessDeviceZona;

    [Header("Referensi UI Progress Bar Zona Ini")]
    [Tooltip("Tarik Image garis biru milik zona ini yang tipe Filled")]
    public Image barProgressZona;

    [Tooltip("Tarik teks persentase misal '75%' milik zona ini")]
    public TextMeshProUGUI teksPersenZona;
}

public class ManagerMisi : MonoBehaviour
{
    public static ManagerMisi Instance;

    [Header("Referensi UI Panel Misi Dalam Gedung")]
    public GameObject panelDaftarMisi;
    public TextMeshProUGUI teksHeaderGedung;
    public Transform daftarMisiContainer;
    public GameObject itemMisiPrefab;

    [Header("Referensi UI Dashboard Persentase Luar Gedung")]
    public GameObject panelPersentase;

    [Header("UI Progress Keseluruhan Global")]
    public Image barProgressGlobal;
    public TextMeshProUGUI teksPersenGlobal;

    [Header("Data Misi")]
    public SkenarioZona[] semuaZona;

    private int zonaAktifID = -1;
    private List<MissionItemUI> listUIAktif = new List<MissionItemUI>();

    private const int SLOT_TOWER_BENAR = 3;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InisialisasiMisiGedung();
    }

    void Start()
    {
        PindahZona(-1);
    }

    void Update()
    {
        CekOtomatisMisiWirelessDevice();
    }

    private void InisialisasiMisiGedung()
    {
        if (semuaZona == null || semuaZona.Length < 3)
        {
            Debug.LogError("Isi Size Semua Zona di Inspector menjadi minimal 3: Gedung Pusat, Gedung Cabang, Ruang Komputer!");
            return;
        }

        // ==========================================
        // GEDUNG 1 / PUSAT
        // ==========================================
        semuaZona[0].namaZona = "Gedung 1 - Pusat";
        semuaZona[0].daftarMisi = new string[]
        {
            "Pasang NanoStation ke tower Gedung Pusat",
            "Pastikan NanoStation Gedung Pusat mendapatkan power",
            "Ubah nama Wireless Device menjadi AP",
            "Hubungkan kabel LAN dari sumber internet ke Switch utama",
            "Hubungkan kabel LAN dari Switch utama ke NanoStation AP",
            "Konfigurasi IP Address AP menjadi 192.168.1.1",
            "Atur Wireless Mode menjadi Access Point",
            "Buat SSID jaringan dengan nama POINTING-PUSAT",
            "Atur Security Mode menjadi WPA2-AES",
            "Masukkan password wireless",
            "Simpan konfigurasi NanoStation AP",
            "Pastikan AP aktif dan siap menerima koneksi"
        };

        // ==========================================
        // GEDUNG 2 / CABANG
        // ==========================================
        semuaZona[1].namaZona = "Gedung 2 - Cabang";
        semuaZona[1].daftarMisi = new string[]
        {
            "Pasang NanoStation ke tower Gedung Cabang",
            "Pastikan NanoStation Gedung Cabang mendapatkan power",
            "Ubah nama Wireless Device menjadi STATION",
            "Arahkan NanoStation Cabang ke Gedung Pusat",
            "Hubungkan kabel LAN dari NanoStation Station ke Switch Cabang",
            "Konfigurasi IP Address Station menjadi 192.168.1.2",
            "Atur Wireless Mode menjadi Station",
            "Scan jaringan wireless dari Gedung Pusat",
            "Pilih SSID POINTING-PUSAT",
            "Masukkan password wireless yang sesuai",
            "Hubungkan Station ke Access Point Gedung Pusat",
            "Pastikan status koneksi wireless berhasil",
            "Pastikan indikator sinyal pointing dalam kondisi baik"
        };

        // ==========================================
        // RUANG KOMPUTER
        // ==========================================
        semuaZona[2].namaZona = "Ruang Komputer";
        semuaZona[2].daftarMisi = new string[]
        {
            "Nyalakan komputer client di Ruang Komputer",
            "Hubungkan kabel LAN dari komputer ke Switch Cabang",
            "Buka menu konfigurasi jaringan pada komputer",
            "Atur IP Address komputer menjadi 192.168.1.10",
            "Atur Subnet Mask menjadi 255.255.255.0",
            "Atur Gateway komputer menjadi 192.168.1.1",
            "Simpan konfigurasi jaringan komputer",
            "Lakukan Ping Test ke IP AP 192.168.1.1",
            "Lakukan Ping Test ke IP Station 192.168.1.2",
            "Pastikan hasil Ping Test berhasil",
            "Selesaikan simulasi konfigurasi jaringan point to point"
        };
    }

    public void PindahZona(int idZonaBaru)
    {
        zonaAktifID = idZonaBaru;

        if (zonaAktifID == -1)
        {
            panelDaftarMisi.SetActive(false);
            panelPersentase.SetActive(true);
            UpdateDashboardPersentase();
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
        if (idZona < 0 || idZona >= semuaZona.Length) return;

        SkenarioZona zona = semuaZona[idZona];
        teksHeaderGedung.text = zona.namaZona.ToUpper();

        foreach (Transform child in daftarMisiContainer)
        {
            Destroy(child.gameObject);
        }

        listUIAktif.Clear();

        // ==========================================
        // GEDUNG 1 DAN GEDUNG 2
        // Misi 0 = Pasang Nano ke tower
        // Misi 1 = Power
        // Misi 2 = Ganti nama
        // Setelah itu baru konfigurasi muncul
        // ==========================================
        if (idZona == 0 || idZona == 1)
        {
            if (zona.misiSelesai < 3)
            {
                BuatItemUI(zona.daftarMisi[zona.misiSelesai], StatusMisi.Aktif);
                return;
            }

            for (int i = 3; i < zona.daftarMisi.Length; i++)
            {
                if (i < zona.misiSelesai)
                {
                    BuatItemUI(zona.daftarMisi[i], StatusMisi.Selesai);
                }
                else if (i == zona.misiSelesai)
                {
                    BuatItemUI(zona.daftarMisi[i], StatusMisi.Aktif);
                }
                else
                {
                    BuatItemUI(zona.daftarMisi[i], StatusMisi.Terkunci);
                }
            }

            return;
        }

        // ==========================================
        // RUANG KOMPUTER
        // ==========================================
        for (int i = 0; i < zona.daftarMisi.Length; i++)
        {
            if (i < zona.misiSelesai)
            {
                BuatItemUI(zona.daftarMisi[i], StatusMisi.Selesai);
            }
            else if (i == zona.misiSelesai)
            {
                BuatItemUI(zona.daftarMisi[i], StatusMisi.Aktif);
            }
            else
            {
                BuatItemUI(zona.daftarMisi[i], StatusMisi.Terkunci);
            }
        }
    }

    private void BuatItemUI(string isiMisi, StatusMisi status)
    {
        GameObject obj = Instantiate(itemMisiPrefab, daftarMisiContainer);
        MissionItemUI ui = obj.GetComponent<MissionItemUI>();

        listUIAktif.Add(ui);
        ui.UpdateTampilan(isiMisi, status);
    }

    // =========================================================
    // DIPANGGIL DARI TOWERMOUNTSLOT
    // =========================================================
    public void CekMisiPasangNanoKeTower(WirelessDevice device, int idZonaDariTower, int nomorSlot)
    {
        if (device == null) return;

        if (idZonaDariTower != 0 && idZonaDariTower != 1) return;
        if (idZonaDariTower < 0 || idZonaDariTower >= semuaZona.Length) return;

        SkenarioZona zona = semuaZona[idZonaDariTower];

        if (nomorSlot != SLOT_TOWER_BENAR)
        {
            Debug.Log("Nano terpasang, tapi bukan di titik " + SLOT_TOWER_BENAR + ". Misi belum selesai.");
            return;
        }

        // Nano ini sekarang resmi menjadi device milik gedung tempat ia dipasang
        zona.wirelessDeviceZona = device;

        Debug.Log("NanoStation menjadi device milik: " + zona.namaZona);

        // Misi pasang Nano ke tower adalah index 0
        if (zona.misiSelesai == 0)
        {
            zona.misiSelesai++;

            if (zonaAktifID == idZonaDariTower)
            {
                RenderUIZona(zonaAktifID);
            }

            UpdateDashboardPersentase();

            Debug.Log("Misi pasang NanoStation ke titik " + SLOT_TOWER_BENAR + " tower SUKSES!");
        }
    }

    // =========================================================
    // VALIDASI OTOMATIS SEMUA MISI WIRELESS DEVICE
    // =========================================================
    private void CekOtomatisMisiWirelessDevice()
    {
        if (zonaAktifID == -1) return;

        // Untuk sekarang validasi WirelessDevice hanya Gedung 1 dan Gedung 2
        if (zonaAktifID != 0 && zonaAktifID != 1) return;

        SkenarioZona zona = semuaZona[zonaAktifID];

        // Misi index 0 tetap dari TowerMountSlot
        if (zona.misiSelesai == 0) return;

        if (zona.wirelessDeviceZona == null)
        {
            return;
        }

        WirelessDevice device = zona.wirelessDeviceZona;

        if (zonaAktifID == 0)
        {
            ValidasiMisiGedungPusat(zona, device);
        }
        else if (zonaAktifID == 1)
        {
            ValidasiMisiGedungCabang(zona, device);
        }
    }

    private void ValidasiMisiGedungPusat(SkenarioZona zona, WirelessDevice device)
    {
        switch (zona.misiSelesai)
        {
            case 1:
                // Pastikan NanoStation Gedung Pusat mendapatkan power
                if (device.hasPower)
                {
                    SelesaikanMisiValid("Power NanoStation Pusat aktif");
                }
                break;

            case 2:
                // Ubah nama Wireless Device menjadi AP
                if (CleanUpper(device.deviceName) == "AP")
                {
                    SelesaikanMisiValid("Nama device sudah AP");
                }
                break;

            case 3:
                // Hubungkan kabel LAN dari sumber internet ke Switch utama
                // Diarahkan ke WirelessDevice melalui NetworkDevice.hasInternet
                if (device.networkDevice != null && device.networkDevice.hasInternet)
                {
                    SelesaikanMisiValid("Internet Gedung Pusat aktif");
                }
                break;

            case 4:
                // Hubungkan kabel LAN dari Switch utama ke NanoStation AP
                if (LANDeviceTerhubung(device))
                {
                    SelesaikanMisiValid("LAN NanoStation AP terhubung");
                }
                break;

            case 5:
                // Konfigurasi IP Address AP menjadi 192.168.1.1
                if (Clean(device.ipAddress) == "192.168.1.1")
                {
                    SelesaikanMisiValid("IP Address AP benar");
                }
                break;

            case 6:
                // Atur Wireless Mode menjadi Access Point
                if (device.mode == WirelessDevice.WirelessMode.AccessPoint)
                {
                    SelesaikanMisiValid("Mode sudah Access Point");
                }
                break;

            case 7:
                // Buat SSID POINTING-PUSAT
                if (CleanUpper(device.ssid) == "POINTING-PUSAT")
                {
                    SelesaikanMisiValid("SSID AP benar");
                }
                break;

            case 8:
                // Atur Security Mode menjadi WPA2-AES
                if (device.securityMode == WirelessDevice.SecurityMode.WPA2_AES)
                {
                    SelesaikanMisiValid("Security Mode sudah WPA2-AES");
                }
                break;

            case 9:
                // Masukkan password wireless
                if (!string.IsNullOrWhiteSpace(device.password))
                {
                    SelesaikanMisiValid("Password wireless sudah diisi");
                }
                break;

            case 10:
                // Simpan konfigurasi NanoStation AP
                // Karena tidak ada variabel khusus tombol Save di WirelessDevice,
                // validasi dianggap selesai kalau konfigurasi utama sudah lengkap.
                if (KonfigurasiAPPusatLengkap(device))
                {
                    SelesaikanMisiValid("Konfigurasi AP lengkap");
                }
                break;

            case 11:
                // Pastikan AP aktif dan siap menerima koneksi
                if (device.hasPower &&
                    device.mode == WirelessDevice.WirelessMode.AccessPoint &&
                    device.isConnected)
                {
                    SelesaikanMisiValid("AP aktif dan siap menerima koneksi");
                }
                break;
        }
    }

    private void ValidasiMisiGedungCabang(SkenarioZona zona, WirelessDevice device)
    {
        switch (zona.misiSelesai)
        {
            case 1:
                // Pastikan NanoStation Gedung Cabang mendapatkan power
                if (device.hasPower)
                {
                    SelesaikanMisiValid("Power NanoStation Cabang aktif");
                }
                break;

            case 2:
                // Ubah nama Wireless Device menjadi STATION
                if (CleanUpper(device.deviceName) == "STATION")
                {
                    SelesaikanMisiValid("Nama device sudah STATION");
                }
                break;

            case 3:
                // Arahkan NanoStation Cabang ke Gedung Pusat
                if (CekArahStationKeAP(device))
                {
                    SelesaikanMisiValid("Arah Station sudah menghadap AP");
                }
                break;

            case 4:
                // Hubungkan kabel LAN dari NanoStation Station ke Switch Cabang
                if (LANDeviceTerhubung(device))
                {
                    SelesaikanMisiValid("LAN NanoStation Station terhubung");
                }
                break;

            case 5:
                // Konfigurasi IP Address Station menjadi 192.168.1.2
                if (Clean(device.ipAddress) == "192.168.1.2")
                {
                    SelesaikanMisiValid("IP Address Station benar");
                }
                break;

            case 6:
                // Atur Wireless Mode menjadi Station
                if (device.mode == WirelessDevice.WirelessMode.Station)
                {
                    SelesaikanMisiValid("Mode sudah Station");
                }
                break;

            case 7:
                // Scan jaringan wireless dari Gedung Pusat
                if (CekAPGedungPusatTersedia())
                {
                    SelesaikanMisiValid("AP Gedung Pusat ditemukan");
                }
                break;

            case 8:
                // Pilih SSID POINTING-PUSAT
                if (CleanUpper(device.lockToAPSSID) == "POINTING-PUSAT")
                {
                    SelesaikanMisiValid("SSID tujuan sudah POINTING-PUSAT");
                }
                break;

            case 9:
                // Masukkan password wireless yang sesuai
                if (PasswordStationSesuaiAP(device))
                {
                    SelesaikanMisiValid("Password Station sesuai AP");
                }
                break;

            case 10:
                // Hubungkan Station ke Access Point Gedung Pusat
                if (device.connectedAP != null)
                {
                    SelesaikanMisiValid("Station sudah terhubung ke AP");
                }
                break;

            case 11:
                // Pastikan status koneksi wireless berhasil
                if (device.isConnected && device.connectedAP != null)
                {
                    SelesaikanMisiValid("Koneksi wireless berhasil");
                }
                break;

            case 12:
                // Pastikan indikator sinyal pointing dalam kondisi baik
                if (device.currentSignal > -75f)
                {
                    SelesaikanMisiValid("Sinyal pointing baik");
                }
                break;
        }
    }

    private void SelesaikanMisiValid(string pesan)
    {
        Debug.Log("Misi valid: " + pesan);
        MisiSelesai();
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

    // =========================================================
    // HELPER VALIDASI WIRELESSDEVICE
    // =========================================================
    private bool LANDeviceTerhubung(WirelessDevice device)
    {
        if (device == null) return false;

        return device.lan0Status == "Connected" ||
               device.lan1Status == "Connected";
    }

    private bool KonfigurasiAPPusatLengkap(WirelessDevice device)
    {
        if (device == null) return false;

        if (!device.hasPower) return false;
        if (CleanUpper(device.deviceName) != "AP") return false;
        if (Clean(device.ipAddress) != "192.168.1.1") return false;
        if (device.mode != WirelessDevice.WirelessMode.AccessPoint) return false;
        if (CleanUpper(device.ssid) != "POINTING-PUSAT") return false;
        if (device.securityMode != WirelessDevice.SecurityMode.WPA2_AES) return false;
        if (string.IsNullOrWhiteSpace(device.password)) return false;

        return true;
    }

    private bool CekAPGedungPusatTersedia()
    {
        if (semuaZona == null || semuaZona.Length < 1) return false;

        WirelessDevice ap = semuaZona[0].wirelessDeviceZona;

        if (ap == null) return false;
        if (!ap.hasPower) return false;
        if (ap.mode != WirelessDevice.WirelessMode.AccessPoint) return false;
        if (CleanUpper(ap.ssid) != "POINTING-PUSAT") return false;

        return true;
    }

    private bool PasswordStationSesuaiAP(WirelessDevice station)
    {
        if (station == null) return false;
        if (semuaZona == null || semuaZona.Length < 1) return false;

        WirelessDevice ap = semuaZona[0].wirelessDeviceZona;

        if (ap == null) return false;
        if (string.IsNullOrWhiteSpace(station.password)) return false;
        if (string.IsNullOrWhiteSpace(ap.password)) return false;

        return station.password == ap.password;
    }

    private bool CekArahStationKeAP(WirelessDevice station)
    {
        if (station == null) return false;
        if (semuaZona == null || semuaZona.Length < 1) return false;

        WirelessDevice ap = semuaZona[0].wirelessDeviceZona;

        if (ap == null) return false;

        Vector3 arahKeAP = (ap.transform.position - station.transform.position).normalized;
        float sudut = Vector3.Angle(station.transform.forward, arahKeAP);

        return sudut <= 20f;
    }

    private string Clean(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Trim();
    }

    private string CleanUpper(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Trim().ToUpper();
    }

    private void UpdateDashboardPersentase()
    {
        float totalMisiGlobal = 0;
        float totalSelesaiGlobal = 0;

        foreach (var zona in semuaZona)
        {
            float totalMisiZona = zona.daftarMisi != null ? zona.daftarMisi.Length : 0;
            float selesaiZona = zona.misiSelesai;

            float persenZona = totalMisiZona == 0 ? 0 : selesaiZona / totalMisiZona;

            if (zona.barProgressZona != null)
                zona.barProgressZona.fillAmount = persenZona;

            if (zona.teksPersenZona != null)
                zona.teksPersenZona.text = Mathf.RoundToInt(persenZona * 100f) + "%";

            totalMisiGlobal += totalMisiZona;
            totalSelesaiGlobal += selesaiZona;
        }

        float persenGlobal = totalMisiGlobal == 0 ? 0 : totalSelesaiGlobal / totalMisiGlobal;

        if (barProgressGlobal != null)
            barProgressGlobal.fillAmount = persenGlobal;

        if (teksPersenGlobal != null)
            teksPersenGlobal.text = Mathf.RoundToInt(persenGlobal * 100f) + "%";
    }
}