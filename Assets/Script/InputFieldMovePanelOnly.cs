using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class InputFieldMovePanelOnly : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
    [Header("Panel yang mau digerakkan")]
    public RectTransform panelToMove;

    [Header("Keyboard Object")]
    public GameObject virtualKeyboard;

    [Header("Virtual Keyboard Controller")]
    public OpenVirtualKeyboard keyboardController;

    [Header("Posisi")]
    public Vector2 positionBeforeClick = new Vector2(50, -28);
    public Vector2 positionAfterClick = new Vector2(50, -24);

    [Header("Gerakan")]
    public bool smoothMove = true;
    public float moveSpeed = 12f;

    private Vector2 targetPosition;
    private bool isMoving;
    private bool panelIsUp;
    private bool keyboardWasActive;

    private void Start()
    {
        if (panelToMove != null)
        {
            panelToMove.anchoredPosition = positionBeforeClick;
            targetPosition = positionBeforeClick;
        }

        if (keyboardController == null)
        {
            GameObject keyboardControllerObj = GameObject.Find("Virtual Keyboard Controller");
            if (keyboardControllerObj != null)
                keyboardController = keyboardControllerObj.GetComponent<OpenVirtualKeyboard>();
        }

        if (virtualKeyboard == null)
        {
            GameObject keyboardObj = GameObject.Find("Virtual Keyboard");
            if (keyboardObj != null)
                virtualKeyboard = keyboardObj;
        }

        keyboardWasActive = virtualKeyboard != null && virtualKeyboard.activeSelf;
    }

    private void Update()
    {
        if (virtualKeyboard != null)
        {
            bool keyboardIsActive = virtualKeyboard.activeSelf;

            if (panelIsUp && keyboardWasActive && !keyboardIsActive)
            {
                MoveDown();
            }

            keyboardWasActive = keyboardIsActive;
        }

        if (!smoothMove || panelToMove == null || !isMoving) return;

        panelToMove.anchoredPosition = Vector2.Lerp(
            panelToMove.anchoredPosition,
            targetPosition,
            Time.deltaTime * moveSpeed
        );

        if (Vector2.Distance(panelToMove.anchoredPosition, targetPosition) < 0.5f)
        {
            panelToMove.anchoredPosition = targetPosition;
            isMoving = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        MovePanelAndKeepKeyboard();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MovePanelAndKeepKeyboard();
    }

    private void MovePanelAndKeepKeyboard()
    {
        MoveUp();

        if (keyboardController != null)
            keyboardController.onExitKeyboardArea = false;
    }

    public void MoveUp()
    {
        if (panelToMove == null) return;

        panelIsUp = true;
        targetPosition = positionAfterClick;

        if (smoothMove)
            isMoving = true;
        else
            panelToMove.anchoredPosition = targetPosition;
    }

    public void MoveDown()
    {
        if (panelToMove == null) return;

        panelIsUp = false;
        targetPosition = positionBeforeClick;

        if (smoothMove)
            isMoving = true;
        else
            panelToMove.anchoredPosition = targetPosition;
    }
}