using TMPro;
using UnityEngine;

public class GlobalKeyboard : MonoBehaviour
{
    public static GlobalKeyboard Instance;

    [Header("Keyboard")]
    [SerializeField] private GameObject keyboardRoot;

    [Header("Settings")]
    [SerializeField] private bool hideWhenDone = true;

    public TMP_InputField CurrentInput { get; private set; }

    private bool shift;
    private bool capsLock;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        HideKeyboard();
    }

    public void SetTarget(TMP_InputField input)
    {
        if (input == null)
            return;

        CurrentInput = input;

        CurrentInput.ActivateInputField();
        CurrentInput.Select();

        int pos = CurrentInput.text.Length;

        CurrentInput.caretPosition = pos;
        CurrentInput.selectionAnchorPosition = pos;
        CurrentInput.selectionFocusPosition = pos;

        ShowKeyboard();
    }

    public void ShowKeyboard()
    {
        if (keyboardRoot != null)
            keyboardRoot.SetActive(true);
    }

    public void HideKeyboard()
    {
        if (keyboardRoot != null)
            keyboardRoot.SetActive(false);
    }

    public void Type(string value)
    {
        if (CurrentInput == null)
            return;

        if (string.IsNullOrEmpty(value))
            return;

        string text = value;

        if (char.IsLetter(text[0]))
        {
            bool upper = shift ^ capsLock;

            text = upper
                ? text.ToUpper()
                : text.ToLower();
        }

        int pos = Mathf.Clamp(CurrentInput.caretPosition, 0, CurrentInput.text.Length);

        CurrentInput.text = CurrentInput.text.Insert(pos, text);

        pos += text.Length;

        CurrentInput.caretPosition = pos;
        CurrentInput.selectionAnchorPosition = pos;
        CurrentInput.selectionFocusPosition = pos;

        // Shift hanya berlaku sekali
        if (shift)
            shift = false;
    }

    public void Backspace()
    {
        if (CurrentInput == null)
            return;

        int pos = Mathf.Clamp(CurrentInput.caretPosition, 0, CurrentInput.text.Length);

        if (pos == 0)
            return;

        CurrentInput.text = CurrentInput.text.Remove(pos - 1, 1);

        pos--;

        CurrentInput.caretPosition = pos;
        CurrentInput.selectionAnchorPosition = pos;
        CurrentInput.selectionFocusPosition = pos;
    }

    public void Clear()
    {
        if (CurrentInput == null)
            return;

        CurrentInput.text = "";

        CurrentInput.caretPosition = 0;
        CurrentInput.selectionAnchorPosition = 0;
        CurrentInput.selectionFocusPosition = 0;
    }

    public void Space()
    {
        Type(" ");
    }

    public void Enter()
    {
        if (CurrentInput == null)
            return;

        CurrentInput.DeactivateInputField();

        if (hideWhenDone)
            HideKeyboard();
    }

    public void MoveLeft()
    {
        if (CurrentInput == null)
            return;

        int pos = Mathf.Clamp(CurrentInput.caretPosition - 1, 0, CurrentInput.text.Length);

        CurrentInput.caretPosition = pos;
        CurrentInput.selectionAnchorPosition = pos;
        CurrentInput.selectionFocusPosition = pos;
    }

    public void MoveRight()
    {
        if (CurrentInput == null)
            return;

        int pos = Mathf.Clamp(CurrentInput.caretPosition + 1, 0, CurrentInput.text.Length);

        CurrentInput.caretPosition = pos;
        CurrentInput.selectionAnchorPosition = pos;
        CurrentInput.selectionFocusPosition = pos;
    }

    public void Home()
    {
        if (CurrentInput == null)
            return;

        CurrentInput.caretPosition = 0;
        CurrentInput.selectionAnchorPosition = 0;
        CurrentInput.selectionFocusPosition = 0;
    }

    public void End()
    {
        if (CurrentInput == null)
            return;

        int pos = CurrentInput.text.Length;

        CurrentInput.caretPosition = pos;
        CurrentInput.selectionAnchorPosition = pos;
        CurrentInput.selectionFocusPosition = pos;
    }

    public void ToggleShift()
    {
        shift = !shift;
    }

    public void ToggleCapsLock()
    {
        capsLock = !capsLock;
    }

    public bool IsShift => shift;

    public bool IsCapsLock => capsLock;
}