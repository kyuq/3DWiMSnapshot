using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Magnorama
{
    public class Mag_RGBD
    {
        public bool EnableRendering
        {
            get
            {
                return _EnableRendering;
            }
            set
            {
                if (_EnableRendering == value) return;

                _EnableRendering = value;
                if (DepthCamera) DepthCamera.EnableRendering = value;
                if (ColorCamera) ColorCamera.EnableRendering = value;
            }
        }
        private bool _EnableRendering = true;

        private Mag_Camera DepthCamera;
        private Mag_Camera ColorCamera;
        
        // Rendertextures from the depth/ color camera.
        private RenderTexture CurrentDepthFrame;
        private RenderTexture BufferedDepthFrame;
        private RenderTexture CurrentColorFrame;
        private RenderTexture BufferedColorFrame;
        
        private ComputeShader Depth2MeshComputeShader;
        private int KernelID;

        // Camera settings
        private float FieldOfViewY = 75;
        private float CameraFarField = 2;
        private float CameraNearField = 0.1f;
        public float maxDistanceThresholdDepth = 0.01f;
        private Vector3 CurrentCapturePos;
        private Quaternion CurrentCaptureRot;

        // To be used for rendering the view to the unity viewport
        private Material ProceduralMat;

        // Compute buffer shared between Compute buffer and rendering material ProceduralMat
        private ComputeBuffer _args;
        private ComputeBuffer _ib;
        private ComputeBuffer _ub;
        private ComputeBuffer _vb;

        private bool _initiated = false;

        public Mag_RGBD(Mag_Camera DepthCamera, Mag_Camera ColorCamera)
        {
            this.DepthCamera = DepthCamera;
            this.ColorCamera = ColorCamera;
        }

        // Initiate ComputeShader, buffers, and materials.
        public void Initiate()
        {
            if (!_initiated && CurrentDepthFrame != null && CurrentColorFrame != null)
            {
                int size = CurrentDepthFrame.width * CurrentDepthFrame.height;

                Depth2MeshComputeShader = UnityEngine.Object.Instantiate(Resources.Load("Magnorama/DepthTexture2Mesh") as ComputeShader);
                KernelID = Depth2MeshComputeShader.FindKernel("Compute");

                Depth2MeshComputeShader.SetInt("_DepthWidth", CurrentDepthFrame.width);
                Depth2MeshComputeShader.SetInt("_DepthHeight", CurrentDepthFrame.height);

                _args = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
                _ib = new ComputeBuffer(size * 6, sizeof(int));
                _ub = new ComputeBuffer(size, 2 * sizeof(float));
                _vb = new ComputeBuffer(size, 3 * sizeof(float));

                int[] args = new int[] { size * 6, 2, 0, 0 };
                _args.SetData(args);


                // Set Kernel variables
                Depth2MeshComputeShader.SetBuffer(KernelID, "vertices", _vb);
                Depth2MeshComputeShader.SetBuffer(KernelID, "uv", _ub);
                Depth2MeshComputeShader.SetBuffer(KernelID, "triangles", _ib);

                ProceduralMat = new Material(Shader.Find("Magnorama/MeshFromComputeBuffer"));
                ProceduralMat.SetBuffer("vertices", _vb);
                ProceduralMat.SetBuffer("uv", _ub);
                ProceduralMat.SetBuffer("triangles", _ib);

                ProceduralMat.mainTexture = CurrentColorFrame;

                _initiated = true;
            }

        }

        // This method should be called from within UpdateDepthFrame of the inherited Mag_Controller component. This method stores the most recent depth texture into the buffer
        public void UpdateDepthFrame(ref RenderTexture depth, Vector3 pos, Quaternion rot, float fov, float near, float far)
        {
            if (BufferedDepthFrame == null || CurrentDepthFrame == null)
            {
                BufferedDepthFrame = new RenderTexture(depth.width, depth.height, 24, depth.format);
                CurrentDepthFrame = new RenderTexture(depth.width, depth.height, 24, depth.format);
            }
            BufferedDepthFrame = depth;

            CurrentCapturePos = pos;
            CurrentCaptureRot = rot;
            CameraNearField = near;
            CameraFarField = far;
            FieldOfViewY = fov;
        }

        // This method should be called from within UpdateColorFrame of the inherited Mag_Controller component. This method stores the most recent color texture into the buffer
        public void UpdateColorFrame(ref RenderTexture color)
        {
            if (BufferedColorFrame == null || CurrentColorFrame == null)
            {
                BufferedColorFrame = new RenderTexture(color.width, color.height, 24, color.format);
                CurrentColorFrame = new RenderTexture(color.width, color.height, 24, color.format);
            }

            BufferedColorFrame = color;
        }

        // Launches the compute buffer to turn the depth image into a triangulated point cloud and renders it for the unity viewport.
        public void Update()
        {
            if (!_EnableRendering) return;

            if (!_initiated)
            {
                Initiate();
                return;
            }

            int numthreads = 32;

            Depth2MeshComputeShader.SetFloat("_focalLength", (CurrentDepthFrame.height / 2.0f) / Mathf.Tan(FieldOfViewY / 2 * 0.0174533f));
            Depth2MeshComputeShader.SetFloat("_farPlane", CameraFarField);
            Depth2MeshComputeShader.SetFloat("_nearPlane", CameraNearField);
            Depth2MeshComputeShader.SetFloat("_maxDistThreshold", maxDistanceThresholdDepth);

            Depth2MeshComputeShader.SetMatrix("_World2ROI", ROI.Instance.transform.worldToLocalMatrix);
            Depth2MeshComputeShader.SetMatrix("_Transform", Matrix4x4.TRS(CurrentCapturePos, CurrentCaptureRot, Vector3.one));


            Graphics.CopyTexture(BufferedDepthFrame, CurrentDepthFrame);
            Graphics.CopyTexture(BufferedColorFrame, CurrentColorFrame);

            Depth2MeshComputeShader.SetTexture(KernelID, "_DepthTex", CurrentDepthFrame);

            Depth2MeshComputeShader.Dispatch(KernelID, CurrentDepthFrame.width / numthreads, CurrentDepthFrame.height / numthreads, 1);

            if (Mag.Instance && ProceduralMat)
            {
                Mag.Instance.SetShaderOptions(ref ProceduralMat);

                Graphics.DrawProceduralIndirect(ProceduralMat, new Bounds(Mag.Instance.transform.position, Vector3.one * Mag.Instance.Scale), MeshTopology.Triangles, _args,
                    castShadows: UnityEngine.Rendering.ShadowCastingMode.Off, receiveShadows: false);
            }

        }

        // Retrieves a Mesh container from the compute buffers. Useful for exporting a snapshot.
        public Mesh GetMesh()
        {
            Mesh m = new Mesh();
            m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            if (_initiated && CurrentDepthFrame != null && CurrentColorFrame != null)
            {
                int size = CurrentDepthFrame.width * CurrentDepthFrame.height;
                Vector3[] vertices = new Vector3[size];
                Vector2[] uv = new Vector2[size];
                int[] indices = new int[size * 6];

                _vb.GetData(vertices);
                _ub.GetData(uv);
                _ib.GetData(indices);

                var roiCenter = ROI.Instance.transform.position;
                m.vertices = vertices.Select(x => x - roiCenter).ToArray();
                m.uv = uv;
                indices = indices.Where(x => x > -1).ToArray();

                m.SetIndices(indices, MeshTopology.Triangles, 0);
            }

            return m;
        }

        // Retrieves a Texture2D object from the render texture
        public Texture2D GetTexture()
        {
            Texture2D t = null;
            if (_initiated && CurrentColorFrame != null)
            {
                t = new Texture2D(CurrentColorFrame.width, CurrentColorFrame.height, TextureFormat.RGBA32, false);

                RenderTexture.active = CurrentColorFrame;

                t.ReadPixels(new Rect(0, 0, CurrentColorFrame.width, CurrentColorFrame.height), 0, 0);
                t.Apply();

                RenderTexture.active = null;
            }
            return t;
        }

        // Releasing internal compute buffers
        public void Release()
        {
            if (_ib != null) _ib.Release();
            if (_ub != null) _ub.Release();
            if (_vb != null) _vb.Release();
        }

    }

}
