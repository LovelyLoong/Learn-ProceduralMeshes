using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using half2 = Unity.Mathematics.half2;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AdvancedSingleStreamProceduralMesh : MonoBehaviour
{
    private void OnEnable()
    {
        var vertexAttributeCount = 4;
        var vertexCount = 4;
        var triangleIndexCount = 6;

        var meshDataArray = Mesh.AllocateWritableMeshData(1);
        var meshData = meshDataArray[0];

        var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(vertexAttributeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        vertexAttributes[0] = new VertexAttributeDescriptor(dimension: 3);
        vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, dimension: 3);
        vertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float16, 4);
        vertexAttributes[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2);

        meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
        vertexAttributes.Dispose();

        var vertices = meshData.GetVertexData<Vertex>();

        var h0 = half(0f);
        var h1 = half(1f);
        var vertex = new Vertex()
        {
            normal = back(),
            tangent = half4(h1, h0, h0, half(-1f)),
        };

        vertex.position = 0f;
        vertex.uv = h0;
        vertices[0] = vertex;

        vertex.position = right();
        vertex.uv = half2(h1, h0);
        vertices[1] = vertex;

        vertex.position = up();
        vertex.uv = half2(h0, h1);
        vertices[2] = vertex;

        vertex.position = float3(1f, 1f, 0f);
        vertex.uv = h1;
        vertices[3] = vertex;


        meshData.SetIndexBufferParams(triangleIndexCount, IndexFormat.UInt16);
        var indices = meshData.GetIndexData<ushort>();
        indices[0] = 0;
        indices[1] = 2;
        indices[2] = 1;
        indices[3] = 1;
        indices[4] = 2;
        indices[5] = 3;

        var bounds = new Bounds(new Vector3(0.5f, 0.5f), new Vector3(1f, 1f));
        meshData.subMeshCount = 1;
        meshData.SetSubMesh(0, new SubMeshDescriptor(0, triangleIndexCount)
        {
            bounds = bounds,
            vertexCount = vertexCount,
        }, MeshUpdateFlags.DontRecalculateBounds);

        var mesh = new Mesh
        {
            name = "Procedural Mesh",
            bounds = bounds,
        };

        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);

        GetComponent<MeshFilter>().mesh = mesh;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct Vertex
    {
        public float3 position;
        public float3 normal;
        public half4 tangent;
        public half2 uv;
    }
}