using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Magnorama
{
    public class Snapshot3D : MonoBehaviour
    {
        public bool Export_Trigger = false;

        private Material[] _Materials;
        private GameObject MeshObject;

        public void CreatePhoto(Mag_RGBD[] views, int grabPriority = 0, bool snapBack = true)
        {

            gameObject.AddComponent<MeshFilter>().mesh = Mag_Resources.CubeMesh;
            gameObject.AddComponent<MeshRenderer>().material = Mag_Resources.WireMatPhoto;
            gameObject.transform.localScale = Vector3.one * ROI.Instance.Scale;
            gameObject.layer = LayerMask.NameToLayer("Captureable");
            var grab = gameObject.AddComponent<GrabCube>();
            grab.GrapPriority = grabPriority;
            grab.SnapBack = snapBack;

            MeshObject = new GameObject();
            MeshObject.name = "CapturedMesh";
            MeshObject.layer = LayerMask.NameToLayer("Captureable");
            MeshObject.transform.parent = transform;
            MeshObject.transform.localPosition = Vector3.zero;
            MeshObject.transform.localRotation = Quaternion.identity;

            var filter = MeshObject.AddComponent<MeshFilter>();
            var mat = MeshObject.AddComponent<MeshRenderer>();

            mat.materials = new Material[7];

            // Preparing to combine meshes from all viewpoints into a single mesh. Every mesh must maintain its reference to the corresponding color texture.
            CombineInstance[] combine = new CombineInstance[views.Length];
            _Materials = new Material[views.Length];

            for (int i = 0; i < views.Length; i++)
            {
                combine[i].mesh = views[i].GetMesh();
                combine[i].transform = Matrix4x4.identity;

                _Materials[i] = new Material(Shader.Find("Magnorama/UnlitTexture"));
                _Materials[i].mainTexture = views[i].GetTexture();
            }

            filter.mesh = new Mesh();
            filter.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // Extend formatting to allow large meshes
            filter.mesh.CombineMeshes(combine, false); // Combine all meshes into a single mesh instance with submeshes
            filter.mesh.OptimizeReorderVertexBuffer(); // this removes vertices that are not referenced in the triangle list.
            filter.mesh.RecalculateBounds();
            mat.materials = _Materials;
        }

        private void Update()
        {
            for (int i = 0; i < _Materials.Length; i++)
            {
                _Materials[i].SetFloat("_EnableClipping", 1);
                _Materials[i].SetFloat("_ClipScale", 1); // Have to force clipscale to 1 since the scaling was already performed inside the compute shader
                _Materials[i].SetMatrix("_WorldToBox", Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale).inverse);
                _Materials[i].SetVector("_ScaleCenter", transform.position);
            }

            if(Export_Trigger)
            {
                Export_Trigger = false;
                Export();
            }
        }

        public void Export()
        {
            Export(MeshObject, gameObject.name);
        }


        // Export Methods
        #region Export
        private static string SnapshotFolder = "Snapshots/";

        private void Export(GameObject go, string filename)
        {
            var mf = go.GetComponent<MeshFilter>();
            if (!mf || !mf.mesh)
            {
                Debug.LogError("No mesh data found in MeshFilter component");
                return;
            }

            var mr = go.GetComponent<MeshRenderer>();
            if (!mr || mr.materials.Length != mf.mesh.subMeshCount)
            {
                Debug.LogError("Number of submeshes and materials do not match.");
                return;
            }

            // Create new folder with the timestamp in the name
            CreateFolder(SnapshotFolder);
            string extendedFolder = SnapshotFolder + "/" + filename + "/";
            CreateFolder(extendedFolder);

            // Export mesh and texture
            MeshExport(mf, extendedFolder, filename);
            MaterialExport(mr, extendedFolder, filename);
        }

        // Exports the mesh inside the meshfilter into a .obj wavefront file. Only supports vertices, uvs, and triangles.
        private void MeshExport(MeshFilter mf, string folder, string filename)
        {
            using (StreamWriter sw = new StreamWriter(folder + "/" + filename + ".obj"))
            {
                sw.Write("mtllib ./" + filename + ".mtl\n");

                CultureInfo ci = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;

                Mesh m = mf.sharedMesh;

                sw.Write("o " + m.name + "\n");

                Array.ForEach(m.vertices, v => sw.WriteLine("v " + (-v.x).ToString("f4") + " " +  v.y.ToString("f4") + " " + v.z.ToString("f4")));

                Array.ForEach(m.uv, v => sw.WriteLine("vt " + v.x.ToString("f4") + " " + v.y.ToString("f4")));

                for (int submesh = 0; submesh < m.subMeshCount; submesh++)
                {
                    int[] triangles = m.GetTriangles(submesh);
                    if (triangles.Length > 0)
                    {
                        sw.Write("usemtl mat_" + submesh + "\n");
                        int x = 0;
                        int y = 0;
                        int z = 0;

                        int length = triangles.Length;
                        int i = 0;
                        while(i < length)
                        {
                            x = triangles[i++] + 1;
                            y = triangles[i++] + 1;
                            z = triangles[i++] + 1;
                            sw.WriteLine("f " + y + "/" + y + " " + x + "/" + x + " " + z + "/" + z);
                        }

                    }
                }

            }
        }

        // Exports the texture for all materials inside the MeshRenderer and stores them as .png files
        private void MaterialExport(MeshRenderer mr, string folder, string filename)
        {
            using (StreamWriter sw = new StreamWriter(folder + "/" + filename + ".mtl"))
            {
                StringBuilder sb = new StringBuilder();

                var materials = mr.sharedMaterials;

                for (int i = 0; i < materials.Length; i++)
                {
                    if (mr.materials[i] && materials[i].mainTexture)
                    {
                        sw.WriteLine("newmtl mat_" + i);
                        sw.WriteLine("map_Kd ./tex_" + i + ".png\n");

                        //Save textures as PNG

                        var tex = mr.materials[i].mainTexture;
                        Texture2D texture2D = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);

                        RenderTexture currentRT = RenderTexture.active;

                        RenderTexture renderTexture = new RenderTexture(tex.width, tex.height, 32);
                        Graphics.Blit(tex, renderTexture);

                        RenderTexture.active = renderTexture;
                        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                        texture2D.Apply();
                        RenderTexture.active = currentRT;

                        File.WriteAllBytes(folder + "/tex_" + i + ".png", texture2D.EncodeToPNG());
                    }
                }
                sw.Write(sb.ToString());
            }
        }

        
        private bool CreateFolder(string folder)
        {
            try
            {
                System.IO.Directory.CreateDirectory(folder);
                return true;
            }
            catch
            {
                Debug.LogError("Failed to create target folder!");
                return false;
            }
        }
        #endregion
    }

}

