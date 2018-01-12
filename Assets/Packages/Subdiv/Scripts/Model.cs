using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

namespace Subdiv
{

    public class Model
    {
        List<Vertex> vertices;
        List<Edge> edges;
        public List<Face> faces;

        public Model()
        {
            this.vertices = new List<Vertex>();
            this.edges = new List<Edge>();
            this.faces = new List<Face>();
        }

        public Model(Mesh source) : this()
        {
            var points = source.vertices;
            for (int i = 0, n = points.Length; i < n; i++)
            {
                var v = new Vertex(points[i]);
                vertices.Add(v);
            }

            var triangles = source.triangles;
            for (int i = 0, n = triangles.Length; i < n; i += 3)
            {
                int i0 = triangles[i], i1 = triangles[i + 1], i2 = triangles[i + 2];
                Vertex v0 = vertices[i0], v1 = vertices[i1], v2 = vertices[i2];

                var e0 = GetEdge(edges, v0, v1);
                var e1 = GetEdge(edges, v1, v2);
                var e2 = GetEdge(edges, v2, v0);
                var f = new Face(v0, v1, v2, e0, e1, e2);

                faces.Add(f);
                v0.AddFace(f); v1.AddFace(f); v2.AddFace(f);
                e0.AddFace(f); e1.AddFace(f); e2.AddFace(f);
            }
        }

        Edge GetEdge(List<Edge> edges, Vertex v0, Vertex v1)
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

        public void AddFace(Vertex v0, Vertex v1, Vertex v2)
        {
            if (!vertices.Contains(v0)) vertices.Add(v0);
            if (!vertices.Contains(v1)) vertices.Add(v1);
            if (!vertices.Contains(v2)) vertices.Add(v2);

            var e0 = GetEdge(v0, v1);
            var e1 = GetEdge(v1, v2);
            var e2 = GetEdge(v2, v0);
            var f = new Face(v0, v1, v2, e0, e1, e2);

            this.faces.Add(f);
            v0.AddFace(f); v1.AddFace(f); v2.AddFace(f);
            e0.AddFace(f); e1.AddFace(f); e2.AddFace(f);
        }

        Edge GetEdge(Vertex v0, Vertex v1)
        {
            var match = v0.edges.Find(e =>
            {
                return (e.a == v1 || e.b == v1);
            });
            if (match != null) return match;

            var ne = new Edge(v0, v1);
            edges.Add(ne);
            v0.AddEdge(ne);
            v1.AddEdge(ne);
            return ne;
        }

        public Mesh Build(bool weld = false)
        {
            var mesh = new Mesh();
            var triangles = new int[faces.Count * 3];

            if(weld)
            {
                for (int i = 0, n = faces.Count; i < n; i++)
                {
                    var f = faces[i];
                    triangles[i * 3] = vertices.IndexOf(f.v0);
                    triangles[i * 3 + 1] = vertices.IndexOf(f.v1);
                    triangles[i * 3 + 2] = vertices.IndexOf(f.v2);
                }
                mesh.vertices = vertices.Select(v => v.p).ToArray();
            } else
            {
                var vertices = new Vector3[faces.Count * 3];
                for (int i = 0, n = faces.Count; i < n; i++)
                {
                    var f = faces[i];
                    int i0 = i * 3, i1 = i * 3 + 1, i2 = i * 3 + 2;
                    vertices[i0] = f.v0.p;
                    vertices[i1] = f.v1.p;
                    vertices[i2] = f.v2.p;
                    triangles[i0] = i0;
                    triangles[i1] = i1;
                    triangles[i2] = i2;
                }
                mesh.vertices = vertices;
            }

            mesh.indexFormat = mesh.vertexCount < 65535 ? IndexFormat.UInt16 : IndexFormat.UInt32;
            mesh.triangles = triangles;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

    }

}

