using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.Rendering;

namespace Subdiv
{

    public class GPUSubdivisionSurface : MonoBehaviour {

        [SerializeField] protected ComputeShader subdivCompute;
        [SerializeField, Range(1, 5)] protected int details = 1;

        void Start()
        {
            var filter = GetComponent<MeshFilter>();
            var source = filter.mesh;
            var mesh = Subdivide(SubdivisionSurface.Weld(source, float.Epsilon, source.bounds.size.x), details);
            filter.sharedMesh = mesh;
        }

        public Mesh Subdivide(Mesh source, int details = 1, bool weld = false)
        {
            ComputeBuffer vertBuffer, edgeBuffer, triBuffer, subdivBuffer;

            vertBuffer = new ComputeBuffer(source.vertexCount, Marshal.SizeOf(typeof(Vector3)));
            vertBuffer.SetData(source.vertices);

            var triSource = source.triangles;
            triBuffer = new ComputeBuffer(triSource.Length / 3, Marshal.SizeOf(typeof(Triangle_t)));
            var triArray = new Triangle_t[triBuffer.count];

            var edges = new List<Edge_t>();

            for(int i = 0, n = triSource.Length; i < n; i += 3)
            {
                int iv0 = triSource[i], iv1 = triSource[i + 1], iv2 = triSource[i + 2];
                var e0 = new Edge_t() { v0 = iv0, v1 = iv1 };
                var e1 = new Edge_t() { v0 = iv1, v1 = iv2 };
                var e2 = new Edge_t() { v0 = iv2, v1 = iv0 };

                int ie0, ie1, ie2;

                if(edges.Contains(e0))
                {
                    ie0 = edges.IndexOf(e0);
                } else {
                    edges.Add(e0);
                    ie0 = edges.Count - 1;
                }

                if(edges.Contains(e1))
                {
                    ie1 = edges.IndexOf(e1);
                } else {
                    edges.Add(e1);
                    ie1 = edges.Count - 1;
                }

                if(edges.Contains(e2))
                {
                    ie2 = edges.IndexOf(e2);
                } else {
                    edges.Add(e2);
                    ie2 = edges.Count - 1;
                }

                triArray[i / 3] = new Triangle_t() {
                    v0 = iv0, v1 = iv1, v2 = iv2,
                    e0 = ie0, e1 = ie1, e2 = ie2
                };
            }

            edgeBuffer = new ComputeBuffer(edges.Count, Marshal.SizeOf(typeof(Edge_t)));
            edgeBuffer.SetData(edges.ToArray());
            triBuffer.SetData(triArray);

            subdivBuffer = new ComputeBuffer(vertBuffer.count + edgeBuffer.count, Marshal.SizeOf(typeof(Vector3)));
            var kernel = new Kernel(subdivCompute, "Subdivide");
            subdivCompute.SetBuffer(kernel.Index, "_VertexBuffer", vertBuffer);
            subdivCompute.SetBuffer(kernel.Index, "_EdgeBuffer", edgeBuffer);
            subdivCompute.SetBuffer(kernel.Index, "_TriangleBuffer", triBuffer);
            subdivCompute.SetBuffer(kernel.Index, "_SubdivBuffer", subdivBuffer);
            subdivCompute.SetInt("_VertexCount", vertBuffer.count);
            subdivCompute.SetInt("_EdgeCount", edgeBuffer.count);
            subdivCompute.SetInt("_TriangleCount", triBuffer.count);
            subdivCompute.SetInt("_SubdivCount", subdivBuffer.count);

            subdivCompute.Dispatch(kernel.Index, subdivBuffer.count / (int)kernel.ThreadX + 1, (int)kernel.ThreadY, (int)kernel.ThreadZ);

            Vector3[] subdivision = new Vector3[subdivBuffer.count];
            subdivBuffer.GetData(subdivision);

            // Build a mesh
            var mesh = new Mesh();
            int[] triDestination;

            if(weld)
            {
                mesh.vertices = subdivision;

                // Build new triangles
                triDestination = new int[triSource.Length * 4];
                var eoffset = vertBuffer.count;

                for (int i = 0, n = triSource.Length; i < n; i += 3) {
                    int iv0 = triSource[i], iv1 = triSource[i + 1], iv2 = triSource[i + 2];
                    var e0 = new Edge_t() { v0 = iv0, v1 = iv1 };
                    var e1 = new Edge_t() { v0 = iv1, v1 = iv2 };
                    var e2 = new Edge_t() { v0 = iv2, v1 = iv0 };
                    int ie0 = edges.IndexOf(e0) + eoffset, ie1 = edges.IndexOf(e1) + eoffset, ie2 = edges.IndexOf(e2) + eoffset;

                    int it0 = i * 4;
                    int it1 = it0 + 3;
                    int it2 = it1 + 3;
                    int it3 = it2 + 3;

                    triDestination[it0] = iv0; triDestination[it0 + 1] = ie0; triDestination[it0 + 2] = ie2;
                    triDestination[it1] = ie0; triDestination[it1 + 1] = iv1; triDestination[it1 + 2] = ie1;
                    triDestination[it2] = ie0; triDestination[it2 + 1] = ie1; triDestination[it2 + 2] = ie2;
                    triDestination[it3] = ie2; triDestination[it3 + 1] = ie1; triDestination[it3 + 2] = iv2;
                }
            } else {
                // Build new triangles
                triDestination = new int[triSource.Length * 4];
                Vector3[] vertices = new Vector3[triDestination.Length];
                var eoffset = vertBuffer.count;

                for (int i = 0, n = triSource.Length; i < n; i += 3) {
                    int iv0 = triSource[i], iv1 = triSource[i + 1], iv2 = triSource[i + 2];
                    var e0 = new Edge_t() { v0 = iv0, v1 = iv1 };
                    var e1 = new Edge_t() { v0 = iv1, v1 = iv2 };
                    var e2 = new Edge_t() { v0 = iv2, v1 = iv0 };
                    int ie0 = edges.IndexOf(e0) + eoffset, ie1 = edges.IndexOf(e1) + eoffset, ie2 = edges.IndexOf(e2) + eoffset;

                    int it0 = i * 4;
                    int it1 = it0 + 3;
                    int it2 = it1 + 3;
                    int it3 = it2 + 3;

                    vertices[it0] = subdivision[iv0]; vertices[it0 + 1] = subdivision[ie0]; vertices[it0 + 2] = subdivision[ie2];
                    vertices[it1] = subdivision[ie0]; vertices[it1 + 1] = subdivision[iv1]; vertices[it1 + 2] = subdivision[ie1];
                    vertices[it2] = subdivision[ie0]; vertices[it2 + 1] = subdivision[ie1]; vertices[it2 + 2] = subdivision[ie2];
                    vertices[it3] = subdivision[ie2]; vertices[it3 + 1] = subdivision[ie1]; vertices[it3 + 2] = subdivision[iv2];

                    triDestination[it0] = it0; triDestination[it0 + 1] = it0 + 1; triDestination[it0 + 2] = it0 + 2;
                    triDestination[it1] = it1; triDestination[it1 + 1] = it1 + 1; triDestination[it1 + 2] = it1 + 2;
                    triDestination[it2] = it2; triDestination[it2 + 1] = it2 + 1; triDestination[it2 + 2] = it2 + 2;
                    triDestination[it3] = it3; triDestination[it3 + 1] = it3 + 1; triDestination[it3 + 2] = it3 + 2;
                }

                mesh.vertices = vertices;
            }

            mesh.indexFormat = mesh.vertexCount < 65535 ? IndexFormat.UInt16 : IndexFormat.UInt32;
            mesh.triangles = triDestination;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            // Release buffers
            vertBuffer.Release();
            edgeBuffer.Release();
            triBuffer.Release();
            subdivBuffer.Release();

            return mesh;
        }

    }

}


