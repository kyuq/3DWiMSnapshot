using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport_3DUI : MonoBehaviour
{
    public void Teleport(Transform ReferenceTransform)
    {
        var refPos = ReferenceTransform.position;
        refPos.y = 0;

        var tPos = transform.position;
        tPos.y = 0;

        var camPos = Camera.main.transform.position;
        camPos.y = 0;

        var t2cam = camPos - tPos;

        var ref2cam = camPos - refPos;

        var teleportPos = tPos - ref2cam;
        teleportPos.y = refPos.y;

        transform.position = teleportPos;
    }
}
