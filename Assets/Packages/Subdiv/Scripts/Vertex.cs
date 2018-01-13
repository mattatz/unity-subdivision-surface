using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Subdiv
{

    public class Vertex
    {
        public Vector3 p;
        public List<Edge> edges;
        public List<Triangle> triangles;
        public Vertex updated;

        public Vertex(Vector3 p)
        {
            this.p = p;
            this.edges = new List<Edge>();
            this.triangles = new List<Triangle>();
        }

        public void AddEdge(Edge e)
        {
            edges.Add(e);
        }

        public void AddTriangle(Triangle f)
        {
            triangles.Add(f);
        }

    }

}

