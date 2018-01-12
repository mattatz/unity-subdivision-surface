using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Subdiv
{

    public class Edge
    {
        public Vertex a, b;
        public List<Face> faces;
        public Vertex ept;

        public Edge(Vertex a, Vertex b)
        {
            this.a = a;
            this.b = b;
            this.faces = new List<Face>();
        }

        public void AddFace(Face f)
        {
            faces.Add(f);
        }

        public bool Has(Vertex v)
        {
            return v == a || v == b;
        }

        public Vertex GetOtherVertex(Vertex v)
        {
            if (a != v) return a;
            return b;
        }
    }

}

