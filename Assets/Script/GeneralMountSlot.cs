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

        private Renderer rend;
        private Color originalColor;

        private GameObject attachedDevice;

        private GameObject ghost;
        private GameObject marker;

        private Renderer markerRenderer;

        private XRGrabInteractable grabbedObject;

        private Color normalMarker = Color.cyan;

        private void Start()
        {
            rend = GetComponent<Renderer>();

            if (rend != null)
                originalColor = rend.material.color;

            if (mountPoint == null)
            {
                GameObject mp = new GameObject("MountPoint");
                mp.transform.SetParent(transform);
                mp.transform.localPosition = Vector3.zero;
                mp.transform.localRotation = Quaternion.identity;
                mountPoint = mp.transform;
            }

            if (ghostPoint == null)
            {
                GameObject gp = new GameObject("GhostPoint");
                gp.transform.SetParent(transform);
                gp.transform.localPosition = Vector3.zero;
                gp.transform.localRotation = Quaternion.identity;
                ghostPoint = gp.transform;
            }
        }

        private void Update()
        {
            grabbedObject = FindGrabbedObject();

            if (grabbedObject == null)
            {
                DestroyMarker();
                DestroyGhost();
                Highlight(false);
                return;
            }

            CreateMarker();

            float distance =
                Vector3.Distance(
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

        XRGrabInteractable FindGrabbedObject()
        {
            XRGrabInteractable[] all =
                FindObjectsOfType<XRGrabInteractable>();

            foreach (var item in all)
            {
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

            marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            marker.name = "MountMarker";

            marker.transform.SetParent(ghostPoint);

            marker.transform.localPosition =
                new Vector3(0, 0, 0.1f);

            marker.transform.localScale =
                Vector3.one * 0.15f;

            Destroy(marker.GetComponent<Collider>());

            markerRenderer = marker.GetComponent<Renderer>();

            markerRenderer.material.color = normalMarker;
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

            rend.material.color =
                active ? Color.green : originalColor;
        }

        public void AttachDevice(GameObject device)
        {
            if (device == null)
                return;

            occupied = true;
            attachedDevice = device;

            // SNAP KE MOUNT POINT (BUKAN GHOST POINT)
            device.transform.position =
                mountPoint.position;

            device.transform.rotation =
                mountPoint.rotation;

            Rigidbody rb =
                device.GetComponent<Rigidbody>();

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

            Rigidbody rb =
                attachedDevice.GetComponent<Rigidbody>();

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