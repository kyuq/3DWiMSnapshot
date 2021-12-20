
using System.Collections;
using UnityEngine;

namespace Magnorama
{
    // Component to be attached onto the Region of Interest. Provides access to useful values and transformation regarding the ROI.
    public class ROI : MonoBehaviour
    {
        public static ROI Instance;

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

        // Create Box mesh and load wireframe material
        private void Awake()
        {
            if (Instance) Destroy(Instance.gameObject);

            Instance = this;
            var meshFilter = gameObject.GetComponent<MeshFilter>();
            if (!meshFilter)
            {
                gameObject.AddComponent<MeshFilter>().mesh = Mag_Resources.CubeMesh;
                gameObject.AddComponent<MeshRenderer>().material = Mag_Resources.WirematROI;
            }
            else
            {
                meshFilter.mesh = Mag_Resources.CubeMesh;
                gameObject.GetComponent<MeshRenderer>().material = Mag_Resources.WirematROI;
            }

        }

        // Return true, if a position in world coordinate is inside the Magnorama.
        public bool IsInside(Vector3 worldPos)
        {
            Vector3 localPos = transform.InverseTransformPoint(worldPos);
            if (Mathf.Abs(localPos.x) < 0.5f && Mathf.Abs(localPos.y) < 0.5f && Mathf.Abs(localPos.z) < 0.5f)
                return true;
            else
                return false;
        }
    }
}
