using UnityEngine;

public class MinimapFollowPlayer : MonoBehaviour
{
    [Header("Target Player / XR Origin")]
    public Transform target;

    [Header("Tinggi Kamera Minimap")]
    public float height = 50f;

    [Header("Ikuti Rotasi Y Player")]
    public bool rotateWithPlayer = false;

    [Header("Offset Geser Map dari Tombol")]
    public Vector3 mapOffset = Vector3.zero;

    [Header("Batas Geser Map")]
    public float maxOffsetX = 40f;
    public float maxOffsetZ = 40f;

    private void LateUpdate()
    {
        if (target == null) return;

        transform.position = new Vector3(
            target.position.x + mapOffset.x,
            target.position.y + height,
            target.position.z + mapOffset.z
        );

        if (rotateWithPlayer)
        {
            transform.rotation = Quaternion.Euler(90f, target.eulerAngles.y, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }

    public void AddOffset(Vector3 offset)
    {
        mapOffset += offset;

        mapOffset.x = Mathf.Clamp(mapOffset.x, -maxOffsetX, maxOffsetX);
        mapOffset.z = Mathf.Clamp(mapOffset.z, -maxOffsetZ, maxOffsetZ);
    }

    public void ResetOffset()
    {
        mapOffset = Vector3.zero;
    }
}