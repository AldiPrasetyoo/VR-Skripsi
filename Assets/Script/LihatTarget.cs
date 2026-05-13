using UnityEngine;

public class LihatTarget : MonoBehaviour
{
    private Transform cameraTransform;

    void Start()
    {
        // Mencari kamera utama VR
        cameraTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        // Membuat objek ini selalu menghadap kamera
        if (cameraTransform != null)
        {
            // Ambil posisi kamera
            Vector3 targetPosition = cameraTransform.position;
            // Hilangkan pengaruh sumbu Y agar papan tidak mendongak ke atas/bawah
            targetPosition.y = transform.position.y;
            
            // Rotasi papan agar LookAt ke kamera
            transform.LookAt(targetPosition);
            // Terkadang LookAt bikin teks terbalik 180 derajat, putar balik jika perlu
            transform.Rotate(0, 180, 0); 
        }
    }
}