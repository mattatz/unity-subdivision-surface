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
        public List<Triangle> triangles;

        public Model()
        {
            this.vertices = new List<Vertex>();
            this.edges = new List<Edge>();
            this.triangles = new List<Triangle>();
        }

        public Model(Mesh source) : this()
        {
            var points = source.vertices;
            for (int i = 0, n = points.Length; i < n; i++)
            {
                var v = new Vertex(points[i], i);
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
                var f = new Triangle(v0, v1, v2, e0, e1, e2);

                this.triangles.Add(f);
                v0.AddTriangle(f); v1.AddTriangle(f); v2.AddTriangle(f);
                e0.AddTriangle(f); e1.AddTriangle(f); e2.AddTriangle(f);
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

        public void AddTriangle(Vertex v0, Vertex v1, Vertex v2)
        {
            if (!vertices.Contains(v0)) vertices.Add(v0);
            if (!vertices.Contains(v1)) vertices.Add(v1);
            if (!vertices.Contains(v2)) vertices.Add(v2);

            var e0 = GetEdge(v0, v1);
            var e1 = GetEdge(v1, v2);
            var e2 = GetEdge(v2, v0);
            var f = new Triangle(v0, v1, v2, e0, e1, e2);

            this.triangles.Add(f);
            v0.AddTriangle(f); v1.AddTriangle(f); v2.AddTriangle(f);
            e0.AddTriangle(f); e1.AddTriangle(f); e2.AddTriangle(f);
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
            var triangles = new int[this.triangles.Count * 3];

            if(weld)
            {
                for (int i = 0, n = this.triangles.Count; i < n; i++)
                {
                    var f = this.triangles[i];
                    triangles[i * 3] = vertices.IndexOf(f.v0);
                    triangles[i * 3 + 1] = vertices.IndexOf(f.v1);
                    triangles[i * 3 + 2] = vertices.IndexOf(f.v2);
                }
                mesh.vertices = vertices.Select(v => v.p).ToArray();
            } else
            {
                var vertices = new Vector3[this.triangles.Count * 3];
                for (int i = 0, n = this.triangles.Count; i < n; i++)
                {
                    var f = this.triangles[i];
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

