using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace HPhysic
{
    public class PhysicCable : MonoBehaviour
    {
        [Header("Look")]
        [SerializeField, Min(1)] private int numberOfPoints = 3;
        [SerializeField, Min(0.0001f)] private float space = 0.3f;
        [SerializeField, Min(0.0001f)] private float size = 0.3f;

        [Header("Behaviour")]
        [SerializeField, Min(1f)] private float springForce = 200;
        [SerializeField, Min(1f)] private float brakeLengthMultiplier = 2f;
        [SerializeField, Min(0.1f)] private float minBrakeTime = 1f;

        private float timeToBrake = 1f;

        [Header("Auto Length")]
        [SerializeField] private float autoExpandThreshold = 0.5f;
        [SerializeField] private float autoShrinkThreshold = 0.3f;
        [SerializeField] private float autoDelay = 0.15f;
        [SerializeField] private int maxPoints = 20;
        [SerializeField] private int minPoints = 2;

        [Header("Auto Reset")]
        [SerializeField] private int defaultPoints = 5;
        [SerializeField] private float shrinkInterval = 0.05f;

        private float autoTimer = 0f;
        private float shrinkTimer = 0f;

        private bool wasConnected = false;
        private bool forceShrink = false;

        [Header("Object to set")]
        [SerializeField, Required] private GameObject start;
        [SerializeField, Required] private GameObject end;
        [SerializeField, Required] private GameObject connector0;
        [SerializeField, Required] private GameObject point0;

        private List<Transform> points;
        private List<Transform> connectors;

        private const string cloneText = "Part";

        private Connector startConnector;
        private Connector endConnector;

        // ================= RESET =================
        [Button("Reset points")]
        private void UpdatePoints()
        {
            if (!start || !end || !point0 || !connector0)
            {
                Debug.LogWarning("Missing reference!");
                return;
            }

            // 🔥 FIX 1: HAPUS SEMUA SPRING LAMA
            foreach (var rb in GetComponentsInChildren<Rigidbody>())
            {
                var springs = rb.GetComponents<SpringJoint>();
                for (int i = 0; i < springs.Length; i++)
                {
                    DestroyImmediate(springs[i]);
                }
            }

            // Hapus clone lama
            int length = transform.childCount;
            for (int i = 0; i < length; i++)
            {
                if (transform.GetChild(i).name.StartsWith(cloneText))
                {
                    DestroyImmediate(transform.GetChild(i).gameObject);
                    length--;
                    i--;
                }
            }

            Vector3 lastPos = start.transform.position;
            Rigidbody lastBody = start.GetComponent<Rigidbody>();

            for (int i = 0; i < numberOfPoints; i++)
            {
                GameObject cConnector = (i == 0) ? connector0 : CreateNewCon(i);
                GameObject cPoint = (i == 0) ? point0 : CreateNewPoint(i);

                Vector3 newPos = lastPos + transform.forward * space;

                cPoint.transform.position = newPos;
                cPoint.transform.localScale = Vector3.one * size;
                cPoint.transform.rotation = transform.rotation;

                Rigidbody rb = cPoint.GetComponent<Rigidbody>();

                // 🔥 FIX 2: pastikan hanya 1 spring per point
                SpringJoint spring = cPoint.GetComponent<SpringJoint>();
                if (spring == null)
                    spring = cPoint.AddComponent<SpringJoint>();

                SetSpirng(spring, lastBody);

                lastBody = rb;

                cConnector.transform.position = CountConPos(lastPos, newPos);
                cConnector.transform.localScale = CountSizeOfCon(lastPos, newPos);
                cConnector.transform.rotation = CountRoationOfCon(lastPos, newPos);

                lastPos = newPos;
            }

            // 🔥 FIX 3: END selalu disambung dari point terakhir
            Vector3 endPos = lastPos + transform.forward * space;
            end.transform.position = endPos;

            Rigidbody endRB = end.GetComponent<Rigidbody>();

            SpringJoint endSpring = lastBody.gameObject.AddComponent<SpringJoint>();
            SetSpirng(endSpring, endRB);

            GameObject endConnectorObj = CreateNewCon(numberOfPoints);
            endConnectorObj.transform.position = CountConPos(lastPos, endPos);
            endConnectorObj.transform.rotation = CountRoationOfCon(lastPos, endPos);

            CollectPoints();
        }

        private void Start()
        {
            startConnector = start.GetComponent<Connector>();
            endConnector = end.GetComponent<Connector>();

            points = new List<Transform>();
            connectors = new List<Transform>();

            CollectPoints();
        }

        private void CollectPoints()
        {
            points.Clear();
            connectors.Clear();

            points.Add(start.transform);
            points.Add(point0.transform);

            connectors.Add(connector0.transform);

            for (int i = 1; i < numberOfPoints; i++)
            {
                Transform conn = GetConnector(i);
                Transform point = GetPoint(i);

                if (conn != null) connectors.Add(conn);
                if (point != null) points.Add(point);
            }

            Transform endConn = GetConnector(numberOfPoints);
            if (endConn != null) connectors.Add(endConn);

            points.Add(end.transform);
        }

        private void Update()
        {
            float cableLength = 0f;

            bool isConnected = startConnector.IsConnected || endConnector.IsConnected;

            if (isConnected)
                wasConnected = true;

            if (!isConnected && wasConnected)
            {
                forceShrink = true;
                wasConnected = false;
            }

            int numOfParts = connectors.Count;
            Transform lastPoint = points[0];

            for (int i = 0; i < numOfParts; i++)
            {
                if (i + 1 >= points.Count) break;

                Transform nextPoint = points[i + 1];
                Transform connector = connectors[i];

                if (nextPoint == null || connector == null) continue;

                connector.position = CountConPos(lastPoint.position, nextPoint.position);

                if (lastPoint.position != nextPoint.position)
                {
                    connector.rotation = Quaternion.LookRotation(nextPoint.position - connector.position);
                    connector.localScale = CountSizeOfCon(lastPoint.position, nextPoint.position);
                }
                else
                {
                    connector.localScale = Vector3.zero;
                }

                if (isConnected)
                    cableLength += Vector3.Distance(lastPoint.position, nextPoint.position);

                lastPoint = nextPoint;
            }

            if (isConnected)
            {
                float dynamicBrakeLength = space * numberOfPoints * brakeLengthMultiplier + 3f;

                if (cableLength > dynamicBrakeLength)
                {
                    timeToBrake -= Time.deltaTime;

                    if (timeToBrake < 0f)
                    {
                        startConnector.Disconnect();
                        endConnector.Disconnect();
                        timeToBrake = minBrakeTime;
                    }
                }
                else
                {
                    timeToBrake = minBrakeTime;
                }
            }

            if (forceShrink)
            {
                shrinkTimer -= Time.deltaTime;

                if (shrinkTimer <= 0f)
                {
                    if (numberOfPoints > defaultPoints)
                    {
                        numberOfPoints--;
                        UpdatePoints();
                    }
                    else
                    {
                        forceShrink = false;
                    }

                    shrinkTimer = shrinkInterval;
                }

                return;
            }
            else
            {
                shrinkTimer = shrinkInterval;
            }

            if (isConnected)
            {
                autoTimer -= Time.deltaTime;

                if (autoTimer <= 0f)
                {
                    float directDistance = Vector3.Distance(start.transform.position, end.transform.position);
                    float idealLength = space * numberOfPoints;

                    if (directDistance > idealLength + autoExpandThreshold && numberOfPoints < maxPoints)
                    {
                        numberOfPoints++;
                        UpdatePoints();
                        autoTimer = autoDelay;
                    }
                    else if (directDistance < idealLength - autoShrinkThreshold && numberOfPoints > minPoints)
                    {
                        numberOfPoints--;
                        UpdatePoints();
                        autoTimer = autoDelay;
                    }
                }
            }
        }

        private Vector3 CountConPos(Vector3 start, Vector3 end) => (start + end) / 2f;

        private Vector3 CountSizeOfCon(Vector3 start, Vector3 end) =>
            new Vector3(size, size, (start - end).magnitude / 2f);

        private Quaternion CountRoationOfCon(Vector3 start, Vector3 end) =>
            Quaternion.LookRotation(end - start, Vector3.right);

        private string ConnectorName(int index) => $"{cloneText}_{index}_Conn";
        private string PointName(int index) => $"{cloneText}_{index}_Point";

        private Transform GetConnector(int index) =>
            index > 0 ? transform.Find(ConnectorName(index)) : connector0.transform;

        private Transform GetPoint(int index) =>
            index > 0 ? transform.Find(PointName(index)) : point0.transform;

        public void SetSpirng(SpringJoint spring, Rigidbody connectedBody)
        {
            spring.connectedBody = connectedBody;
            spring.spring = springForce;
            spring.damper = 2f;

            spring.autoConfigureConnectedAnchor = false;
            spring.anchor = Vector3.zero;
            spring.connectedAnchor = Vector3.zero;

            spring.minDistance = space;
            spring.maxDistance = space;
        }

        private GameObject CreateNewPoint(int index)
        {
            GameObject temp = Instantiate(point0);
            temp.name = PointName(index);
            temp.transform.parent = transform;
            return temp;
        }

        private GameObject CreateNewCon(int index)
        {
            GameObject temp = Instantiate(connector0);
            temp.name = ConnectorName(index);
            temp.transform.parent = transform;
            return temp;
        }

        public Connector StartConnector => startConnector;
        public Connector EndConnector => endConnector;
        public IReadOnlyList<Transform> Points => points;
    }
}