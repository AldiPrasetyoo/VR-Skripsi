using UnityEngine;
using UnityEngine.AI;

public class VehicleAi : MonoBehaviour
{
    public string state = "idle";

    [Header("AI")]
    public NavMeshAgent agent;
    public VehicleTarget currentTarget;

    [Header("Traffic Light Block")]
    public GameObject redLightBlockPrefab;
    private GameObject redLightBlock;

    [Header("Layer")]
    public int targetLayer = 13;
    public int stopLayer = 12;

    [Header("Distance")]
    public float targetSearchDistance = 400f;
    public float stopCheckDistance = 8f;
    public float arriveDistance = 1f;

    private void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (redLightBlockPrefab != null)
        {
            redLightBlock = Instantiate(
                redLightBlockPrefab,
                transform.position - (transform.forward * 4f),
                transform.rotation
            );

            redLightBlock.transform.SetParent(transform);
            redLightBlock.SetActive(false);
        }
        else
        {
            Debug.LogWarning("RedLightBlockPrefab belum diisi di Inspector.");
        }

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent tidak ditemukan di object: " + gameObject.name);
        }
    }

    private void Update()
    {
        if (agent == null) return;

        if (state == "idle")
        {
            FindFirstTarget();
        }
        else if (state == "moving")
        {
            MoveToNextTarget();
            CheckRedLightBlock();
        }
    }

    private void FindFirstTarget()
    {
        RaycastHit hit;
        int layerMask = 1 << targetLayer;

        Vector3 direction = GetStraightDirection();

        if (Physics.SphereCast(transform.position, 4f, direction, out hit, targetSearchDistance, layerMask))
        {
            VehicleTarget target = hit.transform.GetComponent<VehicleTarget>();

            if (target == null)
            {
                Debug.LogWarning("Object kena raycast tapi tidak punya VehicleTarget: " + hit.transform.name);
                return;
            }

            currentTarget = target;
            agent.SetDestination(currentTarget.transform.position);
            state = "moving";
        }
    }

    private void MoveToNextTarget()
    {
        if (currentTarget == null)
        {
            state = "idle";
            return;
        }

        if (agent.pathPending) return;

        if (agent.remainingDistance < arriveDistance)
        {
            if (currentTarget.nextTarget == null || currentTarget.nextTarget.Length == 0)
            {
                state = "idle";
                return;
            }

            int targetIndex = Random.Range(0, currentTarget.nextTarget.Length);
            VehicleTarget next = currentTarget.nextTarget[targetIndex];

            if (next == null)
            {
                Debug.LogWarning("Ada Next Target yang kosong di: " + currentTarget.name);
                state = "idle";
                return;
            }

            currentTarget = next;
            agent.SetDestination(currentTarget.transform.position);
        }
    }

    private void CheckRedLightBlock()
    {
        RaycastHit hit;
        int layerMask = 1 << stopLayer;

        if (Physics.Raycast(transform.position, transform.forward, out hit, stopCheckDistance, layerMask))
        {
            agent.isStopped = true;

            if (redLightBlock != null)
                redLightBlock.SetActive(true);
        }
        else
        {
            agent.isStopped = false;

            if (redLightBlock != null)
                redLightBlock.SetActive(false);
        }
    }

    private Vector3 GetStraightDirection()
    {
        Vector3 direction = transform.forward;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            direction = new Vector3(Mathf.Round(direction.x), 0f, 0f);
        }
        else
        {
            direction = new Vector3(0f, 0f, Mathf.Round(direction.z));
        }

        return direction.normalized;
    }
}