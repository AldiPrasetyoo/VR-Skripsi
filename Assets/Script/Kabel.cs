using UnityEngine;

public class Kabel : MonoBehaviour
{
    private JaringanKabel nodeA;
    private JaringanKabel nodeB;

    private Transform portA;
    private Transform portB;

    private LineRenderer line;

    void Start()
    {
        line = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (portA != null && portB != null)
        {
            line.positionCount = 2;
            line.SetPosition(0, portA.position);
            line.SetPosition(1, portB.position);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Port")) return;

        JaringanKabel node =
            other.GetComponentInParent<JaringanKabel>();

        if (node == null) return;

        if (nodeA == null)
        {
            nodeA = node;
            portA = other.transform;
            Debug.Log("Connected first end to " + node.name);
        }
        else if (nodeB == null && node != nodeA)
        {
            nodeB = node;
            portB = other.transform;

            nodeA.Connect(nodeB);
            Debug.Log("Cable fully connected!");
        }
    }
}