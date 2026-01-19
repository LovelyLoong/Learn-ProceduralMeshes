using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generator
{
    public struct SharedTriangleGrid : IMeshGenerator
    {
        public int VertexCount => (Resolution + 1) * (Resolution + 1);

        public int IndexCount => 6 * Resolution * Resolution;

        public int JobLength => Resolution + 1;
        
        public int Resolution { get; set; }

        public void Execute<T>(int u, T streams) where T : struct,IMeshStreams
        {
            var vi = u * (Resolution + 1);
            var ti = (u - 1) * 2 * Resolution;

            var vertex = new Vertex();
            vertex.Normal.y = 1f;
            vertex.Tangent.xw = float2(1f, -1f);

            var xOffset = -0.25f;       //这里让X做了一个额外偏移 这样可以比较方便的实现中心生成网格的效果 同时将正方向的网格转变为菱形
            var uOffset = 0f;
            var iA = -Resolution - 2;
            var iB = -Resolution - 1;
            var iC = -1;
            var iD = 0;
            var tA = int3(iA, iC, iD);
            var tB = int3(iD, iB, iA);

            if ((u & 1) == 1)
            {
                xOffset = 0.25f;
                uOffset = 0.5f / (Resolution + 0.5f);
                tA = int3(iA, iC, iB);
                tB = int3(iB, iC, iD);
            }
            
            xOffset = xOffset / Resolution - 0.5f;
            
            //这段代码的思想！ 对于(N + 1) * (N + 1)的问题怎么解决？ 我们每行单独看做一个Job 每一行有N+1个对象怎么解决？先找到头 设置头的相关数据 后续依次复用重复的内容达到最大程度的复用！
            vertex.Position.x = xOffset;
            vertex.Position.z = ((float)u / Resolution - 0.5f) * sqrt(3f) * 0.5f;
            vertex.UV.y = vertex.Position.z / (1 + 0.5f / Resolution) + 0.5f;
            vertex.UV.x = uOffset;
            streams.SetVertex(vi,vertex);
            vi += 1;
            
            for (var x = 1; x <= Resolution; x++, vi++, ti += 2)
            {
                vertex.Position.x = (float)x / Resolution + xOffset;
                vertex.UV.x = x / (Resolution + 0.5f) + uOffset;
                streams.SetVertex(vi,vertex);
                
                if (u > 0)      //这种写法会导致循环内部反复判断z变量的值吗？不会，Burst会帮助我们完成优化，它会发现z本质上在这个函数就是一个常量 所以它会写两个版本的循环 一个是带设置三角形的部分 一个是不带 然后通过一次检测决定后续逻辑使用哪个部分版本！也就是说不会重复判断！
                {
                    streams.SetTriangle(ti + 0,vi + tA);
                    streams.SetTriangle(ti + 1,vi + tB);
                }
            }
            
        }

        public Bounds Bounds => new(Vector3.zero, new Vector3(1+ 0.5f / Resolution,0f,sqrt(3) * 0.5f));
    }
}