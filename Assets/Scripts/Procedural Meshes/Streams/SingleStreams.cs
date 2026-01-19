using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProceduralMeshes.Streams
{
    public struct SingleStreams : IMeshStreams
    {
        [StructLayout(LayoutKind.Sequential)]
        struct Stream0
        {
            public float3 Position;
            public float3 Normal;
            public float4 Tangle;
            public float2 UV0;
        }
        
        [NativeDisableContainerSafetyRestriction]
        NativeArray<Stream0> m_VertexStream;
        
        [NativeDisableContainerSafetyRestriction]
        NativeArray<TriangleUInt16> m_triangleStream;
        
        public void SetUp(Mesh.MeshData data, Bounds bounds, int vertexCount, int indexCount)
        {
            var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            vertexAttributes[0] = new VertexAttributeDescriptor(VertexAttribute.Position,dimension:3);
            vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal,dimension:3);
            vertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent,dimension:4);
            vertexAttributes[3] = new VertexAttributeDescriptor( VertexAttribute.TexCoord0,dimension:2);
            data.SetVertexBufferParams(vertexCount, vertexAttributes);      //顶点数量 顶点的属性
            vertexAttributes.Dispose();
            
            data.SetIndexBufferParams(indexCount, IndexFormat.UInt16);      //三角形下标数量
            data.subMeshCount = 1;
            data.SetSubMesh(0, new SubMeshDescriptor(0, indexCount)
                {
                    bounds = bounds,
                    vertexCount = vertexCount,
                    indexCount = indexCount,
                }, 
                MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);       //第一次进入由于没有执行Job所以不可能有有效数据 所以禁止Bounds验证和下标验证

            m_VertexStream = data.GetVertexData<Stream0>();
            m_triangleStream = data.GetIndexData<ushort>().Reinterpret<TriangleUInt16>(2);  
            //这里为什么要写4？Reinterpret这个API有规定 如果需要解释的新类型和源类型的大小不一样
            //一定要强制明确原大小的字节大小 ，然后用这个大小计算原容器的大小 / 新类型的大小 == 新容器的数量。所以这里的数量本质上就是强调一下原本容器的大小即可！
            //而且这这个地方必须要写，如果不写Unity内部元素报错。
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]      //设置函数的导入编译方式的 使用激进内联策略 为什么？因为默认情况下一些复杂的类型转换Burst不会将其选择内联 而是选择函数调用的方式来使用！
        public void SetVertex(int index, Vertex vertex)
        {
            m_VertexStream[index] = new Stream0     //这里有一个隐式的优化点：为什么这里不直接使用Vertex结构？只需要将Vertex结构设置布局结构就可以了，但是出于灵活性的考虑这里没有直接使用Vertex。
            {                                       //假如这里直接使用Vertex会发生什么？你每次往Vertex中加一个数据，那么所有IMeshStream的实现类都可能因此要修改代码！这对于灵活开发的项目来说是极其糟糕的！
                Position = vertex.Position,         //这是一个很好中介的思想！确保只保留我们需要的部分即可！ 而且Burst编译器的优化工作 可以让我们在实机代码上几乎没有任何额外的消耗！
                Normal = vertex.Normal,
                Tangle = vertex.Tangent,
                UV0 = vertex.UV,
            };
        }

        public void SetTriangle(int index, int3 triangle)
        {
            m_triangleStream[index] = triangle;
        }
        
    }
}