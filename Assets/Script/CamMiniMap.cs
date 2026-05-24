using UnityEngine;

public class MinimapFollowPlayer : MonoBehaviour
{
    [Header("Target Player / XR Origin")]
    public Transform target;

    [Header("Tinggi Kamera Minimap")]
    public float height = 50f;

    [Header("Ikuti Rotasi Y Player")]
    public bool rotateWithPlayer = false;

    private void LateUpdate()
    {
        if (target == null) return;

        transform.position = new Vector3(
            target.position.x,
            target.position.y + height,
            target.position.z
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
}