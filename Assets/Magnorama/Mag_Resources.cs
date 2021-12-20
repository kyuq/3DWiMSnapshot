using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Magnorama
{
    public class Mag_Resources
    {
        public static Mesh CubeMesh
        {
            get
            {
                if (_CubeMesh == null) _CubeMesh = CreateCubeMesh();
                return _CubeMesh;
            }
        }
        private static Mesh _CubeMesh;

        public static Material WireMat
        {
            get
            {
                if (_Magnorama_WireMat == null) _Magnorama_WireMat = Resources.Load("Magnorama/CubeWireframeMat") as Material;
                return _Magnorama_WireMat;
            }
        }
        private static Material _Magnorama_WireMat;

        public static Material WireMatPhoto
        {
            get
            {
                if (_Magnorama_WireMatPhoto == null) _Magnorama_WireMatPhoto = Resources.Load("Magnorama/CubeWireframePhotoMat") as Material;
                return _Magnorama_WireMatPhoto;
            }
        }
        private static Material _Magnorama_WireMatPhoto;

        public static Material WirematROI
        {
            get
            {
                if (_Magnorama_WireMatROI == null) _Magnorama_WireMatROI = Resources.Load("Magnorama/CubeWireframeROIMat") as Material;
                return _Magnorama_WireMatROI;
            }
        }
        private static Material _Magnorama_WireMatROI;

        public static Mesh CreateCubeMesh()
        {
            Vector3[] vertices = {
            new Vector3 (-0.5f, -0.5f, -0.5f),
            new Vector3 ( 0.5f, -0.5f, -0.5f),
            new Vector3 ( 0.5f,  0.5f, -0.5f),
            new Vector3 (-0.5f,  0.5f, -0.5f),
            new Vector3 (-0.5f,  0.5f,  0.5f),
            new Vector3 ( 0.5f,  0.5f,  0.5f),
            new Vector3 ( 0.5f, -0.5f,  0.5f),
            new Vector3 (-0.5f, -0.5f,  0.5f),
        };

            int[] triangles = {
            0, 2, 1, //face front
			0, 3, 2,
            2, 3, 4, //face top
			2, 4, 5,
            1, 2, 5, //face right
			1, 5, 6,
            0, 7, 4, //face left
			0, 4, 3,
            5, 4, 7, //face back
			5, 7, 6,
            0, 6, 7, //face bottom
			0, 1, 6
        };

            Mesh mesh = new Mesh();
            mesh.name = "_Cube";
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.Optimize();
            mesh.RecalculateNormals();

            return mesh;
        }
    }

}
