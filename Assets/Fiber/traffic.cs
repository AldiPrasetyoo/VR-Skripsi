using UnityEngine;

public class TrafficPodMovement : MonoBehaviour
{
    [Header("Arah Gerak")]
    public Vector3 moveDirection = Vector3.forward;

    [Header("Kecepatan")]
    public float speed = 3f;

    [Header("Waktu Hidup")]
    public float lifeTime = 12f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += moveDirection.normalized * speed * Time.deltaTime;

        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }
}