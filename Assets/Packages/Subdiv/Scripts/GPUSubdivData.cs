using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.Rendering;

namespace Subdiv
{

    public class GPUSubdivData : System.IDisposable
    {
        public ComputeBuffer VertexBuffer { get { return vertBuffer; } }
        public ComputeBuffer EdgeBuffer { get { return edgeBuffer; } }
        public ComputeBuffer TriangleBuffer { get { return triBuffer; } }
        public ComputeBuffer SubdivBuffer { get { return subdivBuffer; } }

        public List<Edge_t> Edges { get { return edges; } }
        public int[] Triangles { get { return triangles; } }

        ComputeBuffer vertBuffer, edgeBuffer, triBuffer;
        ComputeBuffer subdivBuffer;
        int[] triangles;
        List<Edge_t> edges;

        public GPUSubdivData() { }

        public GPUSubdivData(ComputeBuffer vbuf, ComputeBuffer ebuf, ComputeBuffer tbuf, int[] tri, List<Edge_t> e) {
            vertBuffer = vbuf;
            edgeBuffer = ebuf;
            triBuffer = tbuf;
            triangles = tri;
            edges = e;

            subdivBuffer = new ComputeBuffer(vertBuffer.count + edgeBuffer.count, Marshal.SizeOf(typeof(Vector3)));
        }

        public GPUSubdivData(Mesh source) {
            vertBuffer = new ComputeBuffer(source.vertexCount, Marshal.SizeOf(typeof(Vector3)));
            vertBuffer.SetData(source.vertices);

            triangles = source.triangles;
            triBuffer = new ComputeBuffer(triangles.Length / 3, Marshal.SizeOf(typeof(Triangle_t)));
            var triArray = new Triangle_t[triBuffer.count];

            edges = new List<Edge_t>();

            for(int i = 0, n = triangles.Length; i < n; i += 3)
            {
                int iv0 = triangles[i], iv1 = triangles[i + 1], iv2 = triangles[i + 2];
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
        }

        Triangle_t AddTriangle(List<Edge_t> newEdges, int iv0, int iv1, int iv2)
        {
            var e0 = new Edge_t() { v0 = iv0, v1 = iv1 };
            var e1 = new Edge_t() { v0 = iv1, v1 = iv2 };
            var e2 = new Edge_t() { v0 = iv2, v1 = iv0 };

            int ie0, ie1, ie2;

            if (newEdges.Contains(e0)) {
                ie0 = newEdges.IndexOf(e0);
            } else {
                newEdges.Add(e0);
                ie0 = newEdges.Count - 1;
            }

            if (newEdges.Contains(e1)) {
                ie1 = newEdges.IndexOf(e1);
            } else {
                newEdges.Add(e1);
                ie1 = newEdges.Count - 1;
            }

            if (newEdges.Contains(e2)) {
                ie2 = newEdges.IndexOf(e2);
            } else {
                newEdges.Add(e2);
                ie2 = newEdges.Count - 1;
            }

            return new Triangle_t()
            {
                v0 = iv0,
                v1 = iv1,
                v2 = iv2,
                e0 = ie0,
                e1 = ie1,
                e2 = ie2
            };
        }

        public GPUSubdivData Next()
        {
            var newTriangles = new int[triangles.Length * 4];
            var newTriArray = new Triangle_t[triBuffer.count * 4];

            var newEdges = new List<Edge_t>();
            var eoffset = VertexBuffer.count;

            for (int i = 0, n = triangles.Length; i < n; i += 3) {
                int iv0 = triangles[i], iv1 = triangles[i + 1], iv2 = triangles[i + 2];
                var e0 = new Edge_t() { v0 = iv0, v1 = iv1 };
                var e1 = new Edge_t() { v0 = iv1, v1 = iv2 };
                var e2 = new Edge_t() { v0 = iv2, v1 = iv0 };
                int ie0 = edges.IndexOf(e0) + eoffset, ie1 = edges.IndexOf(e1) + eoffset, ie2 = edges.IndexOf(e2) + eoffset;

                int it00 = i * 4,    it01 = it00 + 1, it02 = it01 + 1;
                int it10 = it00 + 3, it11 = it10 + 1, it12 = it11 + 1;
                int it20 = it10 + 3, it21 = it20 + 1, it22 = it21 + 1;
                int it30 = it20 + 3, it31 = it30 + 1, it32 = it31 + 1;

                newTriangles[it00] = iv0; newTriangles[it01] = ie0; newTriangles[it02] = ie2;
                newTriangles[it10] = ie0; newTriangles[it11] = iv1; newTriangles[it12] = ie1;
                newTriangles[it20] = ie0; newTriangles[it21] = ie1; newTriangles[it22] = ie2;
                newTriangles[it30] = ie2; newTriangles[it31] = ie1; newTriangles[it32] = iv2;

                int it = (i / 3) * 4;

                // AddTriangle
                newTriArray[it]     = AddTriangle(newEdges, newTriangles[it00], newTriangles[it01], newTriangles[it02]);
                newTriArray[it + 1] = AddTriangle(newEdges, newTriangles[it10], newTriangles[it11], newTriangles[it12]);
                newTriArray[it + 2] = AddTriangle(newEdges, newTriangles[it20], newTriangles[it21], newTriangles[it22]);
                newTriArray[it + 3] = AddTriangle(newEdges, newTriangles[it30], newTriangles[it31], newTriangles[it32]);
            }

            var neBuffer = new ComputeBuffer(newEdges.Count, Marshal.SizeOf(typeof(Edge_t)));
            neBuffer.SetData(newEdges.ToArray());
            var ntriBuffer = new ComputeBuffer(newTriArray.Length, Marshal.SizeOf(typeof(Triangle_t)));
            ntriBuffer.SetData(newTriArray);

            var next = new GPUSubdivData(subdivBuffer, neBuffer, ntriBuffer, newTriangles, newEdges);

            ReleaseBuffer(vertBuffer); vertBuffer = null;
            ReleaseBuffer(edgeBuffer); edgeBuffer = null;
            ReleaseBuffer(triBuffer); triBuffer = null;
            subdivBuffer = null; // ref to NULL to use it in next GPUSubdivData

            return next;
        }

        public Mesh Build(bool weld = false)
        {
            var mesh = new Mesh();
            int[] newTriangles;

            var edges = Edges;
            var triangles = Triangles;

            Vector3[] subdivision = new Vector3[SubdivBuffer.count];
            SubdivBuffer.GetData(subdivision);

            if(weld)
            {
                mesh.vertices = subdivision;

                // Build new triangles
                newTriangles = new int[triangles.Length * 4];
                var eoffset = VertexBuffer.count;

                for (int i = 0, n = triangles.Length; i < n; i += 3) {
                    int iv0 = triangles[i], iv1 = triangles[i + 1], iv2 = triangles[i + 2];
                    var e0 = new Edge_t() { v0 = iv0, v1 = iv1 };
                    var e1 = new Edge_t() { v0 = iv1, v1 = iv2 };
                    var e2 = new Edge_t() { v0 = iv2, v1 = iv0 };
                    int ie0 = edges.IndexOf(e0) + eoffset, ie1 = edges.IndexOf(e1) + eoffset, ie2 = edges.IndexOf(e2) + eoffset;

                    int it0 = i * 4;
                    int it1 = it0 + 3;
                    int it2 = it1 + 3;
                    int it3 = it2 + 3;

                    newTriangles[it0] = iv0; newTriangles[it0 + 1] = ie0; newTriangles[it0 + 2] = ie2;
                    newTriangles[it1] = ie0; newTriangles[it1 + 1] = iv1; newTriangles[it1 + 2] = ie1;
                    newTriangles[it2] = ie0; newTriangles[it2 + 1] = ie1; newTriangles[it2 + 2] = ie2;
                    newTriangles[it3] = ie2; newTriangles[it3 + 1] = ie1; newTriangles[it3 + 2] = iv2;
                }
            } else {
                // Build new triangles
                newTriangles = new int[triangles.Length * 4];
                Vector3[] vertices = new Vector3[newTriangles.Length];
                var eoffset = VertexBuffer.count;

                for (int i = 0, n = triangles.Length; i < n; i += 3) {
                    int iv0 = triangles[i], iv1 = triangles[i + 1], iv2 = triangles[i + 2];
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

                    newTriangles[it0] = it0; newTriangles[it0 + 1] = it0 + 1; newTriangles[it0 + 2] = it0 + 2;
                    newTriangles[it1] = it1; newTriangles[it1 + 1] = it1 + 1; newTriangles[it1 + 2] = it1 + 2;
                    newTriangles[it2] = it2; newTriangles[it2 + 1] = it2 + 1; newTriangles[it2 + 2] = it2 + 2;
                    newTriangles[it3] = it3; newTriangles[it3 + 1] = it3 + 1; newTriangles[it3 + 2] = it3 + 2;
                }

                mesh.vertices = vertices;
            }

            mesh.indexFormat = mesh.vertexCount < 65535 ? IndexFormat.UInt16 : IndexFormat.UInt32;
            mesh.triangles = newTriangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        public void Dispose()
        {
            ReleaseBuffer(vertBuffer);
            ReleaseBuffer(edgeBuffer);
            ReleaseBuffer(triBuffer);
            ReleaseBuffer(subdivBuffer);
        }

        void ReleaseBuffer(ComputeBuffer buf)
        {
            if(buf != null) {
                buf.Release();
            }
        }

    }

    public struct Edge_t
    {
        public int v0, v1;

        // https://stackoverflow.com/questions/2542693/overriding-equals-method-in-structs
        public override bool Equals(object obj)
        {
            if (!(obj is Edge_t)) return false;

            var e = (Edge_t)obj;
            return Has(e.v0) && Has(e.v1);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Has(int iv)
        {
            return (v0 == iv) || (v1 == iv);
        }
    };

    public struct Triangle_t
    {
        public int v0, v1, v2;
        public int e0, e1, e2;
    };

}


