using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

namespace Subdiv
{

    // - Sources:
    //      http://www.cs.cmu.edu/afs/cs/academic/class/15462-s14/www/lec_slides/Subdivision.pdf
    //      https://pages.mtu.edu/~shene/COURSES/cs3621/SLIDES/Subdivision.pdf
    //      https://graphics.stanford.edu/~mdfisher/subdivision.html
    public class SubdivisionSurface 
    {

        public static Mesh Subdivide(Mesh source, int details = 1, bool weld = false)
        {
            var model = Subdivide(source, details);
            var mesh = model.Build(weld);
            return mesh;
        }

        public static Model Subdivide(Mesh source, int details = 1)
        {
            var model = new Model(source);
            var divider = new SubdivisionSurface();

            for (int i = 0; i < details; i++) {
                model = divider.Divide(model);
            }

            return model;
        }

        public static Mesh Weld(Mesh mesh, float threshold, float bucketStep)
        {
            Vector3[] oldVertices = mesh.vertices;
            Vector3[] newVertices = new Vector3[oldVertices.Length];
            int[] old2new = new int[oldVertices.Length];
            int newSize = 0;

            // Find AABB
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            for (int i = 0; i < oldVertices.Length; i++)
            {
                if (oldVertices[i].x < min.x) min.x = oldVertices[i].x;
                if (oldVertices[i].y < min.y) min.y = oldVertices[i].y;
                if (oldVertices[i].z < min.z) min.z = oldVertices[i].z;
                if (oldVertices[i].x > max.x) max.x = oldVertices[i].x;
                if (oldVertices[i].y > max.y) max.y = oldVertices[i].y;
                if (oldVertices[i].z > max.z) max.z = oldVertices[i].z;
            }

            // Make cubic buckets, each with dimensions "bucketStep"
            int bucketSizeX = Mathf.FloorToInt((max.x - min.x) / bucketStep) + 1;
            int bucketSizeY = Mathf.FloorToInt((max.y - min.y) / bucketStep) + 1;
            int bucketSizeZ = Mathf.FloorToInt((max.z - min.z) / bucketStep) + 1;
            List<int>[,,] buckets = new List<int>[bucketSizeX, bucketSizeY, bucketSizeZ];

            // Make new vertices
            for (int i = 0; i < oldVertices.Length; i++)
            {
                // Determine which bucket it belongs to
                int x = Mathf.FloorToInt((oldVertices[i].x - min.x) / bucketStep);
                int y = Mathf.FloorToInt((oldVertices[i].y - min.y) / bucketStep);
                int z = Mathf.FloorToInt((oldVertices[i].z - min.z) / bucketStep);

                // Check to see if it's already been added
                if (buckets[x, y, z] == null) buckets[x, y, z] = new List<int>(); // Make buckets lazily

                for (int j = 0; j < buckets[x, y, z].Count; j++)
                {
                    Vector3 to = newVertices[buckets[x, y, z][j]] - oldVertices[i];
                    if (Vector3.SqrMagnitude(to) < threshold)
                    {
                        old2new[i] = buckets[x, y, z][j];
                        goto skip; // Skip to next old vertex if this one is already there
                    }
                }

                // Add new vertex
                newVertices[newSize] = oldVertices[i];
                buckets[x, y, z].Add(newSize);
                old2new[i] = newSize;
                newSize++;

                skip:;
            }

            // Make new triangles
            int[] oldTris = mesh.triangles;
            int[] newTris = new int[oldTris.Length];
            for (int i = 0; i < oldTris.Length; i++)
            {
                newTris[i] = old2new[oldTris[i]];
            }

            Vector3[] finalVertices = new Vector3[newSize];
            for (int i = 0; i < newSize; i++)
            {
                finalVertices[i] = newVertices[i];
            }

            mesh.Clear();
            mesh.vertices = finalVertices;
            mesh.triangles = newTris;
            mesh.RecalculateNormals();

            return mesh;
        }

        public Edge GetEdge(List<Edge> edges, Vertex v0, Vertex v1)
        {
            var match = v0.edges.Find(e => {
                return e.Has(v1);
            });
            if (match != null) return match;

            var ne = new Edge(v0, v1);
            v0.AddEdge(ne);
            v1.AddEdge(ne);
            edges.Add(ne);
            return ne;
        }

        Model Divide(Model model)
        {
            var nmodel = new Model();
            for (int i = 0, n = model.triangles.Count; i < n; i++)
            {
                var f = model.triangles[i];

                var ne0 = GetEdgePoint(f.e0);
                var ne1 = GetEdgePoint(f.e1);
                var ne2 = GetEdgePoint(f.e2);

                var nv0 = GetVertexPoint(f.v0);
                var nv1 = GetVertexPoint(f.v1);
                var nv2 = GetVertexPoint(f.v2);

                nmodel.AddTriangle(nv0, ne0, ne2);
                nmodel.AddTriangle(ne0, nv1, ne1);
                nmodel.AddTriangle(ne0, ne1, ne2);
                nmodel.AddTriangle(ne2, ne1, nv2);
            }
            return nmodel;
        }

        public Vertex GetEdgePoint(Edge e)
        {
            if (e.ept != null) return e.ept;

            if(e.faces.Count != 2) {
                // boundary case for edge
                var m = (e.a.p + e.b.p) * 0.5f;
                e.ept = new Vertex(m, e.a.index);
            } else
            {
                const float alpha = 3f / 8f;
                const float beta = 1f / 8f;
                var left = e.faces[0].GetOtherVertex(e);
                var right = e.faces[1].GetOtherVertex(e);
                e.ept = new Vertex((e.a.p + e.b.p) * alpha + (left.p + right.p) * beta, e.a.index);
            }

            return e.ept;
        }

        public Vertex[] GetAdjancies(Vertex v)
        {
            var adjancies = new Vertex[v.edges.Count];
            for(int i = 0, n = v.edges.Count; i < n; i++)
            {
                adjancies[i] = v.edges[i].GetOtherVertex(v);
            }
            return adjancies;
        }

        public Vertex GetVertexPoint(Vertex v)
        {
            if (v.updated != null) return v.updated;

            var adjancies = GetAdjancies(v);
            var n = adjancies.Length;
            if(n < 3)
            {
                // boundary case for vertex
                var e0 = v.edges[0].GetOtherVertex(v);
                var e1 = v.edges[1].GetOtherVertex(v);
                const float k0 = (3f / 4f);
                const float k1 = (1f / 8f);
                v.updated = new Vertex(k0 * v.p + k1 * (e0.p + e1.p), v.index);
            } else
            {
                const float pi2 = Mathf.PI * 2f;
                const float k0 = (5f / 8f);
                const float k1 = (3f / 8f);
                const float k2 = (1f / 4f);
                var alpha = (n == 3) ? (3f / 16f) : ((1f / n) * (k0 - Mathf.Pow(k1 + k2 * Mathf.Cos(pi2 / n), 2f)));

                var np = (1f - n * alpha) * v.p;

                for(int i = 0; i < n; i++)
                {
                    var adj = adjancies[i];
                    np += alpha * adj.p;
                }

                v.updated = new Vertex(np, v.index);
            }

            return v.updated;
        }

    }

}


