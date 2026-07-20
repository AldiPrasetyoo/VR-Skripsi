using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class Slot : MonoBehaviour
{
    public GameObject ItemInSlot;
    public Image slotImage;

    private Color originalColor;
    private GameObject hoveringItem;

    private void Start()
    {
        if (slotImage == null)
            slotImage = GetComponentInChildren<Image>();

        originalColor = slotImage.color;
    }

    private void OnTriggerEnter(Collider other)
    {
        Item item = other.GetComponent<Item>();

        if (item == null)
            return;

        hoveringItem = other.gameObject;

        slotImage.color = Color.white;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == hoveringItem)
        {
            hoveringItem = null;

            if (ItemInSlot == null)
                slotImage.color = originalColor;
        }
    }

private void Update()
{
    // ==========================================
    // 1. LOGIKA MENGELUARKAN ITEM
    // Cek langsung pada item yang sudah ada di dalam slot
    // ==========================================
    if (ItemInSlot != null)
    {
        XRGrabInteractable grabInSlot = ItemInSlot.GetComponent<XRGrabInteractable>();
        
        // Jika item yang ada di slot sedang di-grab oleh player
        if (grabInSlot != null && grabInSlot.isSelected)
        {
            // Pastikan tidak bentrok dengan logika hovering
            if (hoveringItem == ItemInSlot)
            {
                hoveringItem = null;
            }

            RemoveItem();
            Debug.Log("Item keluar dari inventory karena di-grab");
        }
    }

    // ==========================================
    // 2. LOGIKA MEMASUKKAN ITEM BARU
    // Cek pada item yang sedang menyentuh slot (hovering)
    // ==========================================
    if (hoveringItem == null)
        return;

    XRGrabInteractable grab = hoveringItem.GetComponent<XRGrabInteractable>();
    if (grab == null)
        return;

    Item item = hoveringItem.GetComponent<Item>();
    if (item == null)
        return;

    // Belum pernah di-grab, jangan boleh masuk inventory
    if (!item.canEnterInventory)
        return;

    // Masih dipegang, tunggu dilepas
    if (grab.isSelected)
        return;

    // Masukkan ke slot
    InsertItem(hoveringItem);

    item.ResetInventoryPermission();
    hoveringItem = null;
}

    void InsertItem(GameObject obj)
    {
        if (ItemInSlot != null)
            return;

        Rigidbody rb =
            obj.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        obj.transform.SetParent(transform);
        obj.transform.localPosition = Vector3.zero;

        Item item =
            obj.GetComponent<Item>();

        obj.transform.localEulerAngles =
            item.slotRotation;

        item.inSlot = true;
        item.currentSlot = this;

        // Reset izin inventory
        item.ResetInventoryPermission();

        CableInventoryHandler cable =
            obj.GetComponent<CableInventoryHandler>();

        if (cable != null)
        {
            cable.SetSlot(this);
        }

        item.OnInventory();

        ItemInSlot = obj;

        slotImage.color = Color.gray;
    }

    public void RemoveItem()
    {
        if (ItemInSlot != null)
        {

            GameObject itemObject = ItemInSlot;


            Item item = itemObject.GetComponent<Item>();

            if(item != null)
            {
                item.inSlot = false;
                item.currentSlot = null;
            }


            Rigidbody rb = itemObject.GetComponent<Rigidbody>();

            if(rb != null)
            {
                rb.isKinematic = false;
            }


            // keluar dari slot inventory
            itemObject.transform.SetParent(null);


            // reset posisi physics
            rb?.WakeUp();

        }


        ItemInSlot = null;


        if(slotImage != null)
            slotImage.color = originalColor;
    }
}