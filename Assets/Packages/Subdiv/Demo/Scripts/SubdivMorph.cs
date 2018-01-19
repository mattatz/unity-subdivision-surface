using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.Rendering;

namespace Subdiv.Demo
{

    struct MVertex_t
    {
        Vector3 position, normal;
        public MVertex_t (Vector3 p, Vector3 n)
        {
            position = p;
            normal = n;
        }
    };

    [RequireComponent (typeof(MeshFilter), typeof(MeshRenderer))]
    public class SubdivMorph : MonoBehaviour {

        [SerializeField, Range(1, 3)] protected int details = 2;
        [SerializeField] protected Vector3 noise = new Vector3(1f, 1f, 1f);
        [SerializeField] protected float speed = 1f;

        protected ComputeBuffer buffer;
        new protected Renderer renderer;
        protected MaterialPropertyBlock block;

        protected const string kBufferKey = "_VertexBuffer";
        protected const string kNoiseParamsKey = "_NoiseParams";

        void Start()
        {
            var filter = GetComponent<MeshFilter>();
            var source = filter.mesh;

            var model = SubdivisionSurface.Subdivide(SubdivisionSurface.Weld(source, float.Epsilon, source.bounds.size.x), details);
            Setup(filter, source, model);
        }

        void Update()
        {
            block.SetVector(kNoiseParamsKey, new Vector4(noise.x, noise.y, noise.z, Time.timeSinceLevelLoad * speed));
            renderer.SetPropertyBlock(block);
        }

        void Setup(MeshFilter filter, Mesh source, Model model)
        {
            var inVertices = source.vertices;
            var inNormals = source.normals;

            var mesh = new Mesh();
            var triangles = new int[model.triangles.Count * 3];
            var vertices = new Vector3[model.triangles.Count * 3];

            buffer = new ComputeBuffer(vertices.Length, Marshal.SizeOf(typeof(MVertex_t)));
            var mvertices = new MVertex_t[buffer.count];

            for (int i = 0, n = model.triangles.Count; i < n; i++)
            {
                var f = model.triangles[i];
                int i0 = i * 3, i1 = i * 3 + 1, i2 = i * 3 + 2;
                vertices[i0] = f.v0.p;
                vertices[i1] = f.v1.p;
                vertices[i2] = f.v2.p;
                triangles[i0] = i0;
                triangles[i1] = i1;
                triangles[i2] = i2;

                mvertices[i0] = new MVertex_t(inVertices[f.v0.index], inNormals[f.v0.index]);
                mvertices[i1] = new MVertex_t(inVertices[f.v1.index], inNormals[f.v1.index]);
                mvertices[i2] = new MVertex_t(inVertices[f.v2.index], inNormals[f.v2.index]);
            }
            mesh.vertices = vertices;
            mesh.indexFormat = mesh.vertexCount < 65535 ? IndexFormat.UInt16 : IndexFormat.UInt32;
            mesh.triangles = triangles;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            buffer.SetData(mvertices);
            filter.sharedMesh = mesh;

            block = new MaterialPropertyBlock();
            renderer = GetComponent<Renderer>();
            renderer.GetPropertyBlock(block);
            block.SetBuffer(kBufferKey, buffer);
            renderer.SetPropertyBlock(block);
        }

        void OnDestroy()
        {
            if(buffer != null)
            {
                buffer.Release();
                buffer = null;
            }
        }

    }

}

