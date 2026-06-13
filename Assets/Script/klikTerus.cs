using UnityEngine;
using UnityEngine.EventSystems;

public class TabletMapHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public enum ButtonType
    {
        Up,
        Down,
        Left,
        Right
    }

    [Header("Controller Map")]
    public TabletMapPanController mapController;

    [Header("Jenis Tombol")]
    public ButtonType buttonType;

    private bool isHolding = false;

    private void Update()
    {
        if (!isHolding) return;
        if (mapController == null) return;

        switch (buttonType)
        {
            case ButtonType.Up:
                mapController.MoveUpHold();
                break;

            case ButtonType.Down:
                mapController.MoveDownHold();
                break;

            case ButtonType.Left:
                mapController.MoveLeftHold();
                break;

            case ButtonType.Right:
                mapController.MoveRightHold();
                break;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHolding = false;
    }
}