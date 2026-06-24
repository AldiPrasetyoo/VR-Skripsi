using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class rayOff : MonoBehaviour
{
    [Header("Ray Components")]
    public XRRayInteractor rayInteractor;
    public XRInteractorLineVisual lineVisual;

    [Header("Teleport Input")]
    public InputActionProperty teleportActivateAction;

    [Header("Pengaturan")]
    public bool showRayWhenHoverUI = true;
    public bool showRayWhenTeleportPressed = true;

    [Header("Debug")]
    public bool debugLog = false;

    private void Reset()
    {
        rayInteractor = GetComponent<XRRayInteractor>();
        lineVisual = GetComponent<XRInteractorLineVisual>();
    }

    private void Awake()
    {
        if (rayInteractor == null)
            rayInteractor = GetComponent<XRRayInteractor>();

        if (lineVisual == null)
            lineVisual = GetComponent<XRInteractorLineVisual>();
    }

    private void OnEnable()
    {
        if (teleportActivateAction.action != null)
            teleportActivateAction.action.Enable();
    }

    private void OnDisable()
    {
        if (teleportActivateAction.action != null)
            teleportActivateAction.action.Disable();
    }

    private void Update()
    {
        bool hoveringUI = IsRayHoveringUI();
        bool teleportPressed = IsTeleportPressed();

        bool showRay = false;

        if (showRayWhenHoverUI && hoveringUI)
            showRay = true;

        if (showRayWhenTeleportPressed && teleportPressed)
            showRay = true;

        SetRayVisible(showRay);

        if (debugLog)
        {
            Debug.Log("Hover UI: " + hoveringUI + " | Teleport: " + teleportPressed + " | Ray Visible: " + showRay);
        }
    }

    private bool IsRayHoveringUI()
    {
        if (rayInteractor == null)
            return false;

        RaycastResult result;

        if (rayInteractor.TryGetCurrentUIRaycastResult(out result))
        {
            return result.gameObject != null;
        }

        return false;
    }

    private bool IsTeleportPressed()
    {
        if (teleportActivateAction.action == null)
            return false;

        return teleportActivateAction.action.IsPressed();
    }

    private void SetRayVisible(bool visible)
    {
        if (lineVisual != null)
            lineVisual.enabled = visible;
    }
}