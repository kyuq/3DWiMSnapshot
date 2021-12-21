using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Magnorama;

namespace Magnorama
{
    public class Mag_UserController : MonoBehaviour
    {
        public GameObject ControllerUI;
        public GameObject ControllerGrabUI;

        // Scaling
        private Vector2? PreviousScalePadValue = null;
        private GrabCube MagGrab = null;

        void Start()
        {
            if (ControllerUI) ControllerUI = Instantiate(ControllerUI);
            if (ControllerGrabUI) ControllerGrabUI = Instantiate(ControllerGrabUI);
        }

        void Update()
        {
            if (!Mag.Instance) return;

            if (MagGrab == null)
            {
                MagGrab = Mag.Instance.gameObject.AddComponent<GrabCube>();
                MagGrab.GrapPriority = 99;
            }


            if (ControllerManager.PadTouchLeftPressed)
            {
                var newPadValue = ControllerManager.PadLeftTouch2D;
                if (PreviousScalePadValue == null) PreviousScalePadValue = newPadValue;

                var angle = -Vector2.SignedAngle(PreviousScalePadValue.Value, newPadValue);

                ControllerManager.LeftVibrate(0.01f, 1, Mathf.Abs(angle * 0.005f));

                PreviousScalePadValue = newPadValue;

                Mag.Instance.Scale = Mathf.Clamp(Mag.Instance.Scale + angle * 0.002f, 0.3f, 1f);
            }
            else
            {
                PreviousScalePadValue = null;
            }

            if (ControllerUI) ControllerUI.transform.SetPositionAndRotation(ControllerManager.PositionLeft, ControllerManager.RotationLeft);
            if (ControllerGrabUI && Mag.Instance.IsInside(ControllerManager.PositionLeft))
            {
                ControllerGrabUI.SetActive(true);
                ControllerGrabUI.transform.SetPositionAndRotation(ControllerManager.PositionLeft, ControllerManager.RotationLeft);
            }
            else
            {
                ControllerGrabUI.SetActive(false);
            }
        }

    }

}
