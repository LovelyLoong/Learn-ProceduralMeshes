using ProceduralMeshes.Streams;
using Unity.Mathematics;
using UnityEngine;

namespace ProceduralMeshes
{
    public interface IMeshStream
    {
        void SetUp(Mesh.MeshData data,Bounds bounds,int vertexCount,int indexCount);

        void SetVertex(int index, Vertex vertex);
        
        void SetTriangle(int index, int3 triangle);
    }
}