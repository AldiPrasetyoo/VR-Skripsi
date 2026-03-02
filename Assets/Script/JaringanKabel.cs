using UnityEngine;
using System.Collections.Generic;

public class JaringanKabel : MonoBehaviour
{
    public List<JaringanKabel> connections =
        new List<JaringanKabel>();

    public bool hasInternet = false;

    public void Connect(JaringanKabel other)
    {
        if (!connections.Contains(other))
            connections.Add(other);

        if (!other.connections.Contains(this))
            other.connections.Add(this);

        Debug.Log(name + " connected to " + other.name);
    }
}