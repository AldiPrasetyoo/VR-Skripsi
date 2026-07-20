using System.Collections;
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

    // KUNCI OPTIMASI: Static cache agar 20+ objek tidak memindai Scene sendiri-sendiri
    private static XRGrabInteractable[] allInteractablesCache;
    private static float lastCacheTime = -1f;
    private const float cacheRefreshInterval = 1f; // Refresh daftar item setiap 1 detik

    void Start()
    {
        if (femaleConnector == null)
            femaleConnector = GetComponent<Connector>();

        if (ghostPoint == null)
            ghostPoint = transform;

        // Menggantikan Update() dengan Coroutine agar sangat ringan di CPU
        StartCoroutine(PreviewRoutine());
    }

    private IEnumerator PreviewRoutine()
    {
        // Looping berjalan aman di background
        while (true)
        {
            // Memberikan jeda 0.15 detik sebelum mengecek ulang (menghemat CPU hingga 90%)
            yield return new WaitForSeconds(0.15f);

            // Jika sudah terkoneksi, tidak perlu repot mencari ghost
            if (femaleConnector.IsConnected)
            {
                DestroyGhost();
                continue;
            }

            grabbedCable = FindGrabbedCable();

            // Tidak ada kabel yang sedang dipegang
            if (grabbedCable == null)
            {
                DestroyGhost();
                continue;
            }

            Connector male = grabbedCable.GetComponent<Connector>();

            if (male == null)
            {
                DestroyGhost();
                continue;
            }

            if (femaleConnector.CanConnect(male))
            {
                ShowGhost(male);
            }
            else
            {
                DestroyGhost();
            }
        }
    }

    XRGrabInteractable FindGrabbedCable()
    {
        // Memperbarui cache hanya 1 detik sekali. 
        // Jika satu port sudah mencari, port lain tinggal pakai data yang sama!
        if (Time.time - lastCacheTime > cacheRefreshInterval || allInteractablesCache == null)
        {
            allInteractablesCache = FindObjectsByType<XRGrabInteractable>(FindObjectsSortMode.None);
            lastCacheTime = Time.time;
        }

        foreach (var item in allInteractablesCache)
        {
            // Mencegah error jika ada item yang dihancurkan di tengah permainan
            if (item == null) 
                continue; 

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
            ghost.transform.rotation = ghostPoint.rotation * gc.RotationOffset;

            Vector3 offset = gc.ConnectionPosition - ghost.transform.position;
            ghost.transform.position = ghostPoint.position - offset;
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