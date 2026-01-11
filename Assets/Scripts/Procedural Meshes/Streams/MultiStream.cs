using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProceduralMeshes.Streams
{
    public struct MultiStream : IMeshStream
    {
        
        [NativeDisableContainerSafetyRestriction]
        NativeArray<float3> stream0;  //vertex Pos
        
        [NativeDisableContainerSafetyRestriction]
        NativeArray<float3> stream1;  //vertex Normal
        
        [NativeDisableContainerSafetyRestriction]
        NativeArray<float4> stream2;  //vertex Tangent
        
        [NativeDisableContainerSafetyRestriction]
        NativeArray<float2> stream3;  //vertex UV
        
        [NativeDisableContainerSafetyRestriction]
        NativeArray<TriangleUInt16> m_triangleStream;
        
        public void SetUp(Mesh.MeshData data, Bounds bounds, int vertexCount, int indexCount)
        {
            var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            vertexAttributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position,dimension:3);
            vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal,dimension:3,stream:1);
            vertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent,dimension:4,stream:2);
            vertexAttributes[3] = new VertexAttributeDescriptor( VertexAttribute.TexCoord0,dimension:2,stream:3);
            data.SetVertexBufferParams(vertexCount, vertexAttributes);      //顶点数量 顶点的属性
            vertexAttributes.Dispose();
            
            data.SetIndexBufferParams(indexCount, IndexFormat.UInt16);      //三角形下标数量
            data.subMeshCount = 1;
            data.SetSubMesh(0, new SubMeshDescriptor(0, indexCount)
                {
                    bounds = bounds,
                    vertexCount = vertexCount,
                    indexCount = indexCount,
                }, MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);       //第一次进入由于没有执行Job所以不可能有有效数据 所以禁止Bounds验证和下标验证

            stream0 = data.GetVertexData<float3>();
            stream1 = data.GetVertexData<float3>(1);
            stream2 = data.GetVertexData<float4>(2);
            stream3 = data.GetVertexData<float2>(3);
            
            m_triangleStream = data.GetIndexData<ushort>().Reinterpret<TriangleUInt16>(2);  
            //这里为什么要写4？Reinterpret这个API有规定 如果需要解释的新类型和源类型的大小不一样
            //一定要强制明确原大小的字节大小 ，然后用这个大小计算原容器的大小 / 新类型的大小 == 新容器的数量。
            //而且这这个地方必须要写，如果不写Unity内部元素报错。
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]      //设置函数的导入编译方式的 使用激进内联策略 为什么？因为默认情况下一些复杂的类型转换Burst不会将其选择内联 而是选择函数调用的方式来使用！
        public void SetVertex(int index, Vertex vertex)
        {
            stream0[index] = vertex.Position;
            stream1[index] = vertex.Normal;
            stream2[index] = vertex.Tangent;
            stream3[index] = vertex.UV;
        }

        public void SetTriangle(int index, int3 triangle)
        {
            m_triangleStream[index] = triangle;
        }
        
    }
}