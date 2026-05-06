using UnityEngine;
using HPhysic;

public class BTSTowerUI : MonoBehaviour
{
    public BTSTower tower;

    public void OnAddButton()
    {
        tower.AddSegment();
    }

    public void OnRemoveButton()
    {
        tower.RemoveSegment();
    }
}