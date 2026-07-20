using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace HPhysic
{
    public class TowerMountSlot : MonoBehaviour
    {
        public bool occupied;

        [Header("Nomor Titik Tower")]
        public int nomorSlot = 2;

        [Header("Mount Point (Posisi Asli Device)")]
        public Transform mountPoint;

        [Header("Ghost Point (Preview)")]
        public Transform ghostPoint;

        [Header("Detection")]
        public float detectRadius = 0.3f;

        [Header("VR Marker Fix")]
        [Tooltip("Masukkan prefab Sphere yang sudah ada materialnya.")]
        public GameObject markerPrefab;
        
        [Tooltip("Opsional jika tidak pakai prefab.")]
        public Material vrReadyMaterial;

        private Renderer rend;
        private Color originalColor;
        private GameObject ghost;
        private GameObject marker;
        private Renderer markerRenderer;
        private XRGrabInteractable grabbedDevice;
        private Color normalMarker = Color.cyan;
        private GameObject attachedDevice;

        // KUNCI OPTIMASI: Static cache berbagi memori untuk semua TowerMountSlot
        private static XRGrabInteractable[] allInteractablesCache;
        private static float lastCacheTime = -1f;
        private const float cacheRefreshInterval = 1f;

        void Start()
        {
            rend = GetComponent<Renderer>();

            if (rend != null)
                originalColor = rend.material.color;

            if (mountPoint == null)
            {
                GameObject point = new GameObject("MountPoint");
                point.transform.SetParent(transform, false);
                point.transform.localPosition = Vector3.zero;
                point.transform.localRotation = Quaternion.identity;
                mountPoint = point.transform;
            }

            if (ghostPoint == null)
            {
                GameObject point = new GameObject("GhostPoint");
                point.transform.SetParent(transform, false);
                point.transform.localPosition = Vector3.zero;
                point.transform.localRotation = Quaternion.identity;
                ghostPoint = point.transform;
            }

            // Ganti Update() dengan Coroutine
            StartCoroutine(MountRoutine());
        }

        private IEnumerator MountRoutine()
        {
            while (true)
            {
                // Jeda 0.15 detik per frame untuk menghemat CPU
                yield return new WaitForSeconds(0.15f);

                if (occupied)
                {
                    DestroyGhost();
                    DestroyMarker();
                    Highlight(false);
                    continue; // Ganti return menjadi continue di dalam loop
                }

                grabbedDevice = FindGrabbedDevice();

                if (grabbedDevice == null)
                {
                    DestroyMarker();
                    DestroyGhost();
                    continue; // Ganti return menjadi continue
                }
                
                CreateMarker();

                float distance = Vector3.Distance(grabbedDevice.transform.position, ghostPoint.position);

                if (distance <= detectRadius)
                {
                    ShowGhost(grabbedDevice.gameObject);
                    Highlight(true);
                }
                else
                {
                    DestroyGhost();
                    Highlight(false);
                }
            }
        }

        XRGrabInteractable FindGrabbedDevice()
        {
            // Update cache maksimal 1 detik sekali
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

                // Hanya mendeteksi WirelessDevice
                WirelessDevice device = item.GetComponent<WirelessDevice>();

                if (device == null)
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
                marker = Instantiate(markerPrefab, ghostPoint);
                marker.name = "Tower_Marker";
            }
            else
            {
                marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = "Tower_Marker";
                marker.transform.SetParent(ghostPoint, false);
            }

            foreach (Collider c in marker.GetComponentsInChildren<Collider>())
            {
                Destroy(c);
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

        public bool IsInsideGhostArea(Vector3 position)
        {
            if (ghostPoint == null)
                return false;

            return Vector3.Distance(position, ghostPoint.position) <= detectRadius;
        }

        void ShowGhost(GameObject device)
        {
            if (ghost != null)
                return;

            ghost = Instantiate(device, ghostPoint);
            ghost.name = "Tower_GhostPreview";
            ghost.transform.localPosition = Vector3.zero;
            ghost.transform.localRotation = Quaternion.identity;

            foreach (Collider c in ghost.GetComponentsInChildren<Collider>())
            {
                c.enabled = false;
            }

            Rigidbody rb = ghost.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            foreach (MonoBehaviour m in ghost.GetComponents<MonoBehaviour>())
            {
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

            if (mountPoint != null)
            {
                device.transform.position = mountPoint.position;
                device.transform.rotation = mountPoint.rotation;
            }
            else
            {
                device.transform.position = transform.position;
                device.transform.rotation = transform.rotation;
            }

            Rigidbody rb = device.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            CekMisiPemasangan(device);
        }

        private SensorGedung CariSensorGedungDariParent()
        {
            Transform current = transform;

            while (current != null)
            {
                SensorGedung sensor = current.GetComponentInChildren<SensorGedung>(true);

                if (sensor != null)
                    return sensor;

                current = current.parent;
            }

            return null;
        }

        private void CekMisiPemasangan(GameObject device)
        {
            WirelessDevice wirelessDevice = device.GetComponent<WirelessDevice>();

            if (wirelessDevice == null)
                return;

            SensorGedung sensorGedung = CariSensorGedungDariParent();

            if (sensorGedung == null)
                return;

            if (ManagerMisi.Instance != null)
            {
                ManagerMisi.Instance.CekMisiPasangNanoKeTower(
                    wirelessDevice,
                    sensorGedung.idZonaGedung,
                    nomorSlot
                );
            }
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
    }
}