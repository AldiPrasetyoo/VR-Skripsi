using UnityEngine;

public class NetworkPort : MonoBehaviour
{
    [SerializeField] private NetworkDevice parentDevice;
    [SerializeField] private NetworkPort connectedPort;

    public NetworkDevice ParentDevice => parentDevice;
    public NetworkPort ConnectedPort => connectedPort;

    private void Awake()
    {
        // Mencari NetworkDevice di object parent-nya
        parentDevice = GetComponentInParent<NetworkDevice>();
    }

    public void ConnectTo(NetworkPort other)
    {
        if (other == null) return;

        // Menyambungkan kedua port secara dua arah
        connectedPort = other;
        other.connectedPort = this;

        // Beritahu Manager untuk mengkalkulasi ulang sebaran Listrik & Internet
        if (InternetManager.Instance != null)
        {
            InternetManager.Instance.RefreshNetworkState();
        }
    }

    public void Disconnect()
    {
        if (connectedPort == null) return;

        NetworkPort temp = connectedPort;

        // Memutuskan sambungan kedua belah pihak
        connectedPort = null;
        temp.connectedPort = null;

        // Beritahu Manager bahwa ada kabel yang dicabut
        if (InternetManager.Instance != null)
        {
            InternetManager.Instance.RefreshNetworkState();
        }
    }
}