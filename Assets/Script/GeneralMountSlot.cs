using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace HPhysic
{
    public class GeneralMountSlot : MonoBehaviour
    {
        [Header("Slot Status")]
        public bool occupied;

        [Header("Mount Point (Posisi Asli)")]
        public Transform mountPoint;

        [Header("Ghost Point (Preview)")]
        public Transform ghostPoint;

        [Header("Detection")]
        public float detectRadius = 0.3f;

        [Header("VR Marker Fix")]
        [Tooltip("Sangat disarankan: Masukkan prefab Sphere yang sudah ada materialnya dan tanpa collider.")]
        public GameObject markerPrefab;
        
        [Tooltip("Opsional jika tidak pakai prefab: Masukkan material dari project yang support VR Single Pass.")]
        public Material vrReadyMaterial;

        private Renderer rend;
        private Color originalColor;

        private GameObject attachedDevice;

        private GameObject ghost;
        private GameObject marker;

        private Renderer markerRenderer;

        private XRGrabInteractable grabbedObject;

        private Color normalMarker = Color.cyan;

        // KUNCI OPTIMASI: Static cache agar puluhan slot tidak memindai Scene sendiri-sendiri
        private static XRGrabInteractable[] allInteractablesCache;
        private static float lastCacheTime = -1f;
        private const float cacheRefreshInterval = 1f;

        private void Start()
        {
            rend = GetComponent<Renderer>();

            if (rend != null)
                originalColor = rend.material.color;

            if (mountPoint == null)
            {
                GameObject mp = new GameObject("MountPoint");
                mp.transform.SetParent(transform, false); 
                mp.transform.localPosition = Vector3.zero;
                mp.transform.localRotation = Quaternion.identity;
                mountPoint = mp.transform;
            }

            if (ghostPoint == null)
            {
                GameObject gp = new GameObject("GhostPoint");
                gp.transform.SetParent(transform, false); 
                gp.transform.localPosition = Vector3.zero;
                gp.transform.localRotation = Quaternion.identity;
                ghostPoint = gp.transform;
            }

            // Ganti Update dengan Coroutine yang ringan
            StartCoroutine(MountRoutine());
        }

        private IEnumerator MountRoutine()
        {
            while (true)
            {
                // Jeda 0.15 detik per pengecekan, menghemat CPU drastis
                yield return new WaitForSeconds(0.15f);

                // OPTIMASI TAMBAHAN: Jika slot sudah terisi, tidak usah cari barang
                if (occupied)
                {
                    DestroyMarker();
                    DestroyGhost();
                    Highlight(false);
                    continue;
                }

                grabbedObject = FindGrabbedObject();

                if (grabbedObject == null)
                {
                    DestroyMarker();
                    DestroyGhost();
                    Highlight(false);
                    continue;
                }

                CreateMarker();

                float distance = Vector3.Distance(
                    grabbedObject.transform.position,
                    ghostPoint.position);

                if (distance <= detectRadius)
                {
                    ShowGhost(grabbedObject.gameObject);
                    Highlight(true);
                }
                else
                {
                    DestroyGhost();
                    Highlight(false);
                }
            }
        }

        XRGrabInteractable FindGrabbedObject()
        {
            // Memperbarui cache hanya 1 detik sekali
            if (Time.time - lastCacheTime > cacheRefreshInterval || allInteractablesCache == null)
            {
                allInteractablesCache = FindObjectsByType<XRGrabInteractable>(FindObjectsSortMode.None);
                lastCacheTime = Time.time;
            }

            foreach (var item in allInteractablesCache)
            {
                if (item == null)
                    continue;

                if (item.interactorsSelecting.Count == 0)
                    continue;

                // HANYA DEVICE GENERAL
                if (item.GetComponent<GeneralMount>() == null)
                    continue;

                return item;
            }

            return null;
        }

        void CreateMarker()
        {
            if (marker != null)
                return;

            if (markerPrefab != null)
            {
                marker = Instantiate(markerPrefab);
                marker.name = "MountMarker";
                marker.transform.SetParent(ghostPoint, false);
            }
            else
            {
                marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = "MountMarker";
                marker.transform.SetParent(ghostPoint, false);
                Destroy(marker.GetComponent<Collider>());
            }

            marker.transform.localPosition = new Vector3(0, 0, 0.1f);
            marker.transform.localScale = Vector3.one * 0.15f;

            markerRenderer = marker.GetComponent<Renderer>();

            if (markerPrefab == null && vrReadyMaterial != null && markerRenderer != null)
            {
                markerRenderer.material = vrReadyMaterial;
            }

            if (markerRenderer != null)
            {
                markerRenderer.material.color = normalMarker;
            }
        }

        void ShowGhost(GameObject obj)
        {
            if (ghost != null)
                return;

            ghost = Instantiate(obj);
            ghost.name = "GhostPreview";

            ghost.transform.position = ghostPoint.position;
            ghost.transform.rotation = ghostPoint.rotation;

            foreach (Collider c in ghost.GetComponentsInChildren<Collider>())
                c.enabled = false;

            Rigidbody rb = ghost.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            foreach (MonoBehaviour m in ghost.GetComponents<MonoBehaviour>())
                m.enabled = false;
        }

        void DestroyGhost()
        {
            if (ghost != null)
            {
                Destroy(ghost);
                ghost = null;
            }
        }

        void DestroyMarker()
        {
            if (marker != null)
            {
                Destroy(marker);
                marker = null;
                markerRenderer = null;
            }
        }

        public void Highlight(bool active)
        {
            if (rend == null)
                return;

            rend.material.color = active ? Color.green : originalColor;
        }

        public void AttachDevice(GameObject device)
        {
            if (device == null)
                return;

            occupied = true;
            attachedDevice = device;

            device.transform.position = mountPoint.position;
            device.transform.rotation = mountPoint.rotation;

            Rigidbody rb = device.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            DestroyGhost();
            DestroyMarker();
            Highlight(false);
        }

        public void Detach()
        {
            if (attachedDevice == null)
            {
                occupied = false;
                return;
            }

            Rigidbody rb = attachedDevice.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            attachedDevice = null;
            occupied = false;
        }

        public GameObject GetAttachedObject()
        {
            return attachedDevice;
        }
    }
}