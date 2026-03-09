using UnityEngine;
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

        void Awake()
        {
            grab = GetComponent<XRGrabInteractable>();
            rb = GetComponent<Rigidbody>();

            grab.selectEntered.AddListener(OnGrab);
            grab.selectExited.AddListener(OnRelease);
        }

        // =========================
        // SAAT DIAMBIL PLAYER
        // =========================

        void OnGrab(SelectEnterEventArgs args)
        {
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
            if (nearestSlot != null)
            {
                nearestSlot.AttachDevice(gameObject);
                currentSlot = nearestSlot;
                if (ghost != null)
                Destroy(ghost);
            }
            else
            {
             // dilepas di udara
            rb.isKinematic = false;
            rb.useGravity = true;
             }
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