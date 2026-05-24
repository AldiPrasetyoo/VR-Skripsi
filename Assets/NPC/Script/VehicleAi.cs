using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class VehicleAi : MonoBehaviour
{

    public string state = "idle";
    NavMeshAgent agent;
    VehicleTarget currentTarget;
    public GameObject redLightBlockPrefab;
    GameObject redLightBlock;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        redLightBlock = Instantiate(redLightBlockPrefab, transform.position - (transform.forward * 4), transform.rotation);
        redLightBlock.transform.parent = this.transform;

        agent = GetComponent<NavMeshAgent>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (state == "idle") {
            RaycastHit hit;
            int layerMask = 1 << 13;

            Vector3 direction = transform.forward;
            if(Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
            {
                direction = new Vector3(Mathf.Round(direction.x), 0, 0);
            }
            else
            {
                direction = new Vector3(0, 0, Mathf.Round(direction.z));
            }

            if (Physics.SphereCast(transform.position, 4, direction, out hit, 400,layerMask)) {
                
                currentTarget = hit.transform.GetComponent<VehicleTarget>();
                agent.SetDestination(hit.transform.position);
                state = "moving";
            }
        }
        if(state == "moving")
        {
            if(agent.remainingDistance < 1f && !agent.pathPending)
            {
                int nextLength = currentTarget.nextTarget.Length;
                if (nextLength > 0)
                {
                    int targetIndex = Random.Range(0, nextLength);
                    currentTarget = currentTarget.nextTarget[targetIndex];
                    agent.SetDestination(currentTarget.transform.position);
                }
                else {
                    state = "idle";        
                }
            }

            RaycastHit hit2;
            int layerMask = 1 << 12;
            if (Physics.Raycast(transform.position, transform.forward, out hit2, 8, layerMask)) { 
                agent.isStopped = true;
                redLightBlock.active = true;
            }
            else
            {
                agent.isStopped = false;
                redLightBlock.active = false;
            }
        }
    }
}
