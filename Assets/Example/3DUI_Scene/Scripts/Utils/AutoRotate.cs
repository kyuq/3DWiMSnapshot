using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    public Vector3 RotationValue;
    

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(RotationValue);
    }
}
