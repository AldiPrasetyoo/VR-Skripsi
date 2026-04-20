using UnityEngine;

public class InternetSource : MonoBehaviour
{
    void Start()
    {
        GetComponent<JaringanKabel>().hasInternet = true;
    }
}