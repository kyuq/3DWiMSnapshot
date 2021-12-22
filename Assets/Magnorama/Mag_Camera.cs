using UnityEngine;

namespace Magnorama
{
    // This script needs to execute its update method before the update methods of roi and mag poses to prevent lag
    [DefaultExecutionOrder(-100)]
    public class Mag_Camera : MonoBehaviour
    {
        // View configurations of this camera. This camera will be positioned depending on the selected viewpoint during the update method.
        public enum RenderViewPoint
        {
            NormalLeft,
            NormalRight,
            NormalMiddle,
            Front,
            Left,
            Right,
            Behind,
            Top,
            Bottom
        }
        public RenderViewPoint ViewPoint = RenderViewPoint.Front;

        // Render mode: DepthOnly captures the z-buffer into a rendertexture. ColorOnly writes the color capture into a rendertexture
        public enum RenderMode
        {
            DepthOnly,
            ColorOnly
        }
        public RenderMode Mode = RenderMode.ColorOnly;
        private RenderMode _Mode = RenderMode.ColorOnly;

        public bool EnableRendering = true;
        public RenderTexture renderTexture;
        private Camera _Camera;

        #region Depth Rendering Variables

        // Retrieve the shader for reading out the zBuffer
        private Material RenderDepthMat
        {
            get
            {
                if (!_RenderDepthMat) _RenderDepthMat = new Material(Shader.Find("Magnorama/RenderDepth"));
                _RenderDepthMat.SetFloat("nearPlane", _Camera.nearClipPlane);
                _RenderDepthMat.SetFloat("farPlane", _Camera.farClipPlane);
                return _RenderDepthMat;
            }
        }
        private Material _RenderDepthMat;
        #endregion

        private void Awake()
        {
            UpdateCameraSettings();
        }

        // Updates own camera settings
        private void UpdateCameraSettings()
        {
            _Mode = Mode;

            _Camera = GetComponent<Camera>();
            if (!_Camera) _Camera = gameObject.AddComponent<Camera>();
            _Camera.clearFlags = CameraClearFlags.SolidColor;
            _Camera.backgroundColor = new Color(0, 0, 0, 0);
            _Camera.stereoTargetEye = StereoTargetEyeMask.None; // This is requires to prevent the VR SDK to control this camera
            _Camera.aspect = 1;
            _Camera.useOcclusionCulling = false;
            _Camera.depth = 1;
            _Camera.nearClipPlane = 0.1f;
            _Camera.farClipPlane = 2;
            _Camera.fieldOfView = 75;

            if (Mag_Root.Instance)
                _Camera.cullingMask = Mag_Root.Instance.CapturingLayers;

            if (Mode == RenderMode.DepthOnly)
            {
                _Camera.depthTextureMode = DepthTextureMode.Depth;
            }
            else
            {
                _Camera.depthTextureMode = DepthTextureMode.None;
            }

            if (renderTexture)
            {
                _Camera.targetTexture = renderTexture;
            }
            else
            {
                _Camera.targetTexture = null;
            }

            // Render manually
            _Camera.enabled = false;

        }

        private void Update()
        {
            if (_Mode != Mode || (_Camera && _Camera.targetTexture != renderTexture))
                UpdateCameraSettings();

            RenderFromCamera();

        }

        // Fire the rendering command for this camera since the component is disabled by default. Call this method from within the update method of the inherited Mag_Controller
        public void RenderFromCamera()
        {
            if (!EnableRendering) return;

            if (ROI.Instance && Mag.Instance)
            {
                var World2ClipBox = Matrix4x4.TRS(ROI.Instance.transform.position, ROI.Instance.transform.rotation, Vector3.one);

                Matrix4x4 World2ClipCam = Matrix4x4.identity;

                var CamPosition = Matrix4x4.TRS(new Vector3(0, 0, -ROI.Instance.Scale * 1.15f), Quaternion.identity, Vector3.one);

                // Adjust camera pose depending on the selected configuration
                switch (ViewPoint)
                {
                    case RenderViewPoint.NormalLeft:
                        {
                            var mainCam = Camera.main.transform;
                            var magTransform = Mag.Instance.transform;
                            var World2ClipShow = Matrix4x4.TRS(magTransform.position, magTransform.rotation, Vector3.one);
                            var CameraPose = Matrix4x4.TRS(mainCam.position, mainCam.rotation, Vector3.one) * Matrix4x4.TRS(new Vector3(-0.0325f, 0, 0), Quaternion.identity, Vector3.one);
                            var Show2Cam = World2ClipShow.inverse * CameraPose;


                            var scaledPos = Show2Cam.GetColumn(3) * (ROI.Instance.Scale / Mag.Instance.Scale);
                            Show2Cam.SetColumn(3, new Vector4(scaledPos.x, scaledPos.y, scaledPos.z, 1.0f));
                            World2ClipCam = World2ClipBox * Show2Cam;

                            _Camera.fieldOfView = Camera.main.fieldOfView;
                            break;
                        }
                    case RenderViewPoint.NormalRight:
                        {
                            var mainCam = Camera.main.transform;
                            var magTransform = Mag.Instance.transform;
                            var World2ClipShow = Matrix4x4.TRS(magTransform.position, magTransform.rotation, Vector3.one);
                            var CameraPose = Matrix4x4.TRS(mainCam.position, mainCam.rotation, Vector3.one) * Matrix4x4.TRS(new Vector3(0.0325f, 0, 0), Quaternion.identity, Vector3.one);
                            var Show2Cam = World2ClipShow.inverse * CameraPose;


                            var scaledPos = Show2Cam.GetColumn(3) * (ROI.Instance.Scale / Mag.Instance.Scale);
                            Show2Cam.SetColumn(3, new Vector4(scaledPos.x, scaledPos.y, scaledPos.z, 1.0f));
                            World2ClipCam = World2ClipBox * Show2Cam;

                            _Camera.fieldOfView = Camera.main.fieldOfView;
                            break;
                        }
                    case RenderViewPoint.NormalMiddle:
                        {
                            var mainCam = Camera.main.transform;
                            var magTransform = Mag.Instance.transform;
                            var World2ClipShow = Matrix4x4.TRS(magTransform.position, magTransform.rotation, Vector3.one);
                            var CameraPose = Matrix4x4.TRS(mainCam.position, mainCam.rotation, Vector3.one);
                            var Show2Cam = World2ClipShow.inverse * CameraPose;


                            var scaledPos = Show2Cam.GetColumn(3) * (ROI.Instance.Scale / Mag.Instance.Scale);
                            Show2Cam.SetColumn(3, new Vector4(scaledPos.x, scaledPos.y, scaledPos.z, 1.0f));
                            World2ClipCam = World2ClipBox * Show2Cam;

                            _Camera.fieldOfView = Camera.main.fieldOfView;
                            break;
                        }
                    case RenderViewPoint.Front:
                        World2ClipCam = World2ClipBox * CamPosition;
                        break;
                    case RenderViewPoint.Right:
                        World2ClipCam = World2ClipBox * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, -90, 0), Vector3.one) * CamPosition;
                        break;
                    case RenderViewPoint.Left:
                        World2ClipCam = World2ClipBox * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 90, 0), Vector3.one) * CamPosition;
                        break;
                    case RenderViewPoint.Behind:
                        World2ClipCam = World2ClipBox * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 180, 0), Vector3.one) * CamPosition;
                        break;
                    case RenderViewPoint.Top:
                        World2ClipCam = World2ClipBox * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90, 0, 0), Vector3.one) * CamPosition;
                        break;
                    case RenderViewPoint.Bottom:
                        World2ClipCam = World2ClipBox * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90, 0, 0), Vector3.one) * CamPosition;
                        break;
                }

                transform.position = World2ClipCam.GetColumn(3);
                transform.rotation = Quaternion.LookRotation(World2ClipCam.GetColumn(2), World2ClipCam.GetColumn(1));

                // Adjust near and far clipping planes to encapsulate the ROI
                var ROISize = transform.forward * ROI.Instance.Scale;
                var pointInFrontROI = ROI.Instance.transform.position - ROISize;
                var pointBehindROI = ROI.Instance.transform.position + ROISize;

                _Camera.farClipPlane = Mathf.Clamp(_Camera.transform.InverseTransformPoint(pointBehindROI).z, 0.02f, 1000);
                _Camera.nearClipPlane = Mathf.Clamp(_Camera.transform.InverseTransformPoint(pointInFrontROI).z, 0.01f, _Camera.farClipPlane - 0.1f);

                // Manually render the camera
                _Camera.Render();
            }
        }

        // Reads camera data into the rendertexture and automatically forwards it to the Mag_Controller. This information will be used in the next Update call of the Mag_Controller
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {

            if (Mode == RenderMode.DepthOnly)
            {
                Graphics.Blit(source, destination, RenderDepthMat);

                if(Mag_Controller.Instance) 
                    Mag_Controller.Instance.UpdateDepthFrame(ViewPoint, ref destination, transform.position, transform.rotation, _Camera.fieldOfView, _Camera.nearClipPlane, _Camera.farClipPlane);
            }
            else // Mode == RenderMode.ColorOnly
            {
                if (!renderTexture)
                {
                    Graphics.Blit(source, destination);
                    return;
                }
                else
                {
                    Graphics.Blit(source, renderTexture);
                }

                if (Mag_Controller.Instance)
                    Mag_Controller.Instance.UpdateColorFrame(ViewPoint, ref renderTexture);
            }

        }
    }
}
