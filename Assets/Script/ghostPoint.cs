using UnityEngine;
using HPhysic;

public class LanGhostPreview : MonoBehaviour
{
    public Connector femaleConnector;

    [Header("Ghost Point (Empty Object)")]
    public Transform ghostPoint;

    private GameObject ghost;

    [Header("Detection")]
    public float detectRadius = 0.15f;

    void Start()
    {
        if (femaleConnector == null)
            femaleConnector = GetComponent<Connector>();

        if (ghostPoint == null)
            ghostPoint = transform; // fallback
    }

    void Update()
    {
        if (femaleConnector == null) return;

        // kalau sudah connect → hapus ghost
        if (femaleConnector.IsConnected)
        {
            DestroyGhost();
            return;
        }

        Connector nearestMale = FindNearbyMale();

        if (nearestMale != null)
        {
            ShowGhost(nearestMale);
        }
        else
        {
            DestroyGhost();
        }
    }

    // =========================
    // DETEKSI MALE
    // =========================
    Connector FindNearbyMale()
    {
        Collider[] hits = Physics.OverlapSphere(
            ghostPoint.position, // 🔥 pakai ghost point
            detectRadius
        );

        foreach (var hit in hits)
        {
            Connector other = hit.GetComponent<Connector>();

            if (other == null) continue;
            if (other == femaleConnector) continue;

            if (other.ConnectionType != Connector.ConType.Male)
                continue;

            if (!femaleConnector.CanConnect(other))
                continue;

            return other;
        }

        return null;
    }

   // =========================
    // GHOST
    // =========================
    void ShowGhost(Connector male)
    {
        if (ghost != null) return;

        ghost = Instantiate(male.gameObject);

        // 🔥 Ambil script Connector dari ghost untuk menghitung offset-nya
        Connector ghostConnector = ghost.GetComponent<Connector>();

        if (ghostConnector != null)
        {
            // 1. Set Rotasi sesuai dengan offset rotasi bawaan dari ujung kabel tersebut
            ghost.transform.rotation = ghostPoint.rotation * ghostConnector.RotationOffset;

            // 2. Set Posisi (Penting: Lakukan setelah rotasi agar jarak offset ikut berputar)
            // Kurangi titik target dengan jarak antara root ghost ke connectionPoint-nya
            Vector3 positionOffset = ghostConnector.ConnectionPosition - ghost.transform.position;
            ghost.transform.position = ghostPoint.position - positionOffset;
        }
        else
        {
            // Fallback aman jika entah kenapa connector tidak terbaca
            ghost.transform.position = ghostPoint.position;
            ghost.transform.rotation = ghostPoint.rotation;
        }


        // 🔥 MATIKAN semua physics & interaction TANPA DELETE
        foreach (Collider c in ghost.GetComponentsInChildren<Collider>())
            c.enabled = false;

        Rigidbody rb = ghost.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // 🔥 disable script berbahaya (tetap bisa baca variabel)
        foreach (var comp in ghost.GetComponents<MonoBehaviour>())
        {
            comp.enabled = false;
        }

        // 🔥 transparan
        Renderer[] rends = ghost.GetComponentsInChildren<Renderer>();

        foreach (Renderer r in rends)
        {
            if (r.material.HasProperty("_Color"))
            {
                Color c = r.material.color;
                c.a = 0.3f;
                r.material.color = c;
            }
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

    // =========================
    // DEBUG
    // =========================
    void OnDrawGizmosSelected()
    {
        if (ghostPoint == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(ghostPoint.position, detectRadius);
    }
}