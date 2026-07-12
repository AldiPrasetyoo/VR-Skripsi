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
            if (!isGrabbed)
                return;

            RotateDevice();

            HighlightNearestSlot();
        }

        private void RotateDevice()
        {
            InputDevice device =
                InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

            if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axis))
            {
                transform.Rotate(
                    Vector3.up,
                    axis.x * rotateSpeed * Time.deltaTime
                );
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

            GeneralMountSlot slot = FindNearestSlot();

            if (slot != null)
            {
                slot.AttachDevice(gameObject);
                currentSlot = slot;
            }
            else
            {
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;
                }
            }

            ResetAllHighlight();
        }

        private GeneralMountSlot FindNearestSlot()
        {
            GeneralMountSlot[] slots =
                FindObjectsByType<GeneralMountSlot>(FindObjectsSortMode.None);

            GeneralMountSlot nearest = null;

            float closest = Mathf.Infinity;

            foreach (GeneralMountSlot slot in slots)
            {
                if (slot.occupied)
                    continue;

                if (slot.ghostPoint == null)
                    continue;

                float dis =
                    Vector3.Distance(
                        transform.position,
                        slot.ghostPoint.position);

                if (dis <= slot.detectRadius && dis < closest)
                {
                    closest = dis;
                    nearest = slot;
                }
            }

            return nearest;
        }

        private void HighlightNearestSlot()
        {
            ResetAllHighlight();

            GeneralMountSlot slot = FindNearestSlot();

            if (slot != null)
            {
                slot.Highlight(true);
            }
        }

        private void ResetAllHighlight()
        {
            foreach (GeneralMountSlot slot in FindObjectsByType<GeneralMountSlot>(FindObjectsSortMode.None))
            {
                slot.Highlight(false);
            }
        }
    }
}