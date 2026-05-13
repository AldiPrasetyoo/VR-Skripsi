using UnityEngine;

public class CamMiniMap : MonoBehaviour
{
    public Transform player; // Tarik XR Origin (atau Main Camera) ke sini di Inspector
    public float tinggiKamera = 50f;

    void LateUpdate()
    {
        if (player != null)
        {
            // Kamera mengikuti posisi X dan Z pemain, tapi ketinggian Y tetap
            Vector3 posisiBaru = player.position;
            posisiBaru.y = tinggiKamera;
            transform.position = posisiBaru;
        }
    }
}