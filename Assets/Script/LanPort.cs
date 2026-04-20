using UnityEngine;

public class LanPort : MonoBehaviour
{
    public bool isOccupied;
    private CablePhysics connectedCable;

    void OnTriggerEnter(Collider other)
    {
        CablePhysics cable = other.GetComponentInParent<CablePhysics>();

        if (cable != null && !isOccupied)
        {
            isOccupied = true;
            connectedCable = cable;

            cable.AttachPort(this, other.transform);

            // snap connector ke port
            other.transform.position = transform.position;
            other.transform.rotation = transform.rotation;
        }
    }

    void OnTriggerExit(Collider other)
    {
        CablePhysics cable = other.GetComponentInParent<CablePhysics>();

        if (cable != null && cable == connectedCable)
        {
            isOccupied = false;
            cable.DetachPort(other.transform);
            connectedCable = null;
        }
    }
}