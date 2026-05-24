using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    public GameObject[] prefabNpc;
    public GameObject[] points;
    public int npcCount = 20;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < npcCount; i++)
        {
            int n = Random.Range(0,prefabNpc.Length);
            int p = Random.Range(0,points.Length);
            Instantiate(prefabNpc[n], points[p].transform.position, Quaternion.identity);
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
