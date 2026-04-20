using UnityEngine;

namespace HPhysic
{
    public class TowerMountSlot : MonoBehaviour
    {
        public bool occupied;

        [Header("Ghost Position")]
        public Transform ghostPoint;

        private Renderer rend;
        private Color originalColor;

        private GameObject attachedDevice;

        void Start()
        {
            rend = GetComponent<Renderer>();

            if (rend != null)
                originalColor = rend.material.color;

            if (ghostPoint == null)
                ghostPoint = transform;
        }

        public void Highlight(bool active)
        {
            if (rend == null) return;

            rend.material.color = active ? Color.green : originalColor;
        }

        public void AttachDevice(GameObject device)
        {
            occupied = true;
            attachedDevice = device;

            device.transform.position = ghostPoint.position;
            device.transform.rotation = ghostPoint.rotation;

            Rigidbody rb = device.GetComponent<Rigidbody>();

            if (rb != null)
            {
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

            attachedDevice.transform.SetParent(null);

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