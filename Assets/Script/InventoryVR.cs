using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryVR : MonoBehaviour
{
    [Header("Inventory UI")]
    public GameObject inventory;

    [Header("XR Origin")]
    public Transform xrOrigin;

    [Header("Spawn Settings")]
    public float distance = 0.8f;
    public float heightOffset = -0.2f;
    public float tiltAngle = 15f;

    [Header("Input")]
    public InputActionReference leftInventoryButton;
    public InputActionReference rightInventoryButton;

    private bool uiActive;
    private Camera vrCamera;

    private Vector3 worldOffset;

    private void Awake()
    {
        vrCamera = Camera.main;
    }

    private void Start()
    {
        if (inventory != null)
            inventory.SetActive(false);

        leftInventoryButton?.action.Enable();
        rightInventoryButton?.action.Enable();
    }

    private void OnDisable()
    {
        leftInventoryButton?.action.Disable();
        rightInventoryButton?.action.Disable();
    }

    private void Update()
    {
        bool leftPressed =
            leftInventoryButton != null &&
            leftInventoryButton.action.WasPressedThisFrame();

        bool rightPressed =
            rightInventoryButton != null &&
            rightInventoryButton.action.WasPressedThisFrame();

        if (leftPressed || rightPressed)
        {
            ToggleInventory();
        }

        if (uiActive && xrOrigin != null)
        {
            inventory.transform.position =
                xrOrigin.position + worldOffset;
        }
    }

    private void ToggleInventory()
    {
        uiActive = !uiActive;

        if (inventory == null)
            return;

        inventory.SetActive(uiActive);

        if (!uiActive)
            return;

        if (vrCamera == null)
            vrCamera = Camera.main;

        Transform cam = vrCamera.transform;

        // Spawn di depan arah pandang
        Vector3 position =
            cam.position +
            cam.forward * distance;

        position.y += heightOffset;

        inventory.transform.position = position;

        // Menghadap pemain
        Vector3 direction =
            cam.position - position;

        direction.y = 0;

        inventory.transform.rotation =
            Quaternion.LookRotation(-direction);

        inventory.transform.Rotate(
            tiltAngle,
            0,
            0,
            Space.Self);

        // Simpan offset terhadap XR Origin
        if (xrOrigin != null)
        {
            worldOffset =
                inventory.transform.position -
                xrOrigin.position;
        }
    }
}