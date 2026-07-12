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
        if (hoveringItem == null)
            return;

        XRGrabInteractable grab =
            hoveringItem.GetComponent<XRGrabInteractable>();

        if (grab == null)
            return;

        Item item =
            hoveringItem.GetComponent<Item>();

        if (item == null)
            return;

        // Belum pernah di-grab, jangan boleh masuk inventory
        if (!item.canEnterInventory)
            return;

        // Masih dipegang, tunggu sampai dilepas
        if (grab.isSelected)
            return;

        InsertItem(hoveringItem);

        // Reset supaya harus di-grab lagi jika ingin dimasukkan ulang
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
            Item item = ItemInSlot.GetComponent<Item>();

            if (item != null)
            {
                item.inSlot = false;
                item.currentSlot = null;
            }

            Rigidbody rb = ItemInSlot.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = false;
            }
        }

        ItemInSlot = null;

        slotImage.color = originalColor;
    }
}