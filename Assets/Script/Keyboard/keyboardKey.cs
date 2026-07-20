using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class KeyboardKey : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    public KeyboardKeyType keyType = KeyboardKeyType.Character;

    [Tooltip("Diisi kalau Character, misal A, B, C, 1, 2, 3")]
    public string keyValue = "";

    [Header("Color")]
    public Image targetImage;

    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.9f, 0.9f, 0.9f);
    public Color pressedColor = Color.red;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (targetImage == null)
            targetImage = GetComponent<Image>();

        button.onClick.AddListener(OnPressed);

        if (targetImage != null)
            targetImage.color = normalColor;
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnPressed);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (targetImage != null)
            targetImage.color = pressedColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (targetImage != null)
            targetImage.color = normalColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetImage != null)
            targetImage.color = normalColor;
    }

    void OnPressed()
    {
        if (GlobalKeyboard.Instance == null)
            return;

        switch (keyType)
        {
            case KeyboardKeyType.Character:
                GlobalKeyboard.Instance.Type(keyValue);
                break;

            case KeyboardKeyType.Space:
                GlobalKeyboard.Instance.Space();
                break;

            case KeyboardKeyType.Backspace:
                GlobalKeyboard.Instance.Backspace();
                break;

            case KeyboardKeyType.Clear:
                GlobalKeyboard.Instance.Clear();
                break;

            case KeyboardKeyType.Enter:
                GlobalKeyboard.Instance.Enter();
                break;

            case KeyboardKeyType.Left:
                GlobalKeyboard.Instance.MoveLeft();
                break;

            case KeyboardKeyType.Right:
                GlobalKeyboard.Instance.MoveRight();
                break;

            case KeyboardKeyType.Home:
                GlobalKeyboard.Instance.Home();
                break;

            case KeyboardKeyType.End:
                GlobalKeyboard.Instance.End();
                break;

            case KeyboardKeyType.Shift:
                GlobalKeyboard.Instance.ToggleShift();
                break;

            case KeyboardKeyType.CapsLock:
                GlobalKeyboard.Instance.ToggleCapsLock();
                break;
        }
    }
}