using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;
using float2 = Unity.Mathematics.float2;

namespace ProceduralMeshes.Generator
{
    public struct PointyHexagonGrid : IMeshGenerator
    {
        public int VertexCount => 7 * Resolution * Resolution;      //网格总共有7个顶点

        public int IndexCount => 18 * Resolution * Resolution;      //网格总共有6个三角形  每个三角形有3个顶点 所以这里一个的是18个顶点

        public int JobLength => Resolution;
        
        public int Resolution { get; set; }

        public void Execute<T>(int u, T streams) where T : struct,IMeshStreams
        {
            var vi = u * 7 * Resolution;
            var ti = u * 6 * Resolution;

            //六边形网格中每个小三角形的高度：
            var h = sqrt(3f) / 4;  //为什么是sqrt(3)/4 不是sqrt(3)/2？因为这个地方考虑的是希望整体的六边形和正方形网格大小类似！所以将六边形中三角形的边长变为0.5=

            var centerOffset = float2.zero;     //首先centerOffset的核心目的是什么？是让生成的内容居中！那么怎么居中？让每个点减去宽高总长的一般即可！这样就仿佛整体把图片按宽高的一半往回拉一样！
            if (Resolution > 1)
            {
                centerOffset.x = (((u & 1) == 0 ? 0.5f : 1.5f) - Resolution) * h;       //原本公式：每个点 +-h -((r -1)* 2h)/2  
                centerOffset.y = -0.375f * (Resolution - 1);        //这里为什么是0.375f？  (0.75 *(r-1))/2
            }

            for (var x = 0; x < Resolution; x++ , vi += 7, ti += 6)     //编译的时候Burst首先会自动优化一遍 检查循环内部是否有完全不会变更的内容 然后将这部分内容直接移出循环！
            {
                var center = (float2(2 * h * x ,  0.75f * u ) + centerOffset) / Resolution;     //注意当你将Y轴的网格合并的时候 整体网格的高度都会变化！所以这里的Z会乘以0.75f
                var xCoordinates = center.x + float2(-h, h) / Resolution;
                var zCoordinates = center.y + float4(-0.5f,-0.25f,0.25f,0.5f) / Resolution;
                
                var vertex = new Vertex();
                vertex.Position.xz = center;
                vertex.Normal.y = 1f;
                vertex.Tangent.xw = float2(1f, -1f);
                vertex.UV = 0.5f;
                streams.SetVertex(vi + 0,vertex);
                
                vertex.Position.z = zCoordinates.x;
                vertex.UV.y = 0f;
                streams.SetVertex(vi + 1,vertex);
                
                vertex.Position.x = xCoordinates.x;
                vertex.Position.z = zCoordinates.y;
                vertex.UV = float2(0.5f - h,0.25f);
                streams.SetVertex(vi + 2,vertex);
                
                vertex.Position.z = zCoordinates.z;
                vertex.UV.y = 0.75f;
                streams.SetVertex(vi + 3,vertex);
                
                vertex.Position.x = center.x;
                vertex.Position.z = zCoordinates.w;
                vertex.UV = float2(0.5f,1f);
                streams.SetVertex(vi + 4,vertex);
                
                vertex.Position.x = xCoordinates.y;
                vertex.Position.z = zCoordinates.z;
                vertex.UV = float2(0.5f + h,0.75f);
                streams.SetVertex(vi + 5,vertex);
                
                vertex.Position.z = zCoordinates.y;
                vertex.UV.y = 0.25f;
                streams.SetVertex(vi + 6,vertex);
                
                streams.SetTriangle(ti + 0,vi + int3(0,1,2));
                streams.SetTriangle(ti + 1,vi + int3(0,2,3));
                streams.SetTriangle(ti + 2,vi + int3(0,3,4));
                streams.SetTriangle(ti + 3,vi + int3(0,4,5));
                streams.SetTriangle(ti + 4,vi + int3(0,5,6));
                streams.SetTriangle(ti + 5,vi + int3(0,6,1));
                
            }
            
        }

        public Bounds Bounds => new(Vector3.zero, new Vector3(
            (Resolution > 1 ? 0.5f + 0.25f / Resolution : 0.5f) * sqrt(3f)
            ,0f
            ,0.75f + 0.25f / Resolution));       //边界推导  高度：((R - 1) * 0.75 + 1) / R  宽度：2 * h * R + h / R
    }
}