using UnityEngine;

namespace HPhysic
{
    public class TowerMountSlot : MonoBehaviour
    {
        public bool occupied;

        [Header("Nomor Titik Tower")]
        public int nomorSlot = 2;

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
            if (device == null) return;

            occupied = true;
            attachedDevice = device;

            // Snap ke posisi ghost tanpa dijadikan child
            device.transform.position = ghostPoint.position;
            device.transform.rotation = ghostPoint.rotation;

            Rigidbody rb = device.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            // Cek misi setelah device berhasil dipasang
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
            {
                Debug.LogWarning("Object yang dipasang bukan WirelessDevice.");
                return;
            }

            SensorGedung sensorGedung = CariSensorGedungDariParent();

            if (sensorGedung == null)
            {
                Debug.LogWarning("TowerMountSlot tidak menemukan SensorGedung di area gedung.");
                return;
            }

            if (ManagerMisi.Instance != null)
            {
                ManagerMisi.Instance.CekMisiPasangNanoKeTower(
                    wirelessDevice,
                    sensorGedung.idZonaGedung,
                    nomorSlot
                );
            }
            else
            {
                Debug.LogWarning("ManagerMisi.Instance tidak ditemukan.");
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