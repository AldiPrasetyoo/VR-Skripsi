using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class CablePhysics : MonoBehaviour
{
    [Header("Connectors")]
    public Transform startConnector;
    public Transform endConnector;

    [Header("Cable Settings")]
    public int segments = 12;
    public float spacing = 0.08f;
    public float springForce = 250f;
    public float damper = 8f;

    [Header("Status")]
    public bool isConnected;

    private List<Rigidbody> points = new List<Rigidbody>();
    private LineRenderer line;

    private LanPort startPort;
    private LanPort endPort;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        GenerateCable();
    }

    void Update()
    {
        UpdateLine();
        CheckConnection();
    }

    void GenerateCable()
    {
        Vector3 dir = (endConnector.position - startConnector.position).normalized;
        Rigidbody previous = startConnector.GetComponent<Rigidbody>();

        for (int i = 0; i < segments; i++)
        {
            GameObject p = new GameObject("Point_" + i);
            p.transform.parent = transform;
            p.transform.position = startConnector.position + dir * spacing * i;

            Rigidbody rb = p.AddComponent<Rigidbody>();
            rb.mass = 0.05f;
            rb.linearDamping = 1f;

            SpringJoint joint = p.AddComponent<SpringJoint>();
            joint.connectedBody = previous;
            joint.spring = springForce;
            joint.damper = damper;
            joint.maxDistance = spacing;

            previous = rb;
            points.Add(rb);
        }

        SpringJoint endJoint = endConnector.gameObject.AddComponent<SpringJoint>();
        endJoint.connectedBody = previous;
        endJoint.spring = springForce;
        endJoint.damper = damper;
        endJoint.maxDistance = spacing;
    }

    void UpdateLine()
    {
        line.positionCount = points.Count + 2;
        line.SetPosition(0, startConnector.position);

        for (int i = 0; i < points.Count; i++)
            line.SetPosition(i + 1, points[i].position);

        line.SetPosition(points.Count + 1, endConnector.position);
    }

    void CheckConnection()
    {
        isConnected = (startPort != null && endPort != null);
    }

    public void AttachPort(LanPort port, Transform connector)
    {
        if (connector == startConnector)
            startPort = port;
        else if (connector == endConnector)
            endPort = port;
    }

    public void DetachPort(Transform connector)
    {
        if (connector == startConnector)
            startPort = null;
        else if (connector == endConnector)
            endPort = null;
    }
}