using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using HPhysic;

public class LanGhostPreview : MonoBehaviour
{
    public Connector femaleConnector;

    [Header("Ghost Point")]
    public Transform ghostPoint;

    private GameObject ghost;
    private XRGrabInteractable grabbedCable;

    void Start()
    {
        if (femaleConnector == null)
            femaleConnector = GetComponent<Connector>();

        if (ghostPoint == null)
            ghostPoint = transform;
    }

    void Update()
    {
        grabbedCable = FindGrabbedCable();

        // Tidak ada kabel yang sedang dipegang
        if (grabbedCable == null)
        {
            DestroyGhost();
            return;
        }

        Connector male = grabbedCable.GetComponent<Connector>();

        if (male == null)
            return;

        if (femaleConnector.CanConnect(male))
        {
            ShowGhost(male);
        }
        else
        {
            DestroyGhost();
        }

        // Jika sudah terkoneksi, ghost hilang
        if (femaleConnector.IsConnected)
        {
            DestroyGhost();
        }
    }

    XRGrabInteractable FindGrabbedCable()
    {
        XRGrabInteractable[] all = FindObjectsByType<XRGrabInteractable>(FindObjectsSortMode.None);

        foreach (var item in all)
        {
            if (item.interactorsSelecting.Count == 0)
                continue;

            Connector c = item.GetComponent<Connector>();

            if (c == null)
                continue;

            if (c.ConnectionType != Connector.ConType.Male)
                continue;

            return item;
        }

        return null;
    }

    void ShowGhost(Connector male)
    {
        if (ghost != null)
            return;

        ghost = Instantiate(male.gameObject);
        ghost.name = "GhostPreview";

        Connector gc = ghost.GetComponent<Connector>();

        if (gc != null)
        {
            ghost.transform.rotation =
                ghostPoint.rotation * gc.RotationOffset;

            Vector3 offset =
                gc.ConnectionPosition - ghost.transform.position;

            ghost.transform.position =
                ghostPoint.position - offset;
        }

        // Nonaktifkan semua collider
        foreach (Collider c in ghost.GetComponentsInChildren<Collider>())
            c.enabled = false;

        // Rigidbody menjadi kinematic
        Rigidbody rb = ghost.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Nonaktifkan semua script kecuali Transform
        foreach (MonoBehaviour m in ghost.GetComponents<MonoBehaviour>())
        {
            if (m != this)
                m.enabled = false;
        }
    }

    void DestroyGhost()
    {
        if (ghost != null)
        {
            Destroy(ghost);
            ghost = null;
        }
    }
}