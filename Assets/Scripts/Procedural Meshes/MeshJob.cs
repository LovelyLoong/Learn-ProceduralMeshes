using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ProceduralMeshes
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast,CompileSynchronously = true)]     //BurstCompile 表示需要使用Burst编译  FloatPrecision 表示精度（一般需要精确就需要使用Standard）  FloatMode 表示模式（Fast的意思就是表示可以接受一些激进策略来优化性能）  CompileSynchronously 表示同步编译
    public struct MeshJob<TS,TG> : IJobFor
        where TS : struct,IMeshStream
        where TG : struct,IMeshGenerator
    {
        
        [WriteOnly]
        private TS _stream;
        
        private TG _generator;
        
        public void Execute(int index)
        {
            _generator.Execute(index,_stream);
        }
        
        public static JobHandle ScheduleParallel(Mesh mesh,Mesh.MeshData data,int resolution,JobHandle dependency)
        {
            var job = new MeshJob<TS,TG>();
            job._generator.Resolution = resolution;
            job._stream.SetUp(data, mesh.bounds = job._generator.Bounds, job._generator.VertexCount, job._generator.IndexCount);
            return job.ScheduleParallel(job._generator.JobLength,1,dependency);
        }
    }

    public delegate JobHandle MeshJobScheduleDelegate(Mesh mesh, Mesh.MeshData data, int resolution, JobHandle dependency);

}