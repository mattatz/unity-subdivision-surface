using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Subdiv
{

    struct Edge_t
    {
        public int v0, v1;

        // https://stackoverflow.com/questions/2542693/overriding-equals-method-in-structs
        public override bool Equals(object obj)
        {
            if (!(obj is Edge_t)) return false;

            var e = (Edge_t)obj;
            return Has(e.v0) && Has(e.v1);
        }

        public bool Has(int iv)
        {
            return (v0 == iv) || (v1 == iv);
        }
    };

    struct Triangle_t
    {
        public int v0, v1, v2;
        public int e0, e1, e2;
    };

}


