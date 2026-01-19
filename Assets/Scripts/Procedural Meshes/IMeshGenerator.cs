using UnityEngine;

namespace ProceduralMeshes
{
    public interface IMeshGenerator
    {
        int VertexCount { get; }
        
        int IndexCount  { get; }
        
        int JobLength { get; }
        
        int Resolution { get; set; }
        
        void Execute<T>(int u ,T streams) where T : struct,IMeshStreams;
        
        Bounds Bounds { get; }
    }
}