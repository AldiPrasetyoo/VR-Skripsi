using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_InputField))]
public class TMPInputDetection :
    MonoBehaviour,
    IPointerClickHandler,
    ISelectHandler
{
    TMP_InputField input;

    void Awake()
    {
        input = GetComponent<TMP_InputField>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Select();
    }

    public void OnSelect(BaseEventData eventData)
    {
        Select();
    }

    void Select()
    {
        if (GlobalKeyboard.Instance == null)
            return;

        GlobalKeyboard.Instance.SetTarget(input);
    }
}