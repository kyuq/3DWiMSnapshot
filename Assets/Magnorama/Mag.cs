
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Magnorama
{
    // Component to be attached onto the Magnorama (or World-in-Miniature). Provides access to useful values and transformation regarding the Magnorama and the ROI.
    // Configures compatible Magnorama materials for clipping and scaling.
    public class Mag : MonoBehaviour
    {
        public static Mag Instance;

        public float Scale
        {
            get
            {
                return gameObject.transform.localScale.x;
            }
            set
            {
                gameObject.transform.localScale = Vector3.one * value;
            }
        }

        // Transforms from ROI to Magnorama without consideration of the scale.
        public Matrix4x4 Roi2Mag
        {
            get
            {
                if (!ROI.Instance) return Matrix4x4.identity;

                var origin2roi = Matrix4x4.TRS(ROI.Instance.transform.position, ROI.Instance.transform.rotation, Vector3.one);
                var origin2mag = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                return origin2mag * origin2roi.inverse;
            }
        }

        // Magnification value that is dependent from the ROI.
        public float ClipScale
        {
            get
            {
                if (!ROI.Instance) return 1;
                return Scale * (1.0f / ROI.Instance.Scale);
            }
        }

        // Transformation to check if a point in world position would be clipped through the Magnorama (0<x<1)
        public Matrix4x4 WorldToBox
        {
            get
            {
                if (!ROI.Instance) return Matrix4x4.identity;

                return Matrix4x4.TRS(transform.position, transform.rotation, ROI.Instance.transform.localScale).inverse;
            }
        }

        // Create Box mesh and load wireframe material
        private void Awake()
        {
            if (Instance) Destroy(Instance.gameObject);

            Instance = this;
            var meshFilter = gameObject.GetComponent<MeshFilter>();
            if (!meshFilter)
            {
                gameObject.AddComponent<MeshFilter>().mesh = Mag_Resources.CubeMesh;
                gameObject.AddComponent<MeshRenderer>().material = Mag_Resources.WireMat;
            }
            else
            {
                meshFilter.mesh = Mag_Resources.CubeMesh;
                gameObject.GetComponent<MeshRenderer>().material = Mag_Resources.WireMat;
            }

        }

        // Transforms a point in world coordinate such that the relation matches between Magnorama and the ROI. From Mag to ROI.
        public static Vector3 TransformMag2ROIPoint(Vector3 point)
        {
            if (Mag.Instance && ROI.Instance)
            {
                return ROI.Instance.transform.TransformPoint(Mag.Instance.transform.InverseTransformPoint(point));
            }
            else return point;
        }

        // Transforms a Vector in world coordinate such that the relation matches between Magnorama and the ROI. From Mag to ROI.
        public static Vector3 TransformMag2ROIVector(Vector3 vector)
        {
            if (Mag.Instance && ROI.Instance)
            {
                return ROI.Instance.transform.TransformVector(Mag.Instance.transform.InverseTransformVector(vector));
            }
            else return vector;
        }

        // Transforms a point in world coordinate such that the relation matches between Magnorama and the ROI. From ROI to Mag.
        public static Vector3 TransformROI2MagPoint(Vector3 point)
        {
            if (Mag.Instance && ROI.Instance)
            {
                return Mag.Instance.transform.TransformPoint(ROI.Instance.transform.InverseTransformPoint(point));
            }
            else return point;
        }

        // Transforms a Vector in world coordinate such that the relation matches between Magnorama and the ROI. From ROI to Mag.
        public static Vector3 TransformROI2MagVector3(Vector3 vector)
        {
            if (Mag.Instance && ROI.Instance)
            {
                return Mag.Instance.transform.TransformVector(ROI.Instance.transform.InverseTransformVector(vector));
            }
            else return vector;
        }

        // Sets given compatible shaders for clipping and scaling
        public void SetShaderOptions(ref Material material)
        {
            if (ROI.Instance)
            {
                material.SetFloat("_EnableClipping", 1);
                material.SetFloat("_ClipScale", ClipScale);
                material.SetMatrix("_WorldToBox", WorldToBox);
                material.SetMatrix("_RoI2Mag", Roi2Mag);
                material.SetVector("_ScaleCenter", transform.position);
            }
            else
            {
                material.SetFloat("_EnableClipping", 0);
            }
        }

        // Return true, if a position in world coordinate is inside the Magnorama.
        public bool IsInside(Vector3 position)
        {
            if (!isActiveAndEnabled) return false;

            Vector3 localPos = transform.InverseTransformPoint(position);
            if (Mathf.Abs(localPos.x) < 0.5f && Mathf.Abs(localPos.y) < 0.5f && Mathf.Abs(localPos.z) < 0.5f)
                return true;
            else
                return false;
        }
    }

}
