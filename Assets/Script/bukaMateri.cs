using UnityEngine;

public class bukaMateri : MonoBehaviour
{

    [Header("Panels")]
    public GameObject pelajari;

    public void TogglePanel()
    {
        if (pelajari == null) return;

        pelajari.SetActive(!pelajari.activeSelf);
    }
}
