using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


public class InventorySlotGrab : MonoBehaviour
{

    public int slotIndex;


    private XRGrabInteractable grab;


    private XRInteractionManager interactionManager;



    private void Awake()
    {

        grab = GetComponent<XRGrabInteractable>();


        interactionManager =
        FindObjectOfType<XRInteractionManager>();

    }



    private void Start()
    {

        grab.selectEntered.AddListener(OnGrab);

    }




    void OnGrab(SelectEnterEventArgs args)
    {


        InventoryItem item =
        VRInventoryManager.Instance
        .TakeItem(slotIndex);



        if(item == null)
            return;




        // aktifkan item
        item.ShowItem();




        // posisi mengikuti tangan yang mengambil slot
        item.transform.position =
        args.interactorObject.transform.position;



        item.transform.rotation =
        args.interactorObject.transform.rotation;




        XRGrabInteractable itemGrab =
        item.GetComponent<XRGrabInteractable>();



        if(itemGrab == null)
            return;





        // ambil object langsung oleh tangan yang sama
        interactionManager.SelectEnter(
            args.interactorObject,
            itemGrab
        );


    }


}