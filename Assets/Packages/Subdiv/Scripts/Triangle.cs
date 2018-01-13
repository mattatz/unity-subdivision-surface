using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Subdiv
{

    public class Triangle
    {
        public Vertex v0, v1, v2;
        public Edge e0, e1, e2;

        public Triangle(
            Vertex v0, Vertex v1, Vertex v2,
            Edge e0, Edge e1, Edge e2
        )
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;

            this.e0 = e0;
            this.e1 = e1;
            this.e2 = e2;
        }

        public Vertex GetOtherVertex(Edge e)
        {
            if (!e.Has(v0)) return v0;
            else if (!e.Has(v1)) return v1;
            return v2;
        }

    }

}

