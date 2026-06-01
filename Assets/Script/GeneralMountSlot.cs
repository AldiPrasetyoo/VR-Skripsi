using UnityEngine;

namespace HPhysic
{
    public class GeneralMountSlot : MonoBehaviour
    {
        [Header("Slot Status")]
        public bool occupied;

        [Header("Mount Point")]
        public Transform mountPoint;

        private Renderer rend;
        private Color originalColor;

        private GameObject attachedDevice;

        private void Start()
        {
            rend = GetComponent<Renderer>();

            if (rend != null)
            {
                originalColor = rend.material.color;
            }

            if (mountPoint == null)
            {
                mountPoint = transform;
            }
        }

        public void Highlight(bool active)
        {
            if (rend == null) return;

            rend.material.color = active ? Color.green : originalColor;
        }

        public void AttachDevice(GameObject device)
        {
            if (device == null) return;

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