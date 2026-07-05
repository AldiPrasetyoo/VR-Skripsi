using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


public class CableInventoryHandler : MonoBehaviour
{

    [Header("Connector Grab")]
    public XRGrabInteractable startGrab;
    public XRGrabInteractable endGrab;



    [Header("Saat keluar inventory")]
    public XRGrabInteractable mainGrab;



    private Slot currentSlot;




    void Start()
    {

        startGrab.selectEntered.AddListener(OnGrab);

        endGrab.selectEntered.AddListener(OnGrab);


    }






    void OnGrab(SelectEnterEventArgs args)
    {


        // kalau kabel ada di inventory
        if(currentSlot != null)
        {

            currentSlot.RemoveItem();

            currentSlot = null;


        }



        transform.SetParent(null);



        // pastikan start yang aktif
        if(mainGrab != null)
        {

            startGrab.enabled = true;

            endGrab.enabled = false;


        }



    }






    public void SetSlot(Slot slot)
    {

        currentSlot = slot;

    }





}