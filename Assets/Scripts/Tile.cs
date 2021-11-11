using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    Unit myUnit;

    public GameObject highlight;

    void Start()
    {

    }

    void Update()
    {
        
    }

    public void SetUnit(Unit newUnit)
    {
        myUnit = newUnit;
    }

    public Unit GetUnit()
    {
        return myUnit;
    }

    public void Highlight(bool activate)
    {
        highlight.SetActive(activate);
    }
}
