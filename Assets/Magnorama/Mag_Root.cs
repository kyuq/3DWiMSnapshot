using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Magnorama
{
    // 
    public class Mag_Root : MonoBehaviour
    {
        public static Mag_Root Instance;

        public LayerMask CapturingLayers;

        public Vector3 MagnoramaInitialPos;
        public float MagnoramaInitialScale = 0.75f;
        public Vector3 ROIInitialPos;
        public float ROIInitialScale = 0.15f;

        void Awake()
        {
            Instance = this;
            CreateMagnorama();
            CreateROI();
        }

        private void CreateMagnorama()
        {
            var MagnoramaGO = new GameObject("Magnorama");
            MagnoramaGO.transform.parent = transform;
            MagnoramaGO.transform.localPosition = MagnoramaInitialPos;
            MagnoramaGO.transform.localRotation = Quaternion.identity;
            MagnoramaGO.layer = LayerMask.NameToLayer("Ignore Raycast");
            Mag.Instance = MagnoramaGO.AddComponent<Mag>();
            Mag.Instance.Scale = MagnoramaInitialScale;
        }

        private void CreateROI()
        {
            var MagnoramaROIGO = new GameObject("ROI");
            MagnoramaROIGO.transform.parent = transform;
            MagnoramaROIGO.transform.localPosition = ROIInitialPos;
            MagnoramaROIGO.transform.localRotation = Quaternion.identity;
            var roi = MagnoramaROIGO.AddComponent<ROI>();
            roi.Scale = ROIInitialScale;
        }

        public void ResetPose()
        {
            if (Mag.Instance)
            {
                Mag.Instance.transform.localPosition = MagnoramaInitialPos;
                Mag.Instance.transform.localRotation = Quaternion.identity;
                Mag.Instance.Scale = MagnoramaInitialScale;
            }

            if (ROI.Instance)
            {
                ROI.Instance.transform.localPosition = ROIInitialPos;
                ROI.Instance.transform.localRotation = Quaternion.identity;
                ROI.Instance.Scale = ROIInitialScale;
            }
        }

    }

}
