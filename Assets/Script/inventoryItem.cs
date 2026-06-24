using UnityEngine;


public class InventoryItem : MonoBehaviour
{

    [Header("Item Data")]
    public string itemName;

    public Sprite itemIcon;



    public void HideItem()
    {

        gameObject.SetActive(false);

    }



    public void ShowItem()
    {

        gameObject.SetActive(true);

    }


}