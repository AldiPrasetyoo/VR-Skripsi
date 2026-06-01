using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace HPhysic
{
    public class GeneralMount : MonoBehaviour
    {
        private XRGrabInteractable grab;
        private Rigidbody rb;

        private GeneralMountSlot currentSlot;
        private GeneralMountSlot nearestSlot;

        private GameObject ghost;

        private bool isGrabbed;

        [Header("Rotation")]
        public float rotateSpeed = 90f;

        private void Awake()
        {
            grab = GetComponent<XRGrabInteractable>();
            rb = GetComponent<Rigidbody>();

            if (grab != null)
            {
                grab.selectEntered.AddListener(OnGrab);
                grab.selectExited.AddListener(OnRelease);
            }
        }

        private void Update()
        {
            if (currentSlot != null && isGrabbed)
            {
                RotateDevice();
            }
        }

        private void RotateDevice()
        {
            InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

            if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axis))
            {
                float rotate = axis.x * rotateSpeed * Time.deltaTime;
                transform.Rotate(Vector3.up, rotate);
            }
        }

        private void OnGrab(SelectEnterEventArgs args)
        {
            isGrabbed = true;

            if (currentSlot != null)
            {
                currentSlot.Detach();
                currentSlot = null;
            }

            transform.SetParent(null);

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = false;
            }
        }

        private void OnRelease(SelectExitEventArgs args)
        {
            isGrabbed = false;

            if (nearestSlot != null)
            {
                nearestSlot.AttachDevice(gameObject);
                currentSlot = nearestSlot;

                if (ghost != null)
                {
                    Destroy(ghost);
                }
            }
            else
            {
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            GeneralMountSlot slot = other.GetComponent<GeneralMountSlot>();

            if (slot == null) return;
            if (slot.occupied) return;

            nearestSlot = slot;
            slot.Highlight(true);

            ShowGhost(slot);
        }

        private void OnTriggerExit(Collider other)
        {
            GeneralMountSlot slot = other.GetComponent<GeneralMountSlot>();

            if (slot != null)
            {
                slot.Highlight(false);
            }

            if (ghost != null)
            {
                Destroy(ghost);
            }

            if (slot == nearestSlot)
            {
                nearestSlot = null;
            }
        }

        private void ShowGhost(GeneralMountSlot slot)
        {
            if (ghost != null) return;
            if (slot.mountPoint == null) return;

            ghost = Instantiate(gameObject);

            GeneralMount ghostMount = ghost.GetComponent<GeneralMount>();
            if (ghostMount != null)
            {
                Destroy(ghostMount);
            }

            XRGrabInteractable ghostGrab = ghost.GetComponent<XRGrabInteractable>();
            if (ghostGrab != null)
            {
                Destroy(ghostGrab);
            }

            Rigidbody ghostRb = ghost.GetComponent<Rigidbody>();
            if (ghostRb != null)
            {
                Destroy(ghostRb);
            }

            foreach (Collider c in ghost.GetComponentsInChildren<Collider>())
            {
                Destroy(c);
            }

            foreach (Renderer r in ghost.GetComponentsInChildren<Renderer>())
            {
                Color color = r.material.color;
                color.a = 0.3f;
                r.material.color = color;
            }

            ghost.transform.position = slot.mountPoint.position;
            ghost.transform.rotation = slot.mountPoint.rotation;
        }
    }
}