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

        private Renderer rend;
        private Color originalColor;
        private GameObject ghost;
        private GameObject marker;
        private Renderer markerRenderer;
        private XRGrabInteractable grabbedDevice;
        private Color normalMarker = Color.cyan;
        private GameObject attachedDevice;

        void Start()
        {
            rend = GetComponent<Renderer>();

            if (rend != null)
                originalColor = rend.material.color;

            // AUTO CREATE MOUNT POINT
            if (mountPoint == null)
            {
                GameObject point = new GameObject("MountPoint");
                point.transform.SetParent(transform);
                point.transform.localPosition = Vector3.zero;
                point.transform.localRotation = Quaternion.identity;
                mountPoint = point.transform;
            }

            // AUTO CREATE GHOST POINT
            if (ghostPoint == null)
            {
                GameObject point = new GameObject("GhostPoint");
                point.transform.SetParent(transform);
                point.transform.localPosition = Vector3.zero;
                point.transform.localRotation = Quaternion.identity;
                ghostPoint = point.transform;
            }
        }

        void Update()
        {
            grabbedDevice = FindGrabbedDevice();

            if (occupied)
            {
                DestroyGhost();
                DestroyMarker();
                Highlight(false);
                return;
            }

            if (grabbedDevice == null)
            {
                DestroyMarker();
                DestroyGhost();
                return;
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

        XRGrabInteractable FindGrabbedDevice()
        {
            XRGrabInteractable[] all = FindObjectsOfType<XRGrabInteractable>();

            foreach (var item in all)
            {
                if (item.interactorsSelecting.Count == 0)
                    continue;

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

            marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "Tower_Marker";
            marker.transform.SetParent(ghostPoint);
            marker.transform.localPosition = new Vector3(0, 0, 0.1f);
            marker.transform.localScale = Vector3.one * 0.15f;

            Destroy(marker.GetComponent<Collider>());

            markerRenderer = marker.GetComponent<Renderer>();
            markerRenderer.material.color = normalMarker;
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

            ghost = Instantiate(device);
            ghost.name = "Tower_GhostPreview";
            ghost.transform.position = ghostPoint.position;
            ghost.transform.rotation = ghostPoint.rotation;
            ghost.transform.SetParent(ghostPoint);

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