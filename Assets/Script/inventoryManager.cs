using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;



public class VRInventoryManager : MonoBehaviour
{


    public static VRInventoryManager Instance;



    [Header("Inventory UI")]
    public GameObject inventoryPanel;



    [Header("4 Slot Inventory")]
    public Image[] slots = new Image[4];



    [Header("Controller Button")]
    public InputActionReference openInventoryButton;




    public List<InventoryItem> items =
    new List<InventoryItem>();



    private bool isOpen = false;





    private void Awake()
    {

        Instance = this;

    }






    private void Start()
    {

        if(inventoryPanel != null)
            inventoryPanel.SetActive(false);



        Refresh();

    }







    private void OnEnable()
    {

        if(openInventoryButton != null)
            openInventoryButton.action.Enable();

    }





    private void OnDisable()
    {

        if(openInventoryButton != null)
            openInventoryButton.action.Disable();

    }







    private void Update()
    {


        if(openInventoryButton == null)
            return;



        if(openInventoryButton.action.WasPressedThisFrame())
        {

            ToggleInventory();

        }


    }







    void ToggleInventory()
    {

        isOpen = !isOpen;


        inventoryPanel.SetActive(isOpen);


    }









    public void AddItem(InventoryItem item)
    {


        if(items.Count >= 4)
        {

            Debug.Log("Inventory penuh");

            return;

        }




        items.Add(item);



        Refresh();



        Debug.Log(
            item.itemName +
            " masuk inventory"
        );


    }








    public InventoryItem TakeItem(int index)
    {


        if(index >= items.Count)
            return null;




        InventoryItem item =
        items[index];



        items.RemoveAt(index);



        Refresh();



        return item;


    }









    public void Refresh()
    {


        for(int i = 0; i < slots.Length; i++)
        {

            slots[i].sprite = null;

            slots[i].enabled = false;


        }






        for(int i = 0; i < items.Count; i++)
        {


            slots[i].sprite =
            items[i].itemIcon;



            slots[i].enabled = true;


        }



    }


}