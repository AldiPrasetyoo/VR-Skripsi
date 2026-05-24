using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Npc : MonoBehaviour
{
    NavMeshAgent agent;
    Animator animator;

    string state = "idle";

    PatrolPoint point;

    int direction = 0;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        animator.SetBool("isGrounded", true);

        direction = Random.Range(0, 2);

    }

    // Update is called once per frame
    void Update()
    {
        if (state == "idle")
        {
            animator.SetBool("isMoving", false);

            PatrolPoint[] points = (PatrolPoint[])GameObject.FindObjectsOfType(typeof(PatrolPoint));
            float distance = Mathf.Infinity;

            for(int i = 0; i < points.Length; i++)
            {
                Vector3 pointPos = points[i].transform.position;
                float tempDistance = Vector3.Distance(transform.position, pointPos);

                if (tempDistance < distance)
                {
                    distance = tempDistance;
                    point = points[i];
                }
            }
            if (point != null)
            {
                if (direction == 0)
                {
                    agent.SetDestination(point.next.transform.position);
                    point = point.next.GetComponent<PatrolPoint>();
                }
                else
                {
                    agent.SetDestination(point.prev.transform.position);
                    point = point.prev.GetComponent<PatrolPoint>();
                }
                
                state = "moving";
            }

        }
        if (state == "moving")
        {
            animator.SetBool("isMoving", true);
            if(!agent.pathPending && agent.remainingDistance < 1f)
            {
                if (direction == 0)
                {
                    agent.SetDestination(point.next.transform.position);
                    point = point.next.GetComponent<PatrolPoint>();
                }
                else
                {
                    agent.SetDestination(point.prev.transform.position);
                    point = point.prev.GetComponent<PatrolPoint>();
                }
            }
        }
    }
}
