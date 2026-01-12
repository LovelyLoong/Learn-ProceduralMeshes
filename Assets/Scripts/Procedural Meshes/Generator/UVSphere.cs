using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generator
{
    public struct UVSphere : IMeshGenerator
    {
        private int ResolutionV => 2 * Resolution;
        private int ResolutionU => 4 * Resolution;
        
        public int VertexCount => (ResolutionU + 1) * (ResolutionV + 1);

        public int IndexCount => 6 * ResolutionU * ResolutionV;

        public int JobLength => ResolutionU + 1;
        
        public int Resolution { get; set; }

        public void Execute<T>(int u, T streams) where T : IMeshStream
        {
            var vi = u * (ResolutionV + 1);
            var ti = (u - 1) * 2 * ResolutionV;

            var vertex = new Vertex();
            // vertex.Normal.z = -1f;
            // vertex.Tangent.xw = float2(1f, -1f);
            vertex.Normal.y = -1f;
            vertex.Position.y = -1f;
            vertex.Tangent.w = -1f;

            //这段代码的思想！ 对于(N + 1) * (N + 1)的问题怎么解决？ 我们每行单独看做一个Job 每一行有N+1个对象怎么解决？先找到头 设置头的相关数据 后续依次复用重复的内容达到最大程度的复用！
            // vertex.Position.x = sin(2f * PI * u / Resolution);      //2f * PI * u / Resolution  其实本质就是这个X对于圆心的夹角的弧度！
            // vertex.Position.z = -cos(2f * PI * u / Resolution);
            // vertex.Normal = vertex.Position;
            float2 circle;
            circle.x = sin(2f * PI * u / ResolutionU);
            circle.y = cos(2f * PI * u / ResolutionU);
            vertex.Tangent.xz = circle.yx;
            circle.y = -circle.y;
            
            // vertex.Tangent.x = -vertex.Normal.z;
            // vertex.Tangent.z = vertex.Normal.x;
            
            vertex.UV.x = (float)u / ResolutionU;
            streams.SetVertex(vi,vertex);
            vi ++;
            
            for (var v = 1; v <= ResolutionV; v++, vi++, ti += 2)
            {
                var circleRadius = sin(PI * v / ResolutionV);
                vertex.Position.xz = circle * circleRadius;
                vertex.Position.y = - cos(PI * v / ResolutionV);
                vertex.Normal = vertex.Position;
                vertex.UV.y = (float)v / ResolutionV;
                streams.SetVertex(vi,vertex);
                
                if (u > 0)      //这种写法会导致循环内部反复判断z变量的值吗？不会，Burst会帮助我们完成优化，它会发现z本质上在这个函数就是一个常量 所以它会写两个版本的循环 一个是带设置三角形的部分 一个是不带 然后通过一次检测决定后续逻辑使用哪个部分版本！也就是说不会重复判断！
                {
                    streams.SetTriangle(ti + 0,vi + new int3(-ResolutionV-2, -ResolutionV - 1,-1 ));
                    streams.SetTriangle(ti + 1,vi + new int3(-1,-ResolutionV - 1, 0));
                }
            }
            
        }

        public Bounds Bounds => new(Vector3.zero, new Vector3(2f,2f,2f));
    }
}