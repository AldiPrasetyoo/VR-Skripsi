using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace HPhysic
{
    public class BTSTower : MonoBehaviour
    {
        [Header("Tower Settings")]
        [SerializeField, Min(1)] private int numberOfSegments = 3;
        [SerializeField] private float segmentHeight = 0.5f;
        [SerializeField] private float segmentScale = 1f;

        [Header("Physics")]
        [SerializeField] private float springForce = 800f;
        [SerializeField] private float damperForce = 50f;
        [SerializeField] private float massPerSegment = 5f;

        [Header("References")]
        [SerializeField] private GameObject baseObject;
        [SerializeField] private GameObject segmentPrefab;
        [SerializeField] private GameObject topObject;

        [Header("Mount Slots")]
        [SerializeField] private Transform mountSlotsParent;

        [Header("Tower Colors")]
        [SerializeField] private Material whiteMaterial;
        [SerializeField] private Material redMaterial;

        private const string cloneName = "Tiang";

        private List<GameObject> segments = new();
        private List<TowerMountSlot> mountSlots = new();

        void Start()
        {
            ApplyColor(baseObject, whiteMaterial);
            LoadMountSlots();
        }

        // =========================
        // LOAD SLOTS
        // =========================

        void LoadMountSlots()
        {
            mountSlots.Clear();

            if (mountSlotsParent == null) return;

            TowerMountSlot[] slots = mountSlotsParent.GetComponentsInChildren<TowerMountSlot>();

            foreach (TowerMountSlot slot in slots)
            {
                mountSlots.Add(slot);
            }
        }

        // =========================
        // BUILD TOWER
        // =========================

        [Button("Build Tower")]
        private void BuildTower()
        {
            if (!baseObject || !segmentPrefab || !topObject)
            {
                Debug.LogWarning("Missing reference!");
                return;
            }

            ClearOldSegments();

            Rigidbody lastBody = baseObject.GetComponent<Rigidbody>();

            GameObject firstSegment = segmentPrefab;

            Rigidbody rb = GetOrAddRigidbody(firstSegment);
            rb.mass = massPerSegment;

            ClearAllJoints(firstSegment);

            SpringJoint firstJoint = firstSegment.AddComponent<SpringJoint>();
            SetupJoint(firstJoint, lastBody);

            SetSegmentColor(firstSegment, 0);

            segments.Add(firstSegment);

            Vector3 lastPosition = firstSegment.transform.position;
            lastBody = rb;

            for (int i = 1; i < numberOfSegments; i++)
            {
                GameObject segment = Instantiate(segmentPrefab, transform);
                segment.name = cloneName + "_" + i;

                Vector3 newPos = lastPosition + transform.up * segmentHeight;

                segment.transform.position = newPos;
                segment.transform.localScale = Vector3.one * segmentScale;

                Rigidbody newRB = GetOrAddRigidbody(segment);
                newRB.mass = massPerSegment;

                ClearAllJoints(segment);

                SpringJoint newJoint = segment.AddComponent<SpringJoint>();
                SetupJoint(newJoint, lastBody);

                SetSegmentColor(segment, i);

                segments.Add(segment);

                lastPosition = newPos;
                lastBody = newRB;
            }

            SetupTop(lastPosition, lastBody);
        }

        [Button("Add Segment")]
        public void AddSegment()
        {
            numberOfSegments++;
            BuildTower();
        }

        [Button("Remove Segment")]
        public void RemoveSegment()
        {
            if (numberOfSegments <= 1)
            {
                Debug.LogWarning("Tower must have at least 1 segment.");
                return;
            }

            numberOfSegments--;
            BuildTower();
        }

        // =========================
        // TOP SETUP
        // =========================

        private void SetupTop(Vector3 lastPosition, Rigidbody lastBody)
        {
            Vector3 topPos = lastPosition + transform.up * segmentHeight;
            topObject.transform.position = topPos;

            Rigidbody topRB = GetOrAddRigidbody(topObject);

            topRB.constraints =
                RigidbodyConstraints.FreezePositionX |
                RigidbodyConstraints.FreezePositionZ |
                RigidbodyConstraints.FreezeRotation;

            ClearAllJoints(topObject);

            SpringJoint topJoint = topObject.AddComponent<SpringJoint>();
            SetupJoint(topJoint, lastBody);

            SetSegmentColor(topObject, numberOfSegments);
        }

        // =========================
        // SNAP DEVICE
        // =========================

        public void SnapDeviceToNearestSlot(GameObject device)
        {
            if (mountSlots.Count == 0) return;

            TowerMountSlot nearest = null;
            float nearestDist = Mathf.Infinity;

            foreach (TowerMountSlot slot in mountSlots)
            {
                if (slot.occupied) continue;

                float dist = Vector3.Distance(device.transform.position, slot.transform.position);

                if (dist < nearestDist)
                {
                    nearest = slot;
                    nearestDist = dist;
                }
            }

            if (nearest != null)
            {
                nearest.AttachDevice(device);
            }
        }

        // =========================
        // UTIL
        // =========================

        private void SetSegmentColor(GameObject obj, int index)
        {
            Material mat = (index % 2 == 0) ? redMaterial : whiteMaterial;
            ApplyColor(obj, mat);
        }

        
private void ApplyColor(GameObject obj, Material mat)
{
    Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

    foreach (Renderer r in renderers)
    {
        // 🚫 Skip kalau layer Tangga
        if (r.gameObject.layer == LayerMask.NameToLayer("Tangga"))
            continue;

        r.sharedMaterial = mat;
    }
}

        private Rigidbody GetOrAddRigidbody(GameObject obj)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();

            if (rb == null)
                rb = obj.AddComponent<Rigidbody>();

            return rb;
        }

        private void SetupJoint(SpringJoint joint, Rigidbody connectedBody)
        {
            joint.connectedBody = connectedBody;
            joint.spring = springForce;
            joint.damper = damperForce;

            joint.autoConfigureConnectedAnchor = false;

            joint.anchor = Vector3.zero;
            joint.connectedAnchor = Vector3.zero;

            joint.minDistance = segmentHeight;
            joint.maxDistance = segmentHeight;
        }

        private void ClearAllJoints(GameObject obj)
        {
            SpringJoint[] joints = obj.GetComponents<SpringJoint>();

            foreach (SpringJoint j in joints)
            {
                DestroyImmediate(j);
            }
        }

        private void ClearOldSegments()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);

                if (child.name.StartsWith(cloneName))
                {
                    DestroyImmediate(child.gameObject);
                }
            }

            segments.Clear();
        }
    }
}