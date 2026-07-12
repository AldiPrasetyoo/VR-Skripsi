using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryVR : MonoBehaviour
{
    [Header("Inventory UI")]
    public GameObject Inventory;

    [Header("Anchor Follow")]
    public Transform Anchor;

    [Header("XRI Controller Input (Kiri)")]
    public InputActionReference leftInventoryButton;

    [Header("XRI Controller Input (Kanan)")]
    public InputActionReference rightInventoryButton;

    private bool UIActive = false;

    private void Start()
    {
        Inventory.SetActive(false);

        // Aktifkan tombol kiri
        if (leftInventoryButton != null)
            leftInventoryButton.action.Enable();

        // Aktifkan tombol kanan
        if (rightInventoryButton != null)
            rightInventoryButton.action.Enable();
    }

    private void OnDisable()
    {
        // Matikan tombol kiri
        if (leftInventoryButton != null)
            leftInventoryButton.action.Disable();

        // Matikan tombol kanan
        if (rightInventoryButton != null)
            rightInventoryButton.action.Disable();
    }

    private void Update()
    {
        // Cek apakah tombol kiri ditekan frame ini
        bool isLeftPressed = leftInventoryButton != null && leftInventoryButton.action.WasPressedThisFrame();
        
        // Cek apakah tombol kanan ditekan frame ini
        bool isRightPressed = rightInventoryButton != null && rightInventoryButton.action.WasPressedThisFrame();

        // Jika salah satu tombol ditekan, jalankan Toggle
        if (isLeftPressed || isRightPressed)
        {
            ToggleInventory();
        }

        if (UIActive)
        {
            Inventory.transform.position = Anchor.position;
            
            Inventory.transform.rotation = Quaternion.Euler(
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