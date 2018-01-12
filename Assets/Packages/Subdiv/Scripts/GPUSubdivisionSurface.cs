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

        public Mesh Subdivide(Mesh mesh, int details = 1, bool weld = false)
        {
            var kernel = new Kernel(subdivCompute, "Subdivide");

            for (int i = 0; i < details; i++) {
                var data = new GPUSubdivData(mesh);

                subdivCompute.SetBuffer(kernel.Index, "_VertexBuffer", data.VertexBuffer);
                subdivCompute.SetBuffer(kernel.Index, "_EdgeBuffer", data.EdgeBuffer);
                subdivCompute.SetBuffer(kernel.Index, "_TriangleBuffer", data.TriangleBuffer);
                subdivCompute.SetBuffer(kernel.Index, "_SubdivBuffer", data.SubdivBuffer);
                subdivCompute.SetInt("_VertexCount", data.VertexBuffer.count);
                subdivCompute.SetInt("_EdgeCount", data.EdgeBuffer.count);
                subdivCompute.SetInt("_TriangleCount", data.TriangleBuffer.count);
                subdivCompute.SetInt("_SubdivCount", data.SubdivBuffer.count);

                subdivCompute.Dispatch(kernel.Index, data.SubdivBuffer.count / (int)kernel.ThreadX + 1, (int)kernel.ThreadY, (int)kernel.ThreadZ);

                // CAUTION:
                //      Need to build a weld mesh to subdivide in next detail
                mesh = data.Build((i != details - 1) || weld);
                data.Dispose();
            }

            return mesh;
        }

    }

}


