using UnityEngine;

public class POEDevice : MonoBehaviour
{
    public bool isPoESource = false;   // switch/injector
    public bool hasPower = false;      // status power

    public void SetPoE(bool state)
    {
        hasPower = state;
        gameObject.SetActive(state);

        Debug.Log($"{name} PoE = {state}");
    }
}