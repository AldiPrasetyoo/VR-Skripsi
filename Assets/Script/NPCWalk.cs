using UnityEngine;
using UnityEngine.AI;

public class RandomWalkNPC : MonoBehaviour
{
    [Header("Jalan Random")]
    public float walkRadius = 15f;
    public float waitTime = 2f;

    [Header("Animasi")]
    public Animator animator;
    public string speedParameter = "Speed";
    public bool useAnimator = false;

    private NavMeshAgent agent;
    private float waitTimer;
    private Vector3 startPosition;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;
        waitTimer = waitTime;

        if (agent != null)
        {
            agent.updateRotation = true;
            agent.updatePosition = true;
            MoveToRandomPoint();
        }
    }

    private void Update()
    {
        if (agent == null) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
            {
                MoveToRandomPoint();
                waitTimer = waitTime;
            }
        }

        if (useAnimator && animator != null)
        {
            animator.SetFloat(speedParameter, agent.velocity.magnitude);
        }
    }

    private void MoveToRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
        randomDirection += startPosition;
        randomDirection.y = transform.position.y;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomDirection, out hit, walkRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}