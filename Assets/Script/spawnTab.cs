using UnityEngine;
using UnityEngine.InputSystem;

public class TabletToggleVR : MonoBehaviour
{
    [Header("Input Tombol X")]
    public InputActionReference xButtonAction;

    [Header("Tablet / iPad")]
    public GameObject tabletObject;

    [Header("Posisi Tablet")]
    public Transform tabletSpawnPoint;

    [Header("Tangan Kiri Visual")]
    public GameObject leftHandVisual;

    [Header("Ikuti posisi spawn point")]
    public bool followSpawnPoint = true;

    private bool isOpen;

    private void Start()
    {
        isOpen = false;

        if (tabletObject != null)
            tabletObject.SetActive(false);

        if (leftHandVisual != null)
            leftHandVisual.SetActive(true);
    }

    private void OnEnable()
    {
        if (xButtonAction != null)
        {
            xButtonAction.action.performed += OnXButtonPressed;
            xButtonAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (xButtonAction != null)
        {
            xButtonAction.action.performed -= OnXButtonPressed;
            xButtonAction.action.Disable();
        }
    }

    private void Update()
    {
        if (isOpen && followSpawnPoint)
        {
            if (tabletObject != null && tabletSpawnPoint != null)
            {
                tabletObject.transform.position = tabletSpawnPoint.position;
                tabletObject.transform.rotation = tabletSpawnPoint.rotation;
            }
        }
    }

    private void OnXButtonPressed(InputAction.CallbackContext context)
    {
        ToggleTablet();
    }

    public void ToggleTablet()
    {
        isOpen = !isOpen;

        if (tabletObject != null)
            tabletObject.SetActive(isOpen);

        if (leftHandVisual != null)
            leftHandVisual.SetActive(!isOpen);

        if (isOpen && tabletSpawnPoint != null && tabletObject != null)
        {
            tabletObject.transform.position = tabletSpawnPoint.position;
            tabletObject.transform.rotation = tabletSpawnPoint.rotation;
        }
    }

    public void CloseTablet()
    {
        isOpen = false;

        if (tabletObject != null)
            tabletObject.SetActive(false);

        if (leftHandVisual != null)
            leftHandVisual.SetActive(true);
    }
}