using UnityEngine;

public class TestConnect : MonoBehaviour
{
    public NetworkPort portA;
    public NetworkPort portB;

    void Start()
    {
        portA.ConnectTo(portB);
    }
}