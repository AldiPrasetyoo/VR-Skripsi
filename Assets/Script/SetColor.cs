using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class SetColor : MonoBehaviour
{
    public List<Color> colors;

    public void setColor(int i)
    {
        GetComponent<Renderer>().material.color = colors[i];
    }
}