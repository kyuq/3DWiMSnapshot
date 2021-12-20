using UnityEngine;

namespace Magnorama
{
    public abstract class Mag_Controller : MonoBehaviour
    {
        public static Mag_Controller Instance = null;

        protected void Awake()
        {
            Instance = this;
        }

        // Creates the virtual depth and color camera and return them trough the out parameters
        public void CreateMagnoramaRGBD(string name, Mag_Camera.RenderViewPoint viewpoint, out Mag_Camera DepthCamera, out Mag_Camera ColorCamera)
        {
            if(Instance)
            {
                GameObject RGBD = new GameObject(name);
                RGBD.transform.parent = Instance.transform;

                GameObject magCamDepthGO = new GameObject();
                magCamDepthGO.name = name + "_Depth";
                magCamDepthGO.transform.parent = RGBD.transform;
                DepthCamera = magCamDepthGO.AddComponent<Mag_Camera>();
                DepthCamera.Mode = Mag_Camera.RenderMode.DepthOnly;
                DepthCamera.ViewPoint = viewpoint;
                DepthCamera.renderTexture = new RenderTexture(1024, 1024, 24, RenderTextureFormat.R16);
                DepthCamera.renderTexture.Create();

                GameObject magCamGOColor = new GameObject();
                magCamGOColor.name = name + "_Color";
                magCamGOColor.transform.parent = RGBD.transform;
                ColorCamera = magCamGOColor.AddComponent<Mag_Camera>();
                ColorCamera.Mode = Mag_Camera.RenderMode.ColorOnly;
                ColorCamera.ViewPoint = viewpoint;
                ColorCamera.renderTexture = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
                ColorCamera.renderTexture.Create();
            }
            else
            {
                DepthCamera = null;
                ColorCamera = null;
                Debug.LogError("Failed to create virtuals RGB-D cameras. Make sure there is exactly one component in your scene that inherets from Mag_Controller");
            }

        }

        // Switch through the RGB-D cameras and update the depth image depending on the provided viewpoint.
        public abstract void UpdateDepthFrame(Mag_Camera.RenderViewPoint viewpoint, ref RenderTexture depth, Vector3 pos, Quaternion rot, float fov, float near, float far);
        // Switch through the RGB-D cameras and update the color image depending on the provided viewpoint.
        public abstract void UpdateColorFrame(Mag_Camera.RenderViewPoint viewpoint, ref RenderTexture color);
    }
}

