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
        
        public int VertexCount => (ResolutionU + 1) * (ResolutionV + 1) - 2;

        public int IndexCount => 6 * ResolutionU * (ResolutionV - 1);

        public int JobLength => ResolutionU + 1;
        
        public int Resolution { get; set; }

        public void Execute<T>(int u, T streams) where T : struct,IMeshStreams
        {
            if (u == 0)
            {
                ExecuteSeam(streams);
            }
            else
            {
                ExecuteRegular(u,streams);
            }
            
        }
        
        public void ExecuteSeam<TS>(TS streams) where TS : struct, IMeshStreams
        {
            var vertex = new Vertex();
            vertex.Tangent.x = 1f;
            vertex.Tangent.w = -1f;
            
            for (var v = 1; v < ResolutionV; v++)
            {
                var circleRadius = sin(PI * v / ResolutionV);
                vertex.Position.z = -circleRadius;
                vertex.Position.y = - cos(PI * v / ResolutionV);
                vertex.Normal = vertex.Position;
                vertex.UV.y = (float)v / ResolutionV;
                streams.SetVertex(v - 1,vertex);
            }
        }
        
        public void ExecuteRegular<T>(int u, T streams) where T : struct,IMeshStreams
        {
            var vi = u * (ResolutionV + 1) - 2;
            var ti = (u - 1) * 2 * (ResolutionV - 1);

            //为什么第一个顶点的数据都要偏移0.5f?这是为了取一半来让整体表现效果显得更均匀！因为实际上这里最终就是将原本一条边的内容强制合并到一个点上面 如果依然取u会感觉整体向右纹理偏移！
            var vertex = new Vertex();
            vertex.Normal.y = -1f;
            vertex.Position.y = -1f;
            vertex.Tangent.x = cos(2f * PI * (u - 0.5f) / ResolutionU);
            vertex.Tangent.z = sin(2f * PI * (u - 0.5f) / ResolutionU);
            vertex.Tangent.w = -1f;
            vertex.UV.x = (u - 0.5f) / ResolutionU;
            streams.SetVertex(vi,vertex);
            vertex.Normal.y = 1f;
            vertex.Position.y = 1f;
            vertex.UV.y = 1f;
            streams.SetVertex(vi + ResolutionV,vertex);
            vi ++;
            
            //这段代码的思想！ 对于(N + 1) * (N + 1)的问题怎么解决？ 我们每行单独看做一个Job 每一行有N+1个对象怎么解决？先找到头 设置头的相关数据 后续依次复用重复的内容达到最大程度的复用！
            // vertex.Position.x = sin(2f * PI * u / Resolution);      //2f * PI * u / Resolution  其实本质就是这个X对于圆心的夹角的弧度！
            // vertex.Position.z = -cos(2f * PI * u / Resolution);
            // vertex.Normal = vertex.Position;
            float2 circle;
            circle.x = sin(2f * PI * u / ResolutionU);
            circle.y = cos(2f * PI * u / ResolutionU);
            vertex.Tangent.xz = circle.yx;
            circle.y = -circle.y;
            vertex.UV.x = (float)u / ResolutionU;
            
            var shiftLeft = ( u == 1 ? 0 : -1) - ResolutionV;
            streams.SetTriangle(ti,vi + new int3(-1,shiftLeft, 0));
            ti += 1;
            
            for (var v = 1; v < ResolutionV; v++, vi++)
            {
                var circleRadius = sin(PI * v / ResolutionV);
                vertex.Position.xz = circle * circleRadius;
                vertex.Position.y = - cos(PI * v / ResolutionV);
                vertex.Normal = vertex.Position;
                vertex.UV.y = (float)v / ResolutionV;
                streams.SetVertex(vi,vertex);
                
                if (v > 1)      //这种写法会导致循环内部反复判断z变量的值吗？不会，Burst会帮助我们完成优化，它会发现z本质上在这个函数就是一个常量 所以它会写两个版本的循环 一个是带设置三角形的部分 一个是不带 然后通过一次检测决定后续逻辑使用哪个部分版本！也就是说不会重复判断！
                {
                    streams.SetTriangle(ti + 0,vi + new int3(shiftLeft - 1, shiftLeft,-1 ));
                    streams.SetTriangle(ti + 1,vi + new int3(-1,shiftLeft, 0));
                    ti += 2;
                }
            }
            
            streams.SetTriangle(ti,vi + new int3(shiftLeft -1, 0,-1 ));
            
        }

        public Bounds Bounds => new(Vector3.zero, new Vector3(2f,2f,2f));
    }
}