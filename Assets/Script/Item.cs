using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

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

    [HideInInspector]
    public bool isGrabbed = false;

    [HideInInspector]
    public bool canEnterInventory = false;

    private XRGrabInteractable grabInteractable;

    private void Awake()
    {
        originalScale = transform.localScale;

        grabInteractable = GetComponent<XRGrabInteractable>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnSelectEntered);
            grabInteractable.selectExited.AddListener(OnSelectExited);
        }
        else
        {
            Debug.LogWarning($"{name} tidak memiliki XRGrabInteractable.");
        }
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
            grabInteractable.selectExited.RemoveListener(OnSelectExited);
        }
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        canEnterInventory = true;

        OnGrab();
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        isGrabbed = false;
        StartCoroutine(ResetPermissionNextFrame());
    }

    private System.Collections.IEnumerator ResetPermissionNextFrame()
    {
        yield return new WaitForEndOfFrame();

        // Kalau item TIDAK berhasil masuk slot manapun di frame ini,
        // matikan izinnya supaya tidak "nyangkut aktif" selamanya.
        if (!inSlot)
            canEnterInventory = false;
    }

    public void ResetInventoryPermission()
    {
        canEnterInventory = false;
    }

    public void OnGrab()
    {
        transform.localScale = originalScale * grabScale;
    }

    public void OnInventory()
    {
        transform.localScale = originalScale * inventoryScale;
    }

    public void OnPlacedWorld()
    {
        transform.localScale = originalScale;
    }
}