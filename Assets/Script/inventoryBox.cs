using UnityEngine;


public class InventoryBox : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {

        InventoryItem item =
        other.GetComponent<InventoryItem>();


        if(item == null)
            return;



        VRInventoryManager.Instance.AddItem(item);



        // jangan destroy
        item.HideItem();


    }

}