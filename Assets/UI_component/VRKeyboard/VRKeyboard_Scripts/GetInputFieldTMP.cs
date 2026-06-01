using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class TMPInputFieldDetection : MonoBehaviour, IPointerDownHandler, ISelectHandler
{
    private TMP_InputField myselfInputField;
    private OpenVirtualKeyboard keyboardController;

    private void Awake()
    {
        myselfInputField = GetComponent<TMP_InputField>();

        GameObject keyboardObject = GameObject.Find("Virtual Keyboard Controller");
        if (keyboardObject != null)
            keyboardController = keyboardObject.GetComponent<OpenVirtualKeyboard>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OpenKeyboard();
    }

    public void OnSelect(BaseEventData eventData)
    {
        OpenKeyboard();
    }

    private void OpenKeyboard()
    {
        if (myselfInputField == null) return;

        GetInputFieldTarget.SelectInputFieldName = transform.name;

        myselfInputField.ActivateInputField();

        GetInputFieldTarget.Index = myselfInputField.text.Length;
        myselfInputField.caretPosition = myselfInputField.text.Length;

#if UNITY_EDITOR
        print("SelectInputFieldName = " + transform.name);
        print("index = " + GetInputFieldTarget.Index);
#endif

        if (keyboardController != null)
        {
            keyboardController.onExitKeyboardArea = false;
            keyboardController.OnOpenVirtualKeyboard();
        }
    }
}