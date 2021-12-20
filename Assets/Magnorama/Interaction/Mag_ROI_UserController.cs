using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Magnorama
{
    public class Mag_ROI_UserController : MonoBehaviour
    {

        public float DefaultDistance = 1000f;

        public GameObject LaserObject;

        private GrabCube RoiGrab = null;
        void Start()
        {
            if (LaserObject)
            {
                LaserObject = Instantiate(LaserObject);
                var laser = LaserObject.GetComponent<Mag_ROI_Laser>();
                if (laser) laser.Controller = this;
            }
        }

        private void Update()
        {
            if (RoiGrab == null && ROI.Instance)
            {
                RoiGrab = ROI.Instance.gameObject.AddComponent<GrabCube>();
                RoiGrab.GrapPriority = 0;
                RoiGrab.ForceDisableGrabbing = true;
                enabled = false;
            }
        }
    }

}
