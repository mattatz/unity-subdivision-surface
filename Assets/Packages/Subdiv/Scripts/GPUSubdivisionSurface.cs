using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.Rendering;

namespace Subdiv
{

    public class GPUSubdivisionSurface {

        #region Compute shader keys

        protected const string kKernelKey = "Subdivide";
        protected const string kVertexBufferKey = "_VertexBuffer", kEdgeBufferKey = "_EdgeBuffer", kTriangleBufferKey = "_TriangleBuffer";
        protected const string kVertexCountKey = "_VertexCount", kEdgeCountKey = "_EdgeCount", kTriangleCountKey = "_TriangleCount";
        protected const string kSubdivBufferKey = "_SubdivBuffer", kSubdivCountKey = "_SubdivCount"; 

        #endregion

        // subdivCompute : SubdivisionSurface.compute
        public static Mesh Subdivide(ComputeShader subdivCompute, Mesh mesh, int details = 1, bool weld = false)
        {
            var kernel = new Kernel(subdivCompute, kKernelKey);

            var data = new GPUSubdivData(mesh);

            for (int i = 0; i < details; i++) {
                subdivCompute.SetBuffer(kernel.Index, kVertexBufferKey, data.VertexBuffer);
                subdivCompute.SetBuffer(kernel.Index, kEdgeBufferKey, data.EdgeBuffer);
                subdivCompute.SetBuffer(kernel.Index, kTriangleBufferKey, data.TriangleBuffer);
                subdivCompute.SetBuffer(kernel.Index, kSubdivBufferKey, data.SubdivBuffer);
                subdivCompute.SetInt(kVertexCountKey, data.VertexBuffer.count);
                subdivCompute.SetInt(kEdgeCountKey, data.EdgeBuffer.count);
                subdivCompute.SetInt(kTriangleCountKey, data.TriangleBuffer.count);
                subdivCompute.SetInt(kSubdivCountKey, data.SubdivBuffer.count);

                subdivCompute.Dispatch(kernel.Index, data.SubdivBuffer.count / (int)kernel.ThreadX + 1, (int)kernel.ThreadY, (int)kernel.ThreadZ);

                if(i != details - 1) {
                    data = data.Next();
                }
            }

            mesh = data.Build(weld);
            data.Dispose();

            return mesh;
        }

    }

}


