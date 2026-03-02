using UnityEngine;

public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    WrongPassword,
    OutOfRange,
    WeakSignal
}

[RequireComponent(typeof(LineRenderer))]
public class StationDevice : MonoBehaviour
{
    [Header("Antenna Spec (Fixed)")]
    [SerializeField] public float beamAngle = 30f;   // tidak bisa diubah user
    [SerializeField] private float minRSSI = -75f;    // threshold connect

    private AccessPoint connectedAP;
    private LineRenderer line;

    public ConnectionState CurrentState { get; private set; }

    private void Start()
    {
        line = GetComponent<LineRenderer>();
        line.enabled = false;
        CurrentState = ConnectionState.Disconnected;
    }

    // =========================
    // CONNECT REQUEST
    // =========================
    public void ConnectToAP(AccessPoint ap, string inputPassword)
    {
        if (ap.password != inputPassword)
        {
            CurrentState = ConnectionState.WrongPassword;
            return;
        }

        connectedAP = ap;
        CurrentState = ConnectionState.Connecting;
    }

    // =========================
    // UPDATE REALTIME ALIGNMENT
    // =========================
    private void Update()
    {
        if (connectedAP == null) return;

        float distance = Vector3.Distance(transform.position, connectedAP.transform.position);

        if (distance > connectedAP.maxDistance)
        {
            CurrentState = ConnectionState.OutOfRange;
            line.enabled = false;
            return;
        }

        if (!IsFacing(connectedAP))
        {
            CurrentState = ConnectionState.Connecting; // masih aligning
            line.enabled = false;
            return;
        }

        float rssi = CalculateRSSI(distance, connectedAP);

        if (rssi < minRSSI)
        {
            CurrentState = ConnectionState.WeakSignal;
            line.enabled = false;
            return;
        }

        // SUCCESS
        CurrentState = ConnectionState.Connected;
        DrawBeam();
    }

    // =========================
    // DIRECTION CHECK
    // =========================
    bool IsFacing(AccessPoint ap)
    {
        Vector3 dir = (ap.transform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dir);
        return angle <= beamAngle;
    }

    // =========================
    // RSSI CALCULATION + OBSTACLE
    // =========================
    float CalculateRSSI(float distance, AccessPoint ap)
    {
        float fspl = 20 * Mathf.Log10(distance) +
                     20 * Mathf.Log10(ap.frequency) + 32.44f;

        float rssi = ap.txPower - fspl;

        rssi -= GetObstacleLoss(ap);

        return rssi;
    }

    float GetObstacleLoss(AccessPoint ap)
    {
        Vector3 dir = ap.transform.position - transform.position;
        float distance = dir.magnitude;

        Ray ray = new Ray(transform.position, dir.normalized);
        RaycastHit[] hits = Physics.RaycastAll(ray, distance);

        float loss = 0f;

        foreach (var hit in hits)
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
            {
                if (hit.collider.CompareTag("Wall"))
                    loss += 15f;
                else if (hit.collider.CompareTag("Glass"))
                    loss += 5f;
                else if (hit.collider.CompareTag("Tree"))
                    loss += 8f;
                else
                    loss += 10f;
            }
        }

        return loss;
    }

    void DrawBeam()
    {
        line.enabled = true;
        line.SetPosition(0, transform.position);
        line.SetPosition(1, connectedAP.transform.position);
    }

    // =========================
    // DISCONNECT
    // =========================
    public void Disconnect()
    {
        connectedAP = null;
        line.enabled = false;
        CurrentState = ConnectionState.Disconnected;
    }
}