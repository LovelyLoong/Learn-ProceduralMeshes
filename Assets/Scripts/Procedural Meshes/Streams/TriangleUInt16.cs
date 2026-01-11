using System.Runtime.InteropServices;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace ProceduralMeshes.Streams
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TriangleUInt16
    {
        public ushort a;
        public ushort b;
        public ushort c;
        
        public static implicit operator TriangleUInt16(int3 triangle)
        {
            return new TriangleUInt16
            {
                a = (ushort)triangle.x,
                b = (ushort)triangle.y,
                c = (ushort)triangle.z,
            };
        }
    }
}