using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Subdiv
{

    public class Vertex
    {
        public Vector3 p;
        public List<Edge> edges;
        public List<Face> faces;
        public Vertex updated;

        public Vertex(Vector3 p)
        {
            this.p = p;
            this.edges = new List<Edge>();
            this.faces = new List<Face>();
        }

        public void AddEdge(Edge e)
        {
            edges.Add(e);
        }

        public void AddFace(Face f)
        {
            faces.Add(f);
        }

    }

}

