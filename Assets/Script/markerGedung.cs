using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapObjectiveMarker : MonoBehaviour
{
    [Header("Target Dunia")]
    public Transform targetWorld;

    [Header("Camera Minimap")]
    public Camera minimapCamera;

    [Header("UI Map / RawImage")]
    public RectTransform mapRect;

    [Header("UI Marker Root")]
    public RectTransform markerRect;

    [Header("Image Marker")]
    public Image markerImage;
    public Sprite normalMarkerSprite;
    public Sprite arrowSprite;

    [Header("Text Label")]
    public TMP_Text markerText;
    public string labelText = "GD2";

    [Header("Pengaturan")]
    public float edgePadding = 35f;
    public bool rotateArrow = true;
    public float arrowRotationOffset = 0f;

    [Header("Text Tetap Lurus")]
    public bool keepTextStraight = true;

    private void Start()
    {
        if (markerText != null)
        {
            markerText.text = labelText;
            markerText.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        UpdateMarker();
    }

    private void UpdateMarker()
    {
        if (targetWorld == null) return;
        if (minimapCamera == null) return;
        if (mapRect == null) return;
        if (markerRect == null) return;

        Vector3 viewportPos = minimapCamera.WorldToViewportPoint(targetWorld.position);

        bool isVisible =
            viewportPos.x >= 0f &&
            viewportPos.x <= 1f &&
            viewportPos.y >= 0f &&
            viewportPos.y <= 1f &&
            viewportPos.z > 0f;

        float mapWidth = mapRect.rect.width;
        float mapHeight = mapRect.rect.height;

        float posX = (viewportPos.x - 0.5f) * mapWidth;
        float posY = (viewportPos.y - 0.5f) * mapHeight;

        if (isVisible)
        {
            markerRect.anchoredPosition = new Vector2(posX, posY);

            if (markerImage != null && normalMarkerSprite != null)
                markerImage.sprite = normalMarkerSprite;

            if (markerText != null)
            {
                markerText.text = labelText;
                markerText.gameObject.SetActive(true);
                markerText.rectTransform.localRotation = Quaternion.identity;
            }

            markerRect.localRotation = Quaternion.identity;
        }
        else
        {
            float clampedX = Mathf.Clamp(
                posX,
                -mapWidth / 2f + edgePadding,
                mapWidth / 2f - edgePadding
            );

            float clampedY = Mathf.Clamp(
                posY,
                -mapHeight / 2f + edgePadding,
                mapHeight / 2f - edgePadding
            );

            markerRect.anchoredPosition = new Vector2(clampedX, clampedY);

            if (markerImage != null && arrowSprite != null)
                markerImage.sprite = arrowSprite;

            if (markerText != null)
            {
                markerText.text = labelText;
                markerText.gameObject.SetActive(true);
            }

            if (rotateArrow)
            {
                Vector2 direction = new Vector2(posX, posY).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                markerRect.localRotation = Quaternion.Euler(0f, 0f, angle + arrowRotationOffset);

                if (keepTextStraight && markerText != null)
                {
                    markerText.rectTransform.localRotation =
                        Quaternion.Euler(0f, 0f, -(angle + arrowRotationOffset));
                }
            }
        }
    }
}