using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ComputerHomeUI : MonoBehaviour
{
    [Header("Device")]
    public ComputerDevice device;

    [Header("Panels")]
    public GameObject Header;
    public GameObject homePanel;
    public GameObject ipPanel;
    public GameObject pingPanel;
    public GameObject browserPanel;

    // ================= SIMPLE NETWORK STATUS =================
    [Header("Simple Network Status")]
    public TMP_Text connectionStatusText;
    public Image connectionIcon;

    [Header("Network Icon Sprite")]
    public Sprite iconDisconnected;
    public Sprite iconLanConnected;
    public Sprite iconConnected;
    public Sprite iconWarning;

    [Header("Network Status Color")]
    public Color connectedColor = Color.green;
    public Color warningColor = new Color(1f, 0.65f, 0f);
    public Color disconnectedColor = Color.red;
    public Color normalColor = Color.white;
    public Color gadaColor = new Color(1f, 0.65f, 0f);

    [Header("Network Status Setting")]
    public string defaultNanoIP = "192.168.1.20";
    public bool autoUpdateNetworkStatus = true;
    public float networkStatusRefreshRate = 0.25f;

    private float networkStatusTimer;

    // ================= IP CONFIG UI =================
    [Header("IP Config UI")]
    public TMP_InputField ipInput;
    public TMP_InputField gatewayInput;
    public TMP_InputField netmaskInput;
    public TMP_InputField dnsInput;

    // ================= PING UI =================
    [Header("Ping UI")]
    public TMP_InputField pingTargetInput;
    public TMP_Text pingResultText;

    [Header("Ping Setting")]
    public float pingLineDelay = 0.35f;

    // ================= BROWSER UI =================
    [Header("Browser UI")]
    public TMP_InputField addressInput;
    public TMP_Text browserResultText;

    [Header("Browser Result Canvas")]
    public GameObject browserContentPanel;
    public GameObject nanoUICanvas;
    public GameObject errorCanvas;
    public TMP_Text errorText;

    [Header("Nano UI Controller")]
    public UI_WirelessDevice nanoUIController;

    // ================= UI SOUND =================
    [Header("UI Sound")]
    public bool enableUISound = true;
    public bool autoFindAudioSource = true;
    public bool autoRegisterButtonSound = true;

    [Range(0f, 1f)] public float uiVolume = 1f;
    public AudioSource audioSource;

    [Header("Button Sound")]
    public AudioClip hoverClip;
    public AudioClip clickClip;
    public AudioClip panelOpenClip;

    [Header("Action Sound")]
    public AudioClip applyClip;
    public AudioClip cancelClip;
    public AudioClip pingStartClip;
    public AudioClip pingReplyClip;
    public AudioClip pingFailClip;
    public AudioClip browserOpenClip;
    public AudioClip successClip;
    public AudioClip errorClip;

    [Header("Network Status Sound")]
    public AudioClip connectedClip;
    public AudioClip warningClip;
    public AudioClip disconnectedClip;
    public AudioClip lanConnectedClip;

    [Header("Optional Extra Buttons")]
    public Button[] extraButtons;

    private Coroutine pingCoroutine;
    private bool isInitializing = true;
    private readonly HashSet<Button> registeredSoundButtons = new HashSet<Button>();

    private enum NetworkSoundState
    {
        Unknown,
        Disconnected,
        Warning,
        LanConnected,
        Connected
    }

    private NetworkSoundState lastNetworkSoundState = NetworkSoundState.Unknown;

    private void Start()
    {
        isInitializing = true;
        InitAudio();
        RegisterButtonSounds();

        ShowHome();
        LoadIPConfigFromDevice();

        if (pingTargetInput != null && string.IsNullOrWhiteSpace(pingTargetInput.text))
        {
            pingTargetInput.text = defaultNanoIP;
        }

        if (addressInput != null && string.IsNullOrWhiteSpace(addressInput.text))
        {
            addressInput.text = defaultNanoIP;
        }

        HideBrowserResults();
        UpdateNetworkStatus();

        isInitializing = false;
    }

    private void Update()
    {
        if (!autoUpdateNetworkStatus) return;

        networkStatusTimer += Time.deltaTime;

        if (networkStatusTimer >= networkStatusRefreshRate)
        {
            networkStatusTimer = 0f;
            UpdateNetworkStatus();
        }
    }

    // ================= PANEL NAVIGATION =================

    public void ShowHome()
    {
        PlayUISound(panelOpenClip);

        SetPanel(Header, true);
        SetPanel(homePanel, true);
        SetPanel(ipPanel, false);
        SetPanel(pingPanel, false);
        SetPanel(browserPanel, false);

        UpdateNetworkStatus();
    }

    public void ShowIPConfig()
    {
        PlayUISound(panelOpenClip);

        SetPanel(Header, true);
        SetPanel(homePanel, false);
        SetPanel(ipPanel, true);
        SetPanel(pingPanel, false);
        SetPanel(browserPanel, false);

        LoadIPConfigFromDevice();
        UpdateNetworkStatus();
    }

    public void ShowPing()
    {
        PlayUISound(panelOpenClip);

        SetPanel(Header, true);
        SetPanel(homePanel, false);
        SetPanel(ipPanel, false);
        SetPanel(pingPanel, true);
        SetPanel(browserPanel, false);

        UpdateNetworkStatus();
    }

    public void ShowBrowser()
    {
        PlayUISound(panelOpenClip);

        SetPanel(Header, true);
        SetPanel(homePanel, false);
        SetPanel(ipPanel, false);
        SetPanel(pingPanel, false);
        SetPanel(browserPanel, true);

        HideBrowserResults();
        UpdateNetworkStatus();
    }

    private void SetPanel(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    // ================= SIMPLE NETWORK STATUS =================

    private void UpdateNetworkStatus()
    {
        if (device == null)
        {
            SetNetworkStatus("", iconDisconnected, gadaColor, NetworkSoundState.Disconnected);
            return;
        }

        if (!device.isConnected)
        {
            SetNetworkStatus("", iconDisconnected, gadaColor, NetworkSoundState.Disconnected);
            return;
        }

        if (!device.ipConfigValid)
        {
            SetNetworkStatus("Unidentified network", iconWarning, normalColor, NetworkSoundState.Warning);
            return;
        }

        string targetAddress = GetTargetNanoAddress();

        WirelessDevice wireless = WirelessDevice.FindNanoByIP(targetAddress);

        if (wireless == null)
        {
            SetNetworkStatus("LAN connected", iconLanConnected, normalColor, NetworkSoundState.LanConnected);
            return;
        }

        if (!wireless.physicalLinkActive)
        {
            SetNetworkStatus("LAN connected", iconLanConnected, normalColor, NetworkSoundState.LanConnected);
            return;
        }

        if (!wireless.ipConfigValid)
        {
            SetNetworkStatus("Unidentified network", iconWarning, normalColor, NetworkSoundState.Warning);
            return;
        }

        if (!TryParseIP(device.ipAddress, out uint computerIP))
        {
            SetNetworkStatus("Unidentified network", iconWarning, normalColor, NetworkSoundState.Warning);
            return;
        }

        if (!TryParseIP(device.netmask, out uint computerMask))
        {
            SetNetworkStatus("Unidentified network", iconWarning, normalColor, NetworkSoundState.Warning);
            return;
        }

        if (!TryParseIP(wireless.ipAddress, out uint nanoIP))
        {
            SetNetworkStatus("Unidentified network", iconWarning, normalColor, NetworkSoundState.Warning);
            return;
        }

        if (!IsSameSubnet(computerIP, nanoIP, computerMask))
        {
            SetNetworkStatus("Unidentified network", iconWarning, normalColor, NetworkSoundState.Warning);
            return;
        }

        SetNetworkStatus("Connected", iconConnected, connectedColor, NetworkSoundState.Connected);
    }

    private void SetNetworkStatus(string message, Sprite icon, Color color, NetworkSoundState state = NetworkSoundState.Unknown)
    {
        PlayNetworkStateSound(state);
        if (connectionStatusText != null)
        {
            connectionStatusText.text = message;
            connectionStatusText.color = color;
        }

        if (connectionIcon != null)
        {
            connectionIcon.sprite = icon;
            connectionIcon.color = color;
        }
    }

    private string GetTargetNanoAddress()
    {
        if (addressInput != null && !string.IsNullOrWhiteSpace(addressInput.text))
        {
            return CleanAddress(addressInput.text);
        }

        if (pingTargetInput != null && !string.IsNullOrWhiteSpace(pingTargetInput.text))
        {
            return CleanAddress(pingTargetInput.text);
        }

        return defaultNanoIP;
    }

    // ================= IP CONFIG =================

    private void LoadIPConfigFromDevice()
    {
        if (device == null) return;

        if (ipInput != null)
        {
            ipInput.text = device.ipAddress;
        }

        if (gatewayInput != null)
        {
            gatewayInput.text = device.gateway;
        }

        if (netmaskInput != null)
        {
            netmaskInput.text = device.netmask;
        }

        if (dnsInput != null)
        {
            dnsInput.text = device.dns;
        }
    }

    public void OnClickApplyIPConfig()
    {
        if (device == null)
        {
            PlayUISound(errorClip);
            Debug.LogWarning("ComputerDevice belum dipasang.");
            return;
        }

        if (ipInput != null)
        {
            device.SetIP(ipInput.text);
        }

        if (gatewayInput != null)
        {
            device.SetGateway(gatewayInput.text);
        }

        if (netmaskInput != null)
        {
            device.SetNetmask(netmaskInput.text);
        }

        if (dnsInput != null)
        {
            device.SetDNS(dnsInput.text);
        }

        Debug.Log("IP Config Applied");

        PlayUISound(applyClip);
        PlayUISound(successClip);

        UpdateNetworkStatus();
    }

    public void OnClickCancelIPConfig()
    {
        PlayUISound(cancelClip);

        LoadIPConfigFromDevice();
        ShowHome();
        UpdateNetworkStatus();
    }

    // ================= PING CMD STYLE =================

    public void OnClickPing()
    {
        PlayUISound(pingStartClip);

        UpdateNetworkStatus();

        if (pingCoroutine != null)
        {
            StopCoroutine(pingCoroutine);
        }

        pingCoroutine = StartCoroutine(PingRoutine());
    }

    private IEnumerator PingRoutine()
    {
        if (pingResultText == null) yield break;

        pingResultText.text = "";

        if (device == null)
        {
            PlayUISound(errorClip);
            AppendPingLine("ComputerDevice belum dipasang.");
            yield break;
        }

        string target = "";

        if (pingTargetInput != null)
        {
            target = CleanAddress(pingTargetInput.text);
        }

        if (string.IsNullOrWhiteSpace(target))
        {
            PlayUISound(errorClip);
            AppendPingLine("Masukkan alamat IP tujuan terlebih dahulu.");
            yield break;
        }

        if (!TryParseIP(target, out uint targetIP))
        {
            PlayUISound(errorClip);
            AppendPingLine("Ping request could not find host " + target + ".");
            AppendPingLine("Please check the name and try again.");
            yield break;
        }

        AppendPingLine("Pinging " + target + " with 32 bytes of data:");
        yield return new WaitForSeconds(pingLineDelay);

        bool success = CanPingTarget(targetIP, target, out string failMessage);

        int sent = 4;
        int received = 0;

        for (int i = 0; i < sent; i++)
        {
            if (success)
            {
                int time = Random.Range(1, 5);
                AppendPingLine("Reply from " + target + ": bytes=32 time<" + time + "ms TTL=64");
                PlayUISound(pingReplyClip);
                received++;
            }
            else
            {
                AppendPingLine(failMessage);
                PlayUISound(pingFailClip);
            }

            yield return new WaitForSeconds(pingLineDelay);
        }

        int lost = sent - received;
        int lossPercent = Mathf.RoundToInt((lost / (float)sent) * 100f);

        AppendPingLine("");
        AppendPingLine("Ping statistics for " + target + ":");
        AppendPingLine("    Packets: Sent = " + sent + ", Received = " + received + ", Lost = " + lost + " (" + lossPercent + "% loss),");

        if (success)
        {
            AppendPingLine("Approximate round trip times in milli-seconds:");
            AppendPingLine("    Minimum = 1ms, Maximum = 4ms, Average = 2ms");
        }

        UpdateNetworkStatus();
    }

    private bool CanPingTarget(uint targetIP, string targetText, out string failMessage)
    {
        failMessage = "Request timed out.";

        if (targetText == "127.0.0.1")
        {
            return true;
        }

        if (device == null)
        {
            failMessage = "PING: transmit failed. General failure.";
            return false;
        }

        if (!device.isConnected)
        {
            failMessage = "PING: transmit failed. General failure.";
            return false;
        }

        if (!device.ipConfigValid)
        {
            failMessage = "PING: transmit failed. General failure.";
            return false;
        }

        if (TryParseIP(device.ipAddress, out uint computerIP))
        {
            if (targetIP == computerIP)
            {
                return true;
            }
        }

        if (TryParseIP(device.gateway, out uint gatewayIP))
        {
            if (targetIP == gatewayIP)
            {
                if (device.gatewayReachable)
                {
                    return true;
                }

                failMessage = "Destination host unreachable.";
                return false;
            }
        }

        WirelessDevice wireless = WirelessDevice.FindNanoByIP(targetText);

        if (wireless != null)
        {
            if (!wireless.physicalLinkActive)
            {
                failMessage = "Request timed out.";
                return false;
            }

            if (!wireless.ipConfigValid)
            {
                failMessage = "Destination host unreachable.";
                return false;
            }

            if (!TryParseIP(device.ipAddress, out uint pcIP))
            {
                failMessage = "PING: transmit failed. General failure.";
                return false;
            }

            if (!TryParseIP(device.netmask, out uint pcMask))
            {
                failMessage = "PING: transmit failed. General failure.";
                return false;
            }

            if (!TryParseIP(wireless.ipAddress, out uint nanoIP))
            {
                failMessage = "Request timed out.";
                return false;
            }

            if (!IsSameSubnet(pcIP, nanoIP, pcMask))
            {
                failMessage = "Destination host unreachable.";
                return false;
            }

            return true;
        }

        if (device.hasInternet && !IsPrivateIP(targetIP))
        {
            return true;
        }

        failMessage = "Request timed out.";
        return false;
    }

    private void AppendPingLine(string line)
    {
        if (pingResultText == null) return;

        pingResultText.text += line + "\n";
    }

    // ================= BROWSER TO NANO =================

    public void OnClickOpenBrowserAddress()
    {
        PlayUISound(clickClip);

        HideBrowserResults();
        UpdateNetworkStatus();

        if (device == null)
        {
            ShowBrowserError("ComputerDevice belum dipasang.");
            return;
        }

        string address = "";

        if (addressInput != null)
        {
            address = CleanAddress(addressInput.text);
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            ShowBrowserError("Masukkan alamat IP terlebih dahulu.");
            return;
        }

        if (!TryParseIP(address, out uint targetIP))
        {
            ShowBrowserError("Alamat tidak valid. Gunakan format IP seperti 192.168.1.20.");
            return;
        }

        if (!device.isConnected)
        {
            ShowBrowserError("LAN cable is unplugged.");
            return;
        }

        if (!device.ipConfigValid)
        {
            ShowBrowserError("Konfigurasi IP komputer belum valid.");
            return;
        }

        WirelessDevice wireless = WirelessDevice.FindNanoByIP(address);

        if (wireless == null)
        {
            ShowBrowserError("Perangkat dengan alamat IP " + address + " tidak ditemukan.");
            return;
        }

        if (!wireless.physicalLinkActive)
        {
            ShowBrowserError("NanoStation belum tersambung ke jalur LAN / PoE.");
            return;
        }

        if (!wireless.ipConfigValid)
        {
            ShowBrowserError("Konfigurasi IP NanoStation belum valid.");
            return;
        }

        if (!TryParseIP(device.ipAddress, out uint computerIP))
        {
            ShowBrowserError("IP komputer tidak valid.");
            return;
        }

        if (!TryParseIP(device.netmask, out uint computerMask))
        {
            ShowBrowserError("Subnet mask komputer tidak valid.");
            return;
        }

        if (!TryParseIP(wireless.ipAddress, out uint nanoIP))
        {
            ShowBrowserError("IP NanoStation tidak valid.");
            return;
        }

        if (!IsSameSubnet(computerIP, nanoIP, computerMask))
        {
            ShowBrowserError("Komputer dan NanoStation tidak berada pada subnet yang sama.");
            return;
        }

        OpenNanoUI(address, wireless);
        UpdateNetworkStatus();
    }

    private void HideBrowserResults()
    {
        if (browserContentPanel != null)
        {
            browserContentPanel.SetActive(false);
        }

        if (nanoUICanvas != null)
        {
            nanoUICanvas.SetActive(false);
        }

        if (errorCanvas != null)
        {
            errorCanvas.SetActive(false);
        }

        if (browserResultText != null)
        {
            browserResultText.text = "";
        }

        if (errorText != null)
        {
            errorText.text = "";
        }
    }

    private void OpenNanoUI(string address, WirelessDevice targetNano)
    {
        PlayUISound(browserOpenClip);
        PlayUISound(successClip);

        if (browserResultText != null)
        {
            browserResultText.text = "Halaman berhasil dibuka: " + address;
            browserResultText.color = connectedColor;
        }

        if (browserContentPanel != null)
        {
            browserContentPanel.SetActive(false);
        }

        if (errorCanvas != null)
        {
            errorCanvas.SetActive(false);
        }

        if (nanoUICanvas != null)
        {
            nanoUICanvas.SetActive(true);
        }

        if (nanoUIController != null)
        {
            nanoUIController.OpenFromComputer(targetNano);
        }
        else
        {
            Debug.LogWarning("Nano UI Controller belum diisi di Inspector ComputerHomeUI.");
        }
    }

    private void ShowBrowserError(string message)
    {
        PlayUISound(errorClip);

        if (browserResultText != null)
        {
            browserResultText.text = message;
            browserResultText.color = disconnectedColor;
        }

        if (nanoUICanvas != null)
        {
            nanoUICanvas.SetActive(false);
        }

        if (browserContentPanel != null)
        {
            browserContentPanel.SetActive(false);
        }

        if (errorCanvas != null)
        {
            errorCanvas.SetActive(true);
        }

        if (errorText != null)
        {
            errorText.text = message;
        }

        UpdateNetworkStatus();
    }

    // ================= AUDIO HELPER =================

    private void InitAudio()
    {
        if (audioSource == null && autoFindAudioSource)
        {
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }
    }

    private void RegisterButtonSounds()
    {
        if (!autoRegisterButtonSound) return;

        Button[] childButtons = GetComponentsInChildren<Button>(true);

        for (int i = 0; i < childButtons.Length; i++)
        {
            RegisterButtonSound(childButtons[i]);
        }

        if (extraButtons != null)
        {
            for (int i = 0; i < extraButtons.Length; i++)
            {
                RegisterButtonSound(extraButtons[i]);
            }
        }
    }

    private void RegisterButtonSound(Button button)
    {
        if (button == null) return;
        if (registeredSoundButtons.Contains(button)) return;

        registeredSoundButtons.Add(button);

        EventTrigger trigger = button.GetComponent<EventTrigger>();

        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry hoverEntry = new EventTrigger.Entry();
        hoverEntry.eventID = EventTriggerType.PointerEnter;
        hoverEntry.callback.AddListener((data) =>
        {
            if (button.interactable)
            {
                PlayUISound(hoverClip);
            }
        });

        EventTrigger.Entry clickEntry = new EventTrigger.Entry();
        clickEntry.eventID = EventTriggerType.PointerClick;
        clickEntry.callback.AddListener((data) =>
        {
            if (button.interactable)
            {
                PlayUISound(clickClip);
            }
        });

        trigger.triggers.Add(hoverEntry);
        trigger.triggers.Add(clickEntry);
    }

    private void PlayUISound(AudioClip clip, float volumeScale = 1f)
    {
        if (!enableUISound) return;
        if (isInitializing) return;
        if (clip == null) return;
        if (audioSource == null) return;

        audioSource.PlayOneShot(clip, uiVolume * volumeScale);
    }

    private void PlayNetworkStateSound(NetworkSoundState state)
    {
        if (state == NetworkSoundState.Unknown) return;
        if (state == lastNetworkSoundState) return;

        lastNetworkSoundState = state;

        switch (state)
        {
            case NetworkSoundState.Connected:
                PlayUISound(connectedClip);
                break;

            case NetworkSoundState.Warning:
                PlayUISound(warningClip);
                break;

            case NetworkSoundState.LanConnected:
                PlayUISound(lanConnectedClip);
                break;

            case NetworkSoundState.Disconnected:
                PlayUISound(disconnectedClip);
                break;
        }
    }

    // ================= IP HELPER =================

    private string CleanAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return "";
        }

        address = address.Trim();
        address = address.Replace("http://", "");
        address = address.Replace("https://", "");

        int slashIndex = address.IndexOf('/');

        if (slashIndex >= 0)
        {
            address = address.Substring(0, slashIndex);
        }

        return address.Trim();
    }

    private bool TryParseIP(string ip, out uint result)
    {
        result = 0;

        if (string.IsNullOrWhiteSpace(ip))
        {
            return false;
        }

        string[] parts = ip.Trim().Split('.');

        if (parts.Length != 4)
        {
            return false;
        }

        for (int i = 0; i < 4; i++)
        {
            if (!int.TryParse(parts[i], out int value))
            {
                return false;
            }

            if (value < 0 || value > 255)
            {
                return false;
            }

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

    private bool IsPrivateIP(uint ip)
    {
        uint ip10Start = IPToUint(10, 0, 0, 0);
        uint ip10End = IPToUint(10, 255, 255, 255);

        uint ip172Start = IPToUint(172, 16, 0, 0);
        uint ip172End = IPToUint(172, 31, 255, 255);

        uint ip192Start = IPToUint(192, 168, 0, 0);
        uint ip192End = IPToUint(192, 168, 255, 255);

        return
            (ip >= ip10Start && ip <= ip10End) ||
            (ip >= ip172Start && ip <= ip172End) ||
            (ip >= ip192Start && ip <= ip192End);
    }

    private uint IPToUint(uint a, uint b, uint c, uint d)
    {
        return (a << 24) + (b << 16) + (c << 8) + d;
    }
}