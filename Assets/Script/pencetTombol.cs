using UnityEngine;
using UnityEngine.EventSystems;

public class VRUIHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float hoverScale = 1.15f;
    public float scaleSpeed = 10f;

    private Vector3 normalScale;
    private Vector3 targetScale;

    void Start()
    {
        normalScale = transform.localScale;
        targetScale = normalScale;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * scaleSpeed
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = normalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = normalScale;
    }
}