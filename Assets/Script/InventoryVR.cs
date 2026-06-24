// Script name: InventoryVR

using UnityEngine;
using UnityEngine.InputSystem;


public class InventoryVR : MonoBehaviour
{

    [Header("Inventory UI")]
    public GameObject Inventory;


    [Header("Anchor Follow")]
    public Transform Anchor;


    [Header("XRI Controller Input")]
    public InputActionReference inventoryButton;



    private bool UIActive = false;



    private void Start()
    {

        Inventory.SetActive(false);


        if(inventoryButton != null)
            inventoryButton.action.Enable();

    }




    private void OnDisable()
    {

        if(inventoryButton != null)
            inventoryButton.action.Disable();

    }






    private void Update()
    {


        if(inventoryButton != null &&
           inventoryButton.action.WasPressedThisFrame())
        {

            ToggleInventory();

        }





        if(UIActive)
        {

            Inventory.transform.position =
            Anchor.position;


            Inventory.transform.rotation =
            Quaternion.Euler(
                Anchor.eulerAngles.x + 15,
                Anchor.eulerAngles.y,
                0
            );

        }



    }






    void ToggleInventory()
    {

        UIActive = !UIActive;


        Inventory.SetActive(UIActive);


    }


}