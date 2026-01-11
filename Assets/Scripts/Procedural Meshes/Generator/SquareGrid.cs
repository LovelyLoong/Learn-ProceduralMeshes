using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generator
{
    public struct SquareGrid : IMeshGenerator
    {
        public int VertexCount => 4 * Resolution * Resolution;

        public int IndexCount => 6 * Resolution * Resolution;

        public int JobLength => Resolution;
        
        public int Resolution { get; set; }

        public void Execute<T>(int u, T streams) where T : IMeshStream
        {
            var vi = u * 4 * Resolution;
            var ti = u * 2 * Resolution;
            
            for (var x = 0; x < Resolution; x++ , vi += 4, ti += 2)     //编译的时候Burst首先会自动优化一遍 检查循环内部是否有完全不会变更的内容 然后将这部分内容直接移出循环！
            {
                var xCoordinates = float2(x,x+1f)  - 0.5f * Resolution ;     //这个地方使用了两次除法 且都在除以相同的内容！是不是可以使用invResolution 来进行优化？
                var zCoordinates = float2(u,u+1f)  - 0.5f * Resolution ;     //由于我们Burst设置为了Fast所以Burst会自动将所有除法转换为乘法！同时优化只执行一次除法运算！剩下的都是乘法运算！
                
                var vertex = new Vertex();
                vertex.Normal.y = 1f;
                vertex.Tangent.xw = float2(1f, -1f);
                
                vertex.Position.x = xCoordinates.x;
                vertex.Position.z = zCoordinates.x;
                vertex.UV = 0f;
                streams.SetVertex(vi + 0, vertex);
                
                vertex.Position.x = xCoordinates.y;
                vertex.UV = float2(1f, 0f);
                streams.SetVertex(vi + 1, vertex);
                
                vertex.Position.x = xCoordinates.x;
                vertex.Position.z = zCoordinates.y;
                vertex.UV = float2(0f, 1f);
                streams.SetVertex(vi + 2, vertex);
                
                vertex.Position.x = xCoordinates.y;
                vertex.UV = 1f;
                streams.SetVertex(vi + 3, vertex);
            
                streams.SetTriangle(ti + 0,vi +  new int3(0, 2, 1));
                streams.SetTriangle(ti + 1,vi +  new int3(1, 2, 3));
            }
            
        }

        public Bounds Bounds => new(Vector3.zero, new Vector3(1f,0f,1f));
    }
}