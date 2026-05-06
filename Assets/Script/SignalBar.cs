using UnityEngine;
using UnityEngine.UI;

public class SignalBar : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("Masukkan 10 gambar bar sinyal dari yang terkecil (index 0) ke terbesar (index 9)")]
    public Image[] signalBars;

    [Header("Colors")]
    public Color offColor = new Color(0.2f, 0.2f, 0.2f, 0.5f); 
    public Color weakColor = Color.red;    
    public Color fairColor = Color.yellow;  
    public Color strongColor = Color.green; 

    public void UpdateSignalBars(float currentDbm)
    {
        int activeBars = 0;

        if (currentDbm >= -45f) activeBars = 10;      
        else if (currentDbm >= -50f) activeBars = 9;  
        else if (currentDbm >= -55f) activeBars = 8;  
        else if (currentDbm >= -60f) activeBars = 7;  
        else if (currentDbm >= -65f) activeBars = 6;  
        else if (currentDbm >= -70f) activeBars = 5;  
        else if (currentDbm >= -75f) activeBars = 4;  
        else if (currentDbm >= -80f) activeBars = 3;  
        else if (currentDbm >= -85f) activeBars = 2;  
        else if (currentDbm >= -90f) activeBars = 1;  
        else activeBars = 0;                          

        Color currentColor;
        if (activeBars <= 3) 
        {
            currentColor = weakColor;
        }
        else if (activeBars <= 6) 
        {
            currentColor = fairColor;
        }
        else 
        {
            currentColor = strongColor;
        }

        for (int i = 0; i < signalBars.Length; i++)
        {
            if (i < activeBars)
            {
                signalBars[i].color = currentColor;
            }
            else
            {
                signalBars[i].color = offColor;
            }
        }
    }
}