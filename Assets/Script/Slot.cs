using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;


public class Slot : MonoBehaviour
{

    public GameObject ItemInSlot;

    public Image slotImage;


    Color originalColor;


    private GameObject hoveringItem;



    private void Start()
    {

        if(slotImage == null)
            slotImage = GetComponentInChildren<Image>();


        originalColor = slotImage.color;

    }





    private void OnTriggerEnter(Collider other)
    {

        Item item = other.GetComponent<Item>();


        if(item == null)
            return;



        hoveringItem = other.gameObject;


        slotImage.color = Color.green;


    }





    private void OnTriggerExit(Collider other)
    {

        if(other.gameObject == hoveringItem)
        {

            hoveringItem = null;


            if(ItemInSlot == null)
                slotImage.color = originalColor;

        }


    }





    private void Update()
    {


        if(hoveringItem == null)
            return;



        XRGrabInteractable grab =
        hoveringItem.GetComponent<XRGrabInteractable>();


        if(grab == null)
            return;




        // ketika dilepas
        if(!grab.isSelected)
        {

            InsertItem(hoveringItem);

            hoveringItem = null;

        }


    }






    void InsertItem(GameObject obj)
    {


        if(ItemInSlot != null)
            return;



        Rigidbody rb =
        obj.GetComponent<Rigidbody>();


        if(rb != null)
            rb.isKinematic = true;





        obj.transform.SetParent(transform);



        obj.transform.localPosition =
        Vector3.zero;




        Item item =
        obj.GetComponent<Item>();



        obj.transform.localEulerAngles =
        item.slotRotation;



        item.inSlot = true;


        item.currentSlot = this;


        item.OnInventory();



        ItemInSlot = obj;



        slotImage.color =
        Color.gray;


    }








    public void RemoveItem()
    {

        ItemInSlot = null;


        slotImage.color =
        originalColor;


    }



}