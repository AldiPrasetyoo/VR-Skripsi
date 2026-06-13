using UnityEngine;
using UnityEngine.UI;

public class TabletMapPanController : MonoBehaviour
{
    [Header("Follow Player Minimap")]
    public MinimapFollowPlayer minimapFollow;

    [Header("Camera Minimap")]
    public Camera minimapCamera;

    [Header("Kecepatan Geser Map")]
    public float panSpeed = 10f;

    [Header("Slider Zoom")]
    public Slider zoomSlider;

    [Tooltip("Semakin kecil, map semakin dekat / zoom in")]
    public float minZoom = 15f;

    [Tooltip("Semakin besar, map semakin jauh / zoom out")]
    public float maxZoom = 80f;

    [Header("Nilai Awal")]
    public float defaultZoom = 45f;

    private void Start()
    {
        if (minimapCamera != null)
        {
            minimapCamera.orthographicSize = defaultZoom;
        }

        if (zoomSlider != null)
        {
            zoomSlider.minValue = minZoom;
            zoomSlider.maxValue = maxZoom;
            zoomSlider.value = defaultZoom;

            zoomSlider.onValueChanged.RemoveAllListeners();
            zoomSlider.onValueChanged.AddListener(SetZoom);
        }
    }

    public void MoveUpHold()
    {
        MoveOffset(Vector3.forward);
    }

    public void MoveDownHold()
    {
        MoveOffset(Vector3.back);
    }

    public void MoveLeftHold()
    {
        MoveOffset(Vector3.left);
    }

    public void MoveRightHold()
    {
        MoveOffset(Vector3.right);
    }

    public void SetZoom(float value)
    {
        if (minimapCamera == null) return;

        minimapCamera.orthographicSize = value;
    }

    public void ResetMap()
    {
        if (minimapFollow != null)
            minimapFollow.ResetOffset();

        if (minimapCamera != null)
            minimapCamera.orthographicSize = defaultZoom;

        if (zoomSlider != null)
            zoomSlider.value = defaultZoom;
    }

    private void MoveOffset(Vector3 direction)
    {
        if (minimapFollow == null) return;

        minimapFollow.AddOffset(direction * panSpeed * Time.deltaTime);
    }
}

