using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generator
{
    public struct UVSphere : IMeshGenerator
    {
        public int VertexCount => (Resolution + 1) * (Resolution + 1);

        public int IndexCount => 6 * Resolution * Resolution;

        public int JobLength => Resolution + 1;
        
        public int Resolution { get; set; }

        public void Execute<T>(int u, T streams) where T : IMeshStream
        {
            var vi = u * (Resolution + 1);
            var ti = (u - 1) * 2 * Resolution;

            var vertex = new Vertex();
            vertex.Normal.y = 1f;
            vertex.Tangent.xw = float2(1f, -1f);

            //这段代码的思想！ 对于(N + 1) * (N + 1)的问题怎么解决？ 我们每行单独看做一个Job 每一行有N+1个对象怎么解决？先找到头 设置头的相关数据 后续依次复用重复的内容达到最大程度的复用！
            vertex.Position.x = -0.5f;
            vertex.Position.z = (float)u / Resolution - 0.5f;
            vertex.UV.y = (float)u / Resolution;
            streams.SetVertex(vi,vertex);
            vi += 1;
            
            for (var v = 1; v <= Resolution; v++, vi++, ti += 2)
            {
                vertex.Position.x = (float)v / Resolution - 0.5f;
                vertex.UV.x = (float)v / Resolution;
                streams.SetVertex(vi,vertex);
                
                if (u > 0)      //这种写法会导致循环内部反复判断z变量的值吗？不会，Burst会帮助我们完成优化，它会发现z本质上在这个函数就是一个常量 所以它会写两个版本的循环 一个是带设置三角形的部分 一个是不带 然后通过一次检测决定后续逻辑使用哪个部分版本！也就是说不会重复判断！
                {
                    streams.SetTriangle(ti + 0,vi + new int3(-Resolution-2, -1, -Resolution - 1));
                    streams.SetTriangle(ti + 1,vi + new int3(-Resolution - 1,-1, 0));
                }
            }
            
        }

        public Bounds Bounds => new(Vector3.zero, new Vector3(2f,2f,2f));
    }
}