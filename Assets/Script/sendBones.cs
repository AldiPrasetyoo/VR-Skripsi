using UnityEngine;
using System.Collections.Generic;

public class TransferCloth : MonoBehaviour
{
    public SkinnedMeshRenderer clothes;
    public Transform playerRoot;


    void Start()
    {
        Dictionary<string, Transform> map = new();


        foreach(Transform t in playerRoot.GetComponentsInChildren<Transform>())
        {
            map[t.name] = t;
        }


        Transform[] oldBones = clothes.bones;

        Transform[] newBones = new Transform[oldBones.Length];


        for(int i = 0; i < oldBones.Length; i++)
        {
            if(map.ContainsKey(oldBones[i].name))
            {
                newBones[i] = map[oldBones[i].name];
            }
        }


        clothes.bones = newBones;
        clothes.rootBone = playerRoot;
    }
}