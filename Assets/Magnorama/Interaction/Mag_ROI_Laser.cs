using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Magnorama;

namespace Magnorama
{
    [RequireComponent(typeof(LineRenderer))]
    public class Mag_ROI_Laser : MonoBehaviour
    {
        public Mag_ROI_UserController Controller;
        private LineRenderer lineRenderer;

        private Vector2? PreviousScalePadValue = null;

        private float Distance = 1000f;
        public float ScalingSpeed = 1;

        private GameObject SphereCursor;

        private Vector3? _GrabbingPosition = null;
        private Matrix4x4 _GrabbingROIPose;
        private float _GrabbingDistance;
        void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();

            SphereCursor = Instantiate(Resources.Load("SphereCursor") as GameObject);
            SphereCursor.SetActive(false);
        }

        private void OnEnable()
        {
            if (Controller)
                Distance = Controller.DefaultDistance;
        }

        void Update()
        {

            transform.SetPositionAndRotation(ControllerManager.PositionRight, ControllerManager.RotationRight);

            // Positioning
            var lineLength = Distance;
            if (Physics.Raycast(transform.position, transform.forward, out var hit, lineLength))
            {
                lineLength = hit.distance;
            }
            var ScaleMag2ROI = ROI.Instance.Scale / Mag.Instance.Scale;
            var ScaleROI2Mag = 1.0f / ScaleMag2ROI;

            if (LaserPassThroughMagnorama(transform.position, transform.forward, out var minPoint, out var maxPoint))
            {
                var rayOrigin = Mag.TransformMag2ROIPoint(transform.position);
                var rayDirection = Mag.TransformMag2ROIVector(transform.forward);

                // User points at the geometry inside the Magnorama
                if (Physics.Raycast(rayOrigin, rayDirection, out var hitMag, Distance * ScaleMag2ROI) && ROI.Instance.IsInside(hitMag.point))
                {

                    lineLength = Vector3.Distance(hitMag.point, rayOrigin) * ScaleROI2Mag;

                    SphereCursor.SetActive(true);
                    SphereCursor.transform.position = transform.position + transform.forward * lineLength;
                    SphereCursor.transform.LookAt(Camera.main.transform.position, Vector3.up);

                    if (ControllerManager.TriggerRightDown)
                    {
                        _GrabbingPosition = hitMag.point;
                        _GrabbingDistance = hitMag.distance;
                        _GrabbingROIPose = ROI.Instance.transform.localToWorldMatrix;
                        SoundManager.PlayTake(SphereCursor.transform.position);
                    }
                }
                // User points through Magnorama, but is not hitting any colliders.
                else
                {
                    var outPointROI = ROI.Instance.transform.TransformPoint(Mag.Instance.transform.InverseTransformPoint(maxPoint));

                    lineLength = Vector3.Distance(rayOrigin, outPointROI) * ScaleROI2Mag;

                    SphereCursor.SetActive(true);
                    SphereCursor.transform.position = transform.position + transform.forward * lineLength;
                    SphereCursor.transform.LookAt(Camera.main.transform.position, Vector3.up);

                    if (ControllerManager.TriggerRightDown)
                    {
                        _GrabbingPosition = outPointROI;
                        _GrabbingDistance = Vector3.Distance(rayOrigin, outPointROI);
                        _GrabbingROIPose = ROI.Instance.transform.localToWorldMatrix;
                        SoundManager.PlayTake(SphereCursor.transform.position);
                    }
                }
            }
            // User is not pointing at the Magnorama
            else
            {
                SphereCursor.SetActive(false);
            }

            // Grabbing in progress
            if (_GrabbingPosition.HasValue)
            {
                if (ControllerManager.TriggerRightPressed)
                {
                    // Compute controller ray transformed to ROI space
                    var rayOrigin = _GrabbingROIPose.MultiplyPoint(Mag.Instance.transform.InverseTransformPoint(transform.position));
                    var rayDirection = _GrabbingROIPose.MultiplyVector(Mag.Instance.transform.InverseTransformVector(transform.forward));

                    var translatePose = rayOrigin + rayDirection * _GrabbingDistance * ScaleROI2Mag;

                    // Spherecursor displays grabbing point
                    SphereCursor.transform.position = transform.position + transform.forward * _GrabbingDistance * ScaleROI2Mag;
                    SphereCursor.transform.LookAt(Camera.main.transform.position, Vector3.up);

                    ROI.Instance.transform.position = (Vector3)_GrabbingROIPose.GetColumn(3) - (translatePose - _GrabbingPosition.Value);

                    lineLength = _GrabbingDistance * ScaleROI2Mag;
                }
                else
                {
                    _GrabbingPosition = null;
                    SoundManager.PlayPut(SphereCursor.transform.position);
                }
            }
            else
            {
                // Scaling
                if (ControllerManager.PadTouchRightPressed)
                {
                    var newPadValue = ControllerManager.PadRightTouch2D;
                    if (PreviousScalePadValue == null) PreviousScalePadValue = newPadValue;

                    var angle = Vector2.SignedAngle(PreviousScalePadValue.Value, newPadValue);

                    ControllerManager.RightVibrate(0.01f, 1, Mathf.Abs(angle * 0.005f));

                    PreviousScalePadValue = newPadValue;

                    float internalScalingSpeed = 0.003f;
                    if (ROI.Instance.Scale > 5) internalScalingSpeed = 0.03f;
                    if (ROI.Instance.Scale > 30) internalScalingSpeed = 0.09f;
                    ROI.Instance.Scale = Mathf.Clamp(ROI.Instance.Scale + angle * internalScalingSpeed * ScalingSpeed, 0.15f, 100f);
                }
                else
                {
                    PreviousScalePadValue = null;
                }

                if (ControllerManager.TriggerRightPressed)
                {
                    lineRenderer.startWidth = 0.02f;
                    lineRenderer.endWidth = 0.02f;

                    ROI.Instance.transform.position = transform.position + transform.forward * lineLength;
                }
                else
                {
                    lineRenderer.startWidth = 0.01f;
                    lineRenderer.endWidth = 0.01f;
                }

            }


            lineRenderer.SetPosition(1, new Vector3(0, 0, lineLength));

        }

        // Adapted from https://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-box-intersection
        private bool LaserPassThroughMagnorama(Vector3 origin, Vector3 direction, out Vector3 minPoint, out Vector3 maxPoint)
        {
            minPoint = Vector3.zero;
            maxPoint = Vector3.zero;

            // Negate offset and rotation of Magnorama
            Matrix4x4 magPose = Matrix4x4.TRS(Mag.Instance.transform.position, Mag.Instance.transform.rotation, Vector3.one);
            origin = magPose.inverse.MultiplyPoint(origin);
            direction = magPose.inverse.MultiplyVector(direction);

            float tmin, tmax, tymin, tymax, tzmin, tzmax;

            var invDir = new Vector3(1.0f / direction.x, 1.0f / direction.y, 1.0f / direction.z);
            int sign_x = invDir.x < 0 ? 1 : 0;
            int sign_y = invDir.y < 0 ? 1 : 0;
            int sign_z = invDir.z < 0 ? 1 : 0;

            Vector3[] bounds = new Vector3[2];
            var magScale = Mag.Instance.Scale / 2;
            bounds[0] = new Vector3(-magScale, -magScale, -magScale);
            bounds[1] = new Vector3(magScale, magScale, magScale);

            tmin = (bounds[sign_x].x - origin.x) * invDir.x;
            tmax = (bounds[1 - sign_x].x - origin.x) * invDir.x;
            tymin = (bounds[sign_y].y - origin.y) * invDir.y;
            tymax = (bounds[1 - sign_y].y - origin.y) * invDir.y;

            if ((tmin > tymax) || (tymin > tmax))
                return false;
            if (tymin > tmin)
                tmin = tymin;
            if (tymax < tmax)
                tmax = tymax;

            tzmin = (bounds[sign_z].z - origin.z) * invDir.z;
            tzmax = (bounds[1 - sign_z].z - origin.z) * invDir.z;

            if ((tmin > tzmax) || (tzmin > tmax))
                return false;
            if (tzmin > tmin)
                tmin = tzmin;
            if (tzmax < tmax)
                tmax = tzmax;

            if (tmin < 0 && tmax < 0) return false;

            minPoint = magPose.MultiplyPoint(origin + direction * tmin);
            maxPoint = magPose.MultiplyPoint(origin + direction * tmax);

            return true;

        }


    }

}

