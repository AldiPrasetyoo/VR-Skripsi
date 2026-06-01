using System.Collections;
using UnityEngine;
using TMPro;

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

    private Coroutine pingCoroutine;

    private void Start()
    {
        ShowHome();
        LoadIPConfigFromDevice();

        if (pingTargetInput != null && string.IsNullOrWhiteSpace(pingTargetInput.text))
        {
            pingTargetInput.text = "192.168.1.20";
        }

        if (addressInput != null && string.IsNullOrWhiteSpace(addressInput.text))
        {
            addressInput.text = "192.168.1.20";
        }

        HideBrowserResults();
    }

    // ================= PANEL NAVIGATION =================

    public void ShowHome()
    {
        SetPanel(Header,true);
        SetPanel(homePanel, true);
        SetPanel(ipPanel, false);
        SetPanel(pingPanel, false);
        SetPanel(browserPanel, false);
    }

    public void ShowIPConfig()
    {
        SetPanel(Header,true);
        SetPanel(homePanel, false);
        SetPanel(ipPanel, true);
        SetPanel(pingPanel, false);
        SetPanel(browserPanel, false);

        LoadIPConfigFromDevice();
    }

    public void ShowPing()
    {
        SetPanel(Header,true);
        SetPanel(homePanel, false);
        SetPanel(ipPanel, false);
        SetPanel(pingPanel, true);
        SetPanel(browserPanel, false);
    }

    public void ShowBrowser()
    {
        SetPanel(Header,false);
        SetPanel(homePanel, false);
        SetPanel(ipPanel, false);
        SetPanel(pingPanel, false);
        SetPanel(browserPanel, true);

        HideBrowserResults();
    }

    private void SetPanel(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
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
    }

    public void OnClickCancelIPConfig()
    {
        LoadIPConfigFromDevice();
        ShowHome();
    }

    // ================= PING CMD STYLE =================

    public void OnClickPing()
    {
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
            AppendPingLine("Masukkan alamat IP tujuan terlebih dahulu.");
            yield break;
        }

        if (!TryParseIP(target, out uint targetIP))
        {
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
                received++;
            }
            else
            {
                AppendPingLine(failMessage);
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
        HideBrowserResults();

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
            ShowBrowserError("Tidak dapat membuka halaman. Kabel LAN komputer belum tersambung.");
            return;
        }

        if (!device.ipConfigValid)
        {
            ShowBrowserError("Tidak dapat membuka halaman. Konfigurasi IP komputer belum valid.");
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
            ShowBrowserError("NanoStation ditemukan, tetapi belum tersambung ke jalur LAN / PoE.");
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
        if (browserResultText != null)
        {
            browserResultText.text = "Halaman berhasil dibuka: " + address;
            browserResultText.color = Color.green;
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
        if (browserResultText != null)
        {
            browserResultText.text = message;
            browserResultText.color = Color.red;
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