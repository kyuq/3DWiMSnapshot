using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VRButton : MonoBehaviour
{
    
    public float ClickDuration = 1;

    public UnityEvent OnClickEvent;
    public UnityEvent OnEnterEvent;
    public UnityEvent OnExitEvent;
    private DateTime? StartClick = null;

    private GameObject Circle;
    private Mesh CircleMesh;
    private float outerRadius = 0.02f;
    private float innerRadius = 0.01f;

    private bool _isInsidePrevFrame = false;
    private void Start()
    {
        InitCircleIndicator();
        Circle.SetActive(false);

        StartCoroutine(ButtonLoop());
    }

    private IEnumerator ButtonLoop()
    {
        while(true)
        {
            if (IsInside(ControllerManager.PositionLeft) || IsInside(ControllerManager.PositionRight))
            {
                if(!_isInsidePrevFrame)
                {
                    _isInsidePrevFrame = true;
                    if (OnEnterEvent != null) OnEnterEvent.Invoke();
                }
                if (OnClickEvent != null && OnClickEvent.GetPersistentEventCount() > 0)
                {
                    if (StartClick == null)
                    {
                        StartClick = DateTime.Now;
                        Circle.SetActive(true);
                        AdjustMeshOnPercent(ref CircleMesh, 0);
                    }
                    else
                    {
                        if ((DateTime.Now - StartClick.Value).TotalSeconds > ClickDuration)
                        {
                            if (OnClickEvent != null) OnClickEvent.Invoke();
                            StartClick = null;
                            Circle.SetActive(false);
                            yield return new WaitForSeconds(2);
                        }
                        else
                        {
                            AdjustMeshOnPercent(ref CircleMesh, (int)(100 * (DateTime.Now - StartClick.Value).TotalSeconds / ClickDuration));
                        }
                    }
                }
            }
            else
            {
                if(_isInsidePrevFrame)
                {
                    _isInsidePrevFrame = false;
                    if (OnExitEvent != null) OnExitEvent.Invoke();
                }
                StartClick = null;
                Circle.SetActive(false);
            }


            yield return null;
        }
    }

    private bool IsInside(Vector3 pos)
    {
        Vector3 localPos = transform.InverseTransformPoint(pos);
        if (Mathf.Abs(localPos.x) < 0.5f && Mathf.Abs(localPos.y) < 0.5f && Mathf.Abs(localPos.z) < 0.5f)
            return true;
        else
            return false;
    }

    private void InitCircleIndicator()
    {
        Circle = new GameObject("CircleIndicator");
        Circle.transform.parent = transform;
        Circle.transform.localPosition = new Vector3(0.27f, 0, 0);
        Circle.transform.localRotation = Quaternion.Euler(0, -90, 0);
        Circle.transform.localScale = Vector3.one;
        Circle.transform.localScale = new Vector3(1 / Circle.transform.lossyScale.x, 1 / Circle.transform.lossyScale.y, 1 / Circle.transform.lossyScale.z);

        CircleMesh = new Mesh();
        CircleMesh.name = "CircleMesh";
        Circle.AddComponent<MeshRenderer>().material = Instantiate(Resources.Load("CircleMat") as Material);
        Circle.AddComponent<MeshFilter>().mesh = CircleMesh;

    }


    private void AdjustMeshOnPercent(ref Mesh mesh, int percentage)
    {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        for (int i = 0; i <= percentage; i++)
        {
            float x_out = outerRadius * Mathf.Cos(i * 3.6f * 0.0174533f + 1.5708f);
            float x_in = innerRadius * Mathf.Cos(i * 3.6f * 0.0174533f + 1.5708f);
            float y_out = outerRadius * Mathf.Sin(i * 3.6f * 0.0174533f + 1.5708f);
            float y_in = innerRadius * Mathf.Sin(i * 3.6f * 0.0174533f + 1.5708f);

            vertices.Add(new Vector3(x_out, y_out, 0));
            vertices.Add(new Vector3(x_in, y_in, 0));

            if (i > 0)
            {
                triangles.Add((i - 1) * 2);
                triangles.Add(i * 2 + 1);
                triangles.Add((i - 1) * 2 + 1);

                triangles.Add((i - 1) * 2);
                triangles.Add(i * 2);
                triangles.Add(i * 2 + 1);
            }
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

    }

}
