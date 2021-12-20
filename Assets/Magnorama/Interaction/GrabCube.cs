using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Magnorama
{
    public class GrabCube : MonoBehaviour
    {
        private static HashSet<GrabCube> Instances;
        private static Coroutine ManageGrabAuthority = null;

        public bool EnableGrabbing = true;
        public bool ForceDisableGrabbing = false;
        public int GrapPriority = 0;

        public bool SnapBack = false;
        private Vector3? SnapPosition = null;
        private Quaternion? SnapRotation = null;

        private bool _IsGrabbing = false;
        private Matrix4x4 _GrabController2Cube;


        private void Start()
        {
            if (Instances == null) Instances = new HashSet<GrabCube>();

            if (ManageGrabAuthority == null) ManageGrabAuthority = StartCoroutine(CheckGrabAuthorityLoop());

            Instances.Add(this);

            StartCoroutine(SnapBack_Coroutine());
        }

        private void Update()
        {
            var pos = ControllerManager.PositionLeft;
            var rot = ControllerManager.RotationLeft;

            if (ControllerManager.TriggerLeftDown && EnableGrabbing && !ForceDisableGrabbing)
            {

                if (IsInside(pos))
                {
                    var world2Cube = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

                    var currentPose = Matrix4x4.TRS(pos, rot, Vector3.one);
                    if (!_IsGrabbing)
                    {
                        _GrabController2Cube = currentPose.inverse * world2Cube;
                        _IsGrabbing = true;

                        if(SoundManager.Instance) SoundManager.PlayTake(transform.position);
                        if (SnapBack)
                        {
                            SnapPosition = transform.position;
                            SnapRotation = transform.rotation;
                        }
                    }
                }
                else
                {
                    _IsGrabbing = false;
                }
            }
            else if (ControllerManager.TriggerLeftPressed && _IsGrabbing)
            {
                if (IsInside(pos))
                {
                    var currentPose = Matrix4x4.TRS(pos, rot, Vector3.one);

                    var l2w = currentPose * _GrabController2Cube;
                    transform.position = l2w.GetColumn(3);
                    transform.rotation = Quaternion.LookRotation(l2w.GetColumn(2), l2w.GetColumn(1));
                }
                else
                {
                    _IsGrabbing = false;
                }
            }
            else
            {
                if (_IsGrabbing && SoundManager.Instance) SoundManager.PlayPut(transform.position);
                _IsGrabbing = false;
            }
        }

        private IEnumerator SnapBack_Coroutine()
        {
            while (true)
            {
                if (SnapBack && !_IsGrabbing)
                {
                    if (SnapPosition.HasValue)
                    {
                        transform.position = Vector3.Lerp(transform.position, SnapPosition.Value, 0.1f);
                        if (Vector3.Distance(transform.position, SnapPosition.Value) < 0.005f) SnapPosition = null;
                    }

                    if (SnapRotation.HasValue)
                    {
                        transform.rotation = Quaternion.Lerp(transform.rotation, SnapRotation.Value, 0.1f);
                        if (Quaternion.Angle(transform.rotation, SnapRotation.Value) < 0.5f) SnapRotation = null;
                    }
                }


                yield return null;
            }
        }

        private static IEnumerator CheckGrabAuthorityLoop()
        {
            while (true)
            {
                if (ControllerManager.Instance)
                {
                    var pos = ControllerManager.PositionLeft;

                    GrabCube highestPrio = null;

                    foreach (var g in Instances)
                    {
                        if (g.IsInside(pos) && !g.ForceDisableGrabbing)
                        {
                            if (highestPrio == null || g.GrapPriority > highestPrio.GrapPriority)
                            {
                                highestPrio = g;
                            }
                            else
                            {
                                g.EnableGrabbing = false;
                            }
                        }
                    }
                    if (highestPrio != null) highestPrio.EnableGrabbing = true;
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

        private void OnDestroy()
        {
            Instances.Remove(this);
        }
    }

}
