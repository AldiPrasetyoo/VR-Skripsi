using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace HPhysic
{
    public class TowerDeviceXR : MonoBehaviour
    {
        private XRGrabInteractable grab;
        private Rigidbody rb;

        private TowerMountSlot currentSlot;
        private TowerMountSlot nearestSlot;

        private GameObject ghost;

        private bool isGrabbed;

        public float rotateSpeed = 90f;

        void Awake()
        {
            grab = GetComponent<XRGrabInteractable>();
            rb = GetComponent<Rigidbody>();

            grab.selectEntered.AddListener(OnGrab);
            grab.selectExited.AddListener(OnRelease);
        }

        void Update()
        {
            // Jika device sudah terpasang dan sedang dipegang
            if (currentSlot != null && isGrabbed)
            {
                RotateDevice();
            }
        }

        // =========================
        // ROTATE DEVICE
        // =========================

        void RotateDevice()
        {
            InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

            if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axis))
            {
                float rotate = axis.x * rotateSpeed * Time.deltaTime;

                transform.Rotate(Vector3.up, rotate);
            }
        }

        // =========================
        // SAAT DIAMBIL PLAYER
        // =========================

        void OnGrab(SelectEnterEventArgs args)
        {
            isGrabbed = true;

            if (currentSlot != null)
            {
                currentSlot.Detach();
                currentSlot = null;
            }

            transform.SetParent(null);

            rb.isKinematic = false;
            rb.useGravity = false;
        }

        // =========================
        // SAAT DILEPAS
        // =========================

        void OnRelease(SelectExitEventArgs args)
        {
            isGrabbed = false;

            TowerMountSlot[] slots = FindObjectsByType<TowerMountSlot>(FindObjectsSortMode.None);

            foreach (TowerMountSlot slot in slots)
            {
                if (slot.occupied)
                    continue;

                if (slot.IsInsideGhostArea(transform.position))
                {
                    slot.AttachDevice(gameObject);
                    currentSlot = slot;

                    if (ghost != null)
                    {
                        Destroy(ghost);
                        ghost = null;
                    }

                    return;
                }
            }

            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // =========================
        // DETEKSI SLOT
        // =========================

        void OnTriggerStay(Collider other)
        {
            TowerMountSlot slot = other.GetComponent<TowerMountSlot>();

            if (slot == null) return;
            if (slot.occupied) return;

            nearestSlot = slot;

            slot.Highlight(true);

            ShowGhost(slot);
        }

        void OnTriggerExit(Collider other)
        {
            TowerMountSlot slot = other.GetComponent<TowerMountSlot>();

            if (slot != null)
                slot.Highlight(false);

            if (ghost != null)
                Destroy(ghost);

            if (slot == nearestSlot)
                nearestSlot = null;
        }

        // =========================
        // GHOST PREVIEW
        // =========================

        void ShowGhost(TowerMountSlot slot)
        {
            if (ghost != null) return;

            ghost = Instantiate(gameObject);

            Destroy(ghost.GetComponent<TowerDeviceXR>());
            Destroy(ghost.GetComponent<XRGrabInteractable>());
            Destroy(ghost.GetComponent<Rigidbody>());

            foreach (Collider c in ghost.GetComponentsInChildren<Collider>())
                Destroy(c);

            Renderer[] rends = ghost.GetComponentsInChildren<Renderer>();

            foreach (Renderer r in rends)
            {
                Color c = r.material.color;
                c.a = 0.3f;
                r.material.color = c;
            }

            ghost.transform.position = slot.ghostPoint.position;
            ghost.transform.rotation = slot.ghostPoint.rotation;
        }
    }
}