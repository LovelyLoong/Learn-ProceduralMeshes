using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Mathematics;
using static Unity.Mathematics.math;

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class AdvancedMultiStreamProceduralMesh : MonoBehaviour
{
    private void OnEnable()
    {
        var vertexAttributeCount = 4;
        var vertexCount = 4;
        var triangleIndexCount = 6;
        
        var meshDataArray = Mesh.AllocateWritableMeshData(1);
        var meshData = meshDataArray[0];
        
        var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(vertexAttributeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        vertexAttributes[0] = new VertexAttributeDescriptor( dimension:3);
        vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal,dimension:3,stream:1);
        vertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float16,4,2);
        vertexAttributes[3] = new VertexAttributeDescriptor( VertexAttribute.TexCoord0,VertexAttributeFormat.Float16,2,3);
        
        meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
        vertexAttributes.Dispose();

        var positions = meshData.GetVertexData<float3>();
        positions[0] = 0f;
        positions[1] = right();
        positions[2] = up();
        positions[3] = float3(1f,1f,0f);
        
        var normals = meshData.GetVertexData<float3>(1);
        normals[0] = back();
        normals[1] = back();
        normals[2] = back();
        normals[3] = back();
        
        var h0 = half(0f);
        var h1 = half(1f);
        
        var tangents = meshData.GetVertexData<half4>(2);
        tangents[0] = half4(h1,h0,h0,half(-1f));
        tangents[1] = half4(h1,h0,h0,half(-1f));
        tangents[2] = half4(h1,h0,h0,half(-1f));
        tangents[3] = half4(h1,h0,h0,half(-1f));
        
        var uvs = meshData.GetVertexData<half2>(3);
        uvs[0] = h0;
        uvs[1] = half2(h1,h0);
        uvs[2] = half2(h0,h1);
        uvs[3] = h1;
        
        meshData.SetIndexBufferParams(triangleIndexCount,IndexFormat.UInt16);
        var indices = meshData.GetIndexData<ushort>();
        indices[0] = 0;
        indices[1] = 2;
        indices[2] = 1;
        indices[3] = 1;
        indices[4] = 2;
        indices[5] = 3;

        var bounds = new Bounds(new Vector3(0.5f,0.5f),new Vector3(1f,1f)); 
        meshData.subMeshCount = 1;
        meshData.SetSubMesh(0, new SubMeshDescriptor(0, triangleIndexCount)
        {
            bounds = bounds,
            vertexCount = vertexCount,
        },MeshUpdateFlags.DontRecalculateBounds);
        
        var mesh = new Mesh
        {
            name = "Procedural Mesh",
            bounds = bounds,
        };
        
        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
        
        GetComponent<MeshFilter>().mesh = mesh;
    }
    
}
