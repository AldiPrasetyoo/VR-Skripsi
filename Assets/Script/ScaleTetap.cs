using UnityEngine;

public class KeepScale : MonoBehaviour
{
    Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    void LateUpdate()
    {
        transform.localScale = originalScale;
    }
}