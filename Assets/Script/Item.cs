using UnityEngine;


public class Item : MonoBehaviour
{

    [Header("Item Data")]
    public string itemName;

    public Vector3 slotRotation;


    [Header("Scale Setting")]
    public float grabScale = 1f;

    public float inventoryScale = 1f;



    private Vector3 originalScale;



    [HideInInspector]
    public bool inSlot = false;



    [HideInInspector]
    public Slot currentSlot;



    private void Awake()
    {

        originalScale = transform.localScale;

    }



    public void OnGrab()
    {

        transform.localScale =
        originalScale * grabScale;

    }




    public void OnInventory()
    {

        transform.localScale =
        originalScale * inventoryScale;

    }




    public void OnPlacedWorld()
    {

        transform.localScale =
        originalScale;

    }



}