using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


public class ItemGrabScale : MonoBehaviour
{

    private Item item;

    private XRGrabInteractable grab;



    private void Awake()
    {

        item = GetComponent<Item>();

        grab = GetComponent<XRGrabInteractable>();

    }





    private void OnEnable()
    {

        grab.selectEntered.AddListener(OnGrab);

        grab.selectExited.AddListener(OnRelease);

    }





    private void OnDisable()
    {

        grab.selectEntered.RemoveListener(OnGrab);

        grab.selectExited.RemoveListener(OnRelease);

    }






    private void OnGrab(SelectEnterEventArgs args)
    {


        // kosongkan slot lama
        if(item.currentSlot != null)
        {

            item.currentSlot.RemoveItem();

            item.currentSlot = null;

        }




        item.inSlot = false;



        StartCoroutine(DelayGrabScale());


    }






    System.Collections.IEnumerator DelayGrabScale()
    {

        yield return null;


        item.OnGrab();


    }







    private void OnRelease(SelectExitEventArgs args)
    {


        // kalau masuk inventory jangan balik besar
        if(item.inSlot)
            return;




        item.OnPlacedWorld();


    }



}