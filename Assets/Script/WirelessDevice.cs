using UnityEngine;

[RequireComponent(typeof(NetworkDevice))]
public class WirelessDevice : MonoBehaviour
{
    public enum WirelessMode
    {
        None,
        AccessPoint,
        Station
    }

    [Header("Mode")]
    public WirelessMode mode = WirelessMode.None;

    private NetworkDevice networkDevice;
    private bool lastInternetState = false;

    [Header("Radio Settings")]
    [Range(0, 30)] public float outputPower = 10f;      // dBm
    [Range(0, 30)] public float antennaGain = 11f;      // dBi
    [Range(10, 180)] public float beamWidth = 60f;      // degree
    public float frequencyMHz = 2400f;
    public float channelWidth = 20f;

    [Header("AP Settings")]
    public string ssid = "";
    public string password = "";
    public int channel = 1;

    [Header("Runtime")]
    public WirelessDevice connectedAP;
    public float currentSignal;
    public float currentEIRP;

    const float DisconnectThreshold = -90f;
    const float MaxEIRP_Indonesia = 36f;

    #region Unity

    private void Awake()
    {
        networkDevice = GetComponent<NetworkDevice>();
    }

    private void OnEnable()
    {
        if (mode == WirelessMode.AccessPoint)
            NetworksRegistry.RegisterAP(this);
    }

    private void OnDisable()
    {
        NetworksRegistry.UnregisterAP(this);
    }

    private void Update()
    {
        currentEIRP = GetEIRP();

        if (mode != WirelessMode.Station)
            return;

        if (connectedAP == null)
        {
            SetInternet(false);
            return;
        }

        currentSignal = CalculateSignal(connectedAP);

        if (currentSignal < DisconnectThreshold)
        {
            Debug.Log($"🔴 {name} DISCONNECTED (Signal too weak: {currentSignal:F1} dBm)");
            Disconnect();
            SetInternet(false);
            return;
        }

        bool newInternetState = connectedAP.networkDevice.hasInternet;
        SetInternet(newInternetState);
    }

    #endregion

    #region Mode

 public void SetMode(int modeIndex)
{
    NetworksRegistry.UnregisterAP(this);

    mode = (WirelessMode)modeIndex;

    if (mode == WirelessMode.AccessPoint)
    {
        NetworksRegistry.RegisterAP(this);

        // Kalau device punya kabel internet
        if (networkDevice.hasInternet)
        {
            networkDevice.SetInternet(true);
        }
    }

    // Hanya reset koneksi wireless
    connectedAP = null;
}

    #endregion

    #region Connection

public void Connect(WirelessDevice ap, string inputPassword)
{
    if (mode != WirelessMode.Station) return;
    if (ap == null) return;
    if (ap.mode != WirelessMode.AccessPoint) return;
    if (ap.password != inputPassword) return;

    connectedAP = ap;

    Debug.Log($"📡 {name} connected to AP {ap.name}");

    // 🔥 langsung cek internet saat connect
    bool internetState = ap.GetComponent<NetworkDevice>().hasInternet;
    Debug.Log($"📶 Internet state from AP: {internetState}");
    SetInternet(internetState);
}

   public void Disconnect()
{
    connectedAP = null;

    // Hanya station yang kehilangan internet saat disconnect
    if (mode == WirelessMode.Station)
    {
        SetInternet(false);
    }
}

    #endregion

    #region Internet State

    private void SetInternet(bool state)
    {
        networkDevice.hasInternet = state;

        if (lastInternetState != state)
        {
            lastInternetState = state;

            if (state)
            {
                Debug.Log(
                    $"🟢 {name} GOT INTERNET via {connectedAP.name} | Signal: {currentSignal:F1} dBm"
                );
            }
            else
            {
                Debug.Log($"🔴 {name} LOST INTERNET");
            }
        }
    }

    #endregion

    #region Radio Calculation

    public float GetEIRP()
    {
        float eirp = outputPower + antennaGain;

        if (eirp > MaxEIRP_Indonesia)
        {
            outputPower -= (eirp - MaxEIRP_Indonesia);
            eirp = MaxEIRP_Indonesia;
        }

        return eirp;
    }

    float CalculateSignal(WirelessDevice ap)
    {
        float distance = Vector3.Distance(transform.position, ap.transform.position);

        if (distance < 0.1f)
            distance = 0.1f;

        float fspl =
            20f * Mathf.Log10(distance) +
            20f * Mathf.Log10(frequencyMHz) -
            27.55f;

        float txAngleLoss = ap.CalculateAngleLoss(transform);
        float rxAngleLoss = CalculateAngleLoss(ap.transform);

        float obstacleLoss = CalculateObstacleLoss(ap.transform, distance);

        float receivedPower =
            ap.GetEIRP()
            + antennaGain
            - fspl
            - txAngleLoss
            - rxAngleLoss
            - obstacleLoss;

        return receivedPower;
    }

    float CalculateObstacleLoss(Transform target, float distance)
    {
        float loss = 0f;

        RaycastHit[] hits = Physics.RaycastAll(
            transform.position,
            (target.position - transform.position).normalized,
            distance
        );

        foreach (var hit in hits)
        {
            ObstacleMaterial mat = hit.collider.GetComponent<ObstacleMaterial>();
            if (mat != null)
                loss += mat.GetLoss();
        }

        return loss;
    }

    float CalculateAngleLoss(Transform target)
    {
        Vector3 dir = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dir);

        if (angle <= beamWidth / 2f)
        {
            float ratio = angle / (beamWidth / 2f);
            return ratio * ratio * 10f;
        }

        return 25f;
    }

    #endregion
}