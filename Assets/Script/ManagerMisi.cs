using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class SkenarioZona
{
    public string namaZona;

    [Header("Referensi Perangkat Tetap")]
    [Tooltip("Tarik komputer yang berada di ruangan/gedung ini")]
    public ComputerDevice komputerZona; 

    [HideInInspector] public string[] daftarMisi;
    [HideInInspector] public string[][] daftarHint; 
    [HideInInspector] public int misiSelesai = 0;

    [HideInInspector] public WirelessDevice wirelessDeviceZona;
}

public class ManagerMisi : MonoBehaviour
{
    public static ManagerMisi Instance;

    [Header("Referensi UI Panel Misi Dalam Gedung")]
    public GameObject panelDaftarMisi;
    public TextMeshProUGUI teksHeaderGedung;
    public Transform daftarMisiContainer;
    public GameObject itemMisiPrefab;

    [Header("Referensi UI Hint")]
    public GameObject panelHint; 
    public TextMeshProUGUI teksHint; 

    [Header("Data Misi")]
    public SkenarioZona[] semuaZona;

    private int zonaAktifID = -1;
    private List<MissionItemUI> listUIAktif = new List<MissionItemUI>();

    private const int SLOT_TOWER_BENAR = 3;
    
    // Variabel Timer
    private float timerMisiAktif = 0f;
    private int indexHintTampil = -1; 
    
    // BARU: Variabel Auto-Hide Hint
    private bool hintSedangTampil = false;
    private float timerSembunyikanHint = 0f;
    private float durasiTampilHint = 7f; // Hint akan hilang sendiri dari layar setelah 7 detik

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        InisialisasiMisiGedung();
    }

    void Start()
    {
        SembunyikanHint();
        PindahZona(-1);
    }

    void Update()
    {
        CekOtomatisMisiWirelessDevice();
        UpdateTimerHint();
    }

    // =========================================================
    // LOGIKA HINT WAKTU & AUTO-HIDE
    // =========================================================
    private void UpdateTimerHint()
    {
        if (zonaAktifID == -1) return;

        SkenarioZona zona = semuaZona[zonaAktifID];

        // Jika semua misi beres, pastikan hint hilang
        if (zona.misiSelesai >= zona.daftarMisi.Length)
        {
            SembunyikanHint();
            return;
        }

        timerMisiAktif += Time.deltaTime;

        // Logika menghitung waktu untuk menyembunyikan hint
        if (hintSedangTampil)
        {
            timerSembunyikanHint += Time.deltaTime;
            if (timerSembunyikanHint >= durasiTampilHint)
            {
                SembunyikanHint();
            }
        }

        // -----------------------------------------------------
        // LOGIKA KHUSUS GEDUNG 2: Hint LOS & Station
        // -----------------------------------------------------
        if (zonaAktifID == 1)
        {
            if (zona.misiSelesai >= 7 && timerMisiAktif >= 5f) 
            {
                if (zona.wirelessDeviceZona != null && !zona.wirelessDeviceZona.isConnected)
                {
                    // Panggil sekali saja per misi ini agar tidak tabrakan dengan timer auto-hide
                    if (indexHintTampil < 0) 
                    {
                        TampilkanPesanHint("Hint: Pastikan NanoStation dalam keadaan Station dan periksa ketinggian tower Gedung 1 LOS dengan Gedung 2.");
                        indexHintTampil = 0; 
                    }
                }
            }
            return; 
        }

        // -----------------------------------------------------
        // LOGIKA GEDUNG 1 (10s, 25s, 40s)
        // -----------------------------------------------------
        if (timerMisiAktif >= 40f && indexHintTampil < 2) TampilkanHintBerdasarkanLevel(zona, 2);
        else if (timerMisiAktif >= 25f && timerMisiAktif < 40f && indexHintTampil < 1) TampilkanHintBerdasarkanLevel(zona, 1);
        else if (timerMisiAktif >= 10f && timerMisiAktif < 25f && indexHintTampil < 0) TampilkanHintBerdasarkanLevel(zona, 0);
    }

    private void TampilkanHintBerdasarkanLevel(SkenarioZona zona, int levelHint)
    {
        indexHintTampil = levelHint;

        if (zona.daftarHint != null && zona.misiSelesai < zona.daftarHint.Length)
        {
            string[] hints = zona.daftarHint[zona.misiSelesai];
            if (hints != null && levelHint < hints.Length)
            {
                TampilkanPesanHint("" + hints[levelHint]);
            }
        }
    }

    // Fungsi utama untuk memunculkan panel, set teks, dan mereset timer auto-hide
    private void TampilkanPesanHint(string pesan)
    {
        if (teksHint != null) teksHint.text = pesan;
        if (panelHint != null) panelHint.SetActive(true);
        
        hintSedangTampil = true;
        timerSembunyikanHint = 0f; 
    }

    // Fungsi untuk menyembunyikan panel gambar dan teks
    private void SembunyikanHint()
    {
        hintSedangTampil = false;
        if (panelHint != null) panelHint.SetActive(false);
    }

    private void ResetTimerHint()
    {
        timerMisiAktif = 0f;
        indexHintTampil = -1;
        SembunyikanHint();
    }

    private void InisialisasiMisiGedung()
    {
        if (semuaZona == null || semuaZona.Length < 2)
        {
            Debug.LogError("Isi Size Semua Zona di Inspector menjadi minimal 2!");
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
            "Hubungkan POE dengan Komputer untuk konfigurasi awal",
            "Buat komputer 1 jaringan dengan NanoStation",
            "Ubah nama NanoStation menjadi AP",
            "Konfigurasi IP Address NanoStation menjadi 192.168.1.15",
            "Atur Wireless Mode menjadi Access Point",
            "Buat SSID jaringan dengan nama POINTING-PUSAT",
            "Atur Security Mode menjadi WPA2-AES",
            "Masukkan password wireless",
            "Simpan konfigurasi NanoStation AP",
            "Pastikan AP aktif dan siap menerima koneksi"
        };

        semuaZona[0].daftarHint = new string[][] {
            new string[] { "Ambil NanoStation dari tampilan bergerak di meja.", "Masukkan NanoStation ke Inventory lalu naik ke atas tower.", "Pasang NanoStation di slot yang menghadap ke arah gedung cabang." },
            new string[] { "Periksa kabel power.", "Hubungkan kabel power ke adaptor POE.", "Colokkan kabel power agar NanoStation menyala." },
            new string[] { "Siapkan kabel LAN.", "Hubungkan port LAN di POE ke komputer.", "Pastikan indikator LAN di perangkat menyala." },
            new string[] { "Buka pengaturan jaringan komputer.", "Atur IP komputer agar satu segmen (misal 192.168.1.x).", "Pastikan komputer bisa melakukan ping ke NanoStation." },
            new string[] { "Buka konfigurasi perangkat di browser.", "Cari pengaturan 'Device Name'.", "Ketikkan 'AP' pada kolom nama perangkat." },
            new string[] { "Buka tab Network di konfigurasi.", "Cari pengaturan IP Address.", "Ubah IP menjadi 192.168.1.15." },
            new string[] { "Buka tab Wireless.", "Perhatikan bagian Wireless Mode.", "Ubah mode dari dropdown menjadi 'Access Point'." },
            new string[] { "Masih di tab Wireless.", "Cari kolom pengisian SSID.", "Ketikkan tepat: POINTING-PUSAT" },
            new string[] { "Keamanan jaringan perlu diatur.", "Cari dropdown Security.", "Pilih opsi WPA2-AES." },
            new string[] { "Jaringan butuh kata sandi.", "Cari kolom Password di bawah Security.", "Ketikkan password yang mudah diingat." },
            new string[] { "Pastikan semua data sudah terisi.", "Cari tombol change.", "Klik tombol change untuk menerapkan perubahan." },
            new string[] { "Periksa indikator di perangkat.", "Cek status koneksi di dashboard.", "Pastikan status menampilkan Connected/Active." }
        };

        // ==========================================
        // GEDUNG 2 / CABANG
        // ==========================================
        semuaZona[1].namaZona = "Gedung 2 - Cabang";
        semuaZona[1].daftarMisi = new string[]
        {
            "Pasang NanoStation ke tower Gedung Cabang",
            "Pastikan NanoStation Gedung Cabang mendapatkan power",
            "Hubungkan POE dengan Komputer untuk konfigurasi awal",
            "Ubah nama NanoStation menjadi STATION",
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
        
        semuaZona[1].daftarHint = new string[0][]; 
    }

    public void PindahZona(int idZonaBaru)
    {
        zonaAktifID = idZonaBaru;
        ResetTimerHint();

        if (zonaAktifID == -1)
        {
            panelDaftarMisi.SetActive(false);
        }
        else
        {
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

        if (zona.misiSelesai < 3)
        {
            BuatItemUI(zona.daftarMisi[zona.misiSelesai], StatusMisi.Aktif);
            return;
        }

        for (int i = 3; i < zona.daftarMisi.Length; i++)
        {
            if (i < zona.misiSelesai) BuatItemUI(zona.daftarMisi[i], StatusMisi.Selesai);
            else if (i == zona.misiSelesai) BuatItemUI(zona.daftarMisi[i], StatusMisi.Aktif);
            else BuatItemUI(zona.daftarMisi[i], StatusMisi.Terkunci);
        }
    }

    private void BuatItemUI(string isiMisi, StatusMisi status)
    {
        GameObject obj = Instantiate(itemMisiPrefab, daftarMisiContainer);
        MissionItemUI ui = obj.GetComponent<MissionItemUI>();
        listUIAktif.Add(ui);
        ui.UpdateTampilan(isiMisi, status);
    }

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

        zona.wirelessDeviceZona = device;
        
        if (zona.misiSelesai == 0)
        {
            zona.misiSelesai++;
            if (zonaAktifID == idZonaDariTower) RenderUIZona(zonaAktifID);
            ResetTimerHint();
        }
    }

    private void CekOtomatisMisiWirelessDevice()
    {
        if (zonaAktifID == -1) return;
        if (zonaAktifID != 0 && zonaAktifID != 1) return;

        SkenarioZona zona = semuaZona[zonaAktifID];
        if (zona.misiSelesai == 0 || zona.wirelessDeviceZona == null) return;

        WirelessDevice device = zona.wirelessDeviceZona;

        if (zonaAktifID == 0) ValidasiMisiGedungPusat(zona, device);
        else if (zonaAktifID == 1) ValidasiMisiGedungCabang(zona, device);
    }

    private void ValidasiMisiGedungPusat(SkenarioZona zona, WirelessDevice device)
    {
        switch (zona.misiSelesai)
        {
            case 1: if (device.hasPower) SelesaikanMisiValid("Power NanoStation Pusat aktif"); break;
            case 2: if (device.physicalLinkActive) SelesaikanMisiValid("POE terhubung dengan komputer"); break;
            case 3: if (IsKomputerSatuJaringan(device, zona.komputerZona)) SelesaikanMisiValid("Komputer satu jaringan dengan NanoStation"); break; 
            case 4: if (CleanUpper(device.deviceName) == "AP") SelesaikanMisiValid("Nama device sudah AP"); break;
            case 5: if (Clean(device.ipAddress) == "192.168.1.15") SelesaikanMisiValid("IP Address AP benar"); break;
            case 6: if (device.mode == WirelessDevice.WirelessMode.AccessPoint) SelesaikanMisiValid("Mode sudah Access Point"); break;
            case 7: if (CleanUpper(device.ssid) == "POINTING-PUSAT") SelesaikanMisiValid("SSID AP benar"); break;
            case 8: if (device.securityMode == WirelessDevice.SecurityMode.WPA2_AES) SelesaikanMisiValid("Security Mode sudah WPA2-AES"); break;
            case 9: if (!string.IsNullOrWhiteSpace(device.password)) SelesaikanMisiValid("Password wireless sudah diisi"); break;
            case 10: if (KonfigurasiAPPusatLengkap(device)) SelesaikanMisiValid("Konfigurasi AP lengkap"); break;
            case 11: if (device.hasPower && device.mode == WirelessDevice.WirelessMode.AccessPoint && device.isConnected) SelesaikanMisiValid("AP aktif dan siap menerima koneksi"); break;
        }
    }

    private void ValidasiMisiGedungCabang(SkenarioZona zona, WirelessDevice device)
    {
        switch (zona.misiSelesai)
        {
            case 1: if (device.hasPower) SelesaikanMisiValid("Power NanoStation Cabang aktif"); break;
            case 2: if (device.physicalLinkActive) SelesaikanMisiValid("POE terhubung dengan komputer"); break;
            case 3: if (CleanUpper(device.deviceName) == "STATION") SelesaikanMisiValid("Nama device sudah STATION"); break;
            case 4: if (CekArahStationKeAP(device)) SelesaikanMisiValid("Arah Station sudah menghadap AP"); break;
            case 5: if (LANDeviceTerhubung(device)) SelesaikanMisiValid("LAN NanoStation Station terhubung ke Switch"); break;
            case 6: if (Clean(device.ipAddress) == "192.168.1.2") SelesaikanMisiValid("IP Address Station benar"); break;
            case 7: if (device.mode == WirelessDevice.WirelessMode.Station) SelesaikanMisiValid("Mode sudah Station"); break;
            case 8: if (CekAPGedungPusatTersedia()) SelesaikanMisiValid("AP Gedung Pusat ditemukan"); break;
            case 9: if (CleanUpper(device.lockToAPSSID) == "POINTING-PUSAT") SelesaikanMisiValid("SSID tujuan sudah POINTING-PUSAT"); break;
            case 10: if (PasswordStationSesuaiAP(device)) SelesaikanMisiValid("Password Station sesuai AP"); break;
            case 11: if (device.connectedAP != null) SelesaikanMisiValid("Station sudah terhubung ke AP"); break;
            case 12: if (device.isConnected && device.connectedAP != null) SelesaikanMisiValid("Koneksi wireless berhasil"); break;
            case 13: if (device.currentSignal > -75f) SelesaikanMisiValid("Sinyal pointing baik"); break;
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
            ResetTimerHint();
        }
    }

    // =========================================================
    // HELPER VALIDASI
    // =========================================================
    
    private bool IsKomputerSatuJaringan(WirelessDevice nanoDevice, ComputerDevice komputerZonaIni)
    {
        if (nanoDevice == null || komputerZonaIni == null) return false;

        if (!TryParseIP(komputerZonaIni.ipAddress, out uint pcIP)) return false;
        if (!TryParseIP(komputerZonaIni.netmask, out uint pcMask)) return false;
        if (!TryParseIP(nanoDevice.ipAddress, out uint nanoIP)) return false;

        return IsSameSubnet(pcIP, nanoIP, pcMask);
    }

    private bool LANDeviceTerhubung(WirelessDevice device) { return device != null && (device.lan0Status == "Connected" || device.lan1Status == "Connected"); }
    
    private bool KonfigurasiAPPusatLengkap(WirelessDevice device)
    {
        if (device == null || !device.hasPower || CleanUpper(device.deviceName) != "AP" || Clean(device.ipAddress) != "192.168.1.15") return false;
        if (device.mode != WirelessDevice.WirelessMode.AccessPoint || CleanUpper(device.ssid) != "POINTING-PUSAT") return false;
        if (device.securityMode != WirelessDevice.SecurityMode.WPA2_AES || string.IsNullOrWhiteSpace(device.password)) return false;
        return true;
    }
    
    private bool CekAPGedungPusatTersedia()
    {
        if (semuaZona == null || semuaZona.Length < 1) return false;
        WirelessDevice ap = semuaZona[0].wirelessDeviceZona;
        return ap != null && ap.hasPower && ap.mode == WirelessDevice.WirelessMode.AccessPoint && CleanUpper(ap.ssid) == "POINTING-PUSAT";
    }
    
    private bool PasswordStationSesuaiAP(WirelessDevice station)
    {
        if (station == null || semuaZona == null || semuaZona.Length < 1) return false;
        WirelessDevice ap = semuaZona[0].wirelessDeviceZona;
        return ap != null && !string.IsNullOrWhiteSpace(station.password) && !string.IsNullOrWhiteSpace(ap.password) && station.password == ap.password;
    }
    
    private bool CekArahStationKeAP(WirelessDevice station)
    {
        if (station == null || semuaZona == null || semuaZona.Length < 1 || semuaZona[0].wirelessDeviceZona == null) return false;
        WirelessDevice ap = semuaZona[0].wirelessDeviceZona;
        Vector3 arahKeAP = (ap.transform.position - station.transform.position).normalized;
        return Vector3.Angle(station.transform.forward, arahKeAP) <= 20f;
    }

    private string Clean(string value) { return string.IsNullOrEmpty(value) ? "" : value.Trim(); }
    private string CleanUpper(string value) { return string.IsNullOrEmpty(value) ? "" : value.Trim().ToUpper(); }

    private bool TryParseIP(string ip, out uint result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(ip)) return false;

        string[] parts = ip.Trim().Split('.');
        if (parts.Length != 4) return false;

        for (int i = 0; i < 4; i++)
        {
            if (!int.TryParse(parts[i], out int value)) return false;
            if (value < 0 || value > 255) return false;
            
            result = (result << 8) + (uint)value;
        }

        return true;
    }

    private uint GetNetworkAddress(uint ip, uint mask)
    {
        return ip & mask;
    }

    private bool IsSameSubnet(uint ip1, uint ip2, uint mask)
    {
        return GetNetworkAddress(ip1, mask) == GetNetworkAddress(ip2, mask);
    }
}