using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        transform.LookAt(Camera.main.transform);

        transform.rotation = Quaternion.Euler(-transform.rotation.eulerAngles.x, 0, 0);
    }
}
