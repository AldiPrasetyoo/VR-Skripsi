using UnityEngine;
using UnityEngine.UI;


public class InventoryUI : MonoBehaviour
{

    [Header("4 Slot Inventory")]
    public Image[] slots = new Image[4];



    public void Refresh()
    {

        // kosongkan semua
        for(int i = 0; i < slots.Length; i++)
        {

            slots[i].sprite = null;

            slots[i].enabled = false;

        }



        // isi item
        for(int i = 0; i < VRInventoryManager.Instance.items.Count; i++)
        {

            slots[i].sprite =
            VRInventoryManager.Instance.items[i].itemIcon;


            slots[i].enabled = true;

        }


    }


}