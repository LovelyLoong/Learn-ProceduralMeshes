using System;
using ProceduralMeshes;
using ProceduralMeshes.Generator;
using ProceduralMeshes.Streams;
using UnityEngine;
using UnityEngine.Serialization;

namespace DefaultNamespace
{
    [RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
    public class ProcedureMesh : MonoBehaviour
    {
        [SerializeField]
        [Range(1,50)]
        private int resolution = 1;
        [SerializeField]
        private MeshType meshType;
        [SerializeField]
        private GizmoMode gizmoMode;
        
        private Mesh _mesh;
        private Vector3[] _vertices;
        private Vector3[] _normals;
        private Vector4[] _tangents;
        
        private static MeshJobScheduleDelegate[] _jobs = 
        {
            MeshJob<SingleStream,SquareGrid>.ScheduleParallel,
            MeshJob<SingleStream,SharedSquareGrid>.ScheduleParallel,
            MeshJob<SingleStream,SharedTriangleGrid>.ScheduleParallel,
            MeshJob<MultiStream,PointyHexagonGrid>.ScheduleParallel,
            MeshJob<MultiStream,FlatHexagonGrid>.ScheduleParallel,
            MeshJob<MultiStream,UVSphere>.ScheduleParallel,
        };

        private enum  MeshType
        {
            SquareGrid,
            SharedSquareGrid,
            SharedTriangleGrid,
            PointyHexagonGrid,
            FlatHexagonGrid,
            UVSphere,
        }

        [System.Flags]
        private enum GizmoMode
        {
            None = 0b0,
            Vertices = 0b1,
            Normals = 0b10,
            Tangents = 0b100,
        }
        

        
        private void Awake()
        {
            _mesh = new Mesh { name = "Procedural Mesh" };
            GetComponent<MeshFilter>().mesh = _mesh;
        }

        
        private void GenerateMesh()
        {
            var meshDataAry = Mesh.AllocateWritableMeshData(1);
            var meshData = meshDataAry[0];
            _jobs[(int)meshType](_mesh,meshData,resolution,default).Complete();
            Mesh.ApplyAndDisposeWritableMeshData(meshDataAry, _mesh);
        }
        
        private void OnValidate()
        {
            enabled = true;
        }

        private void Update()
        {
            GenerateMesh();
            enabled = false;
            _vertices = null;
            _normals = null;
            _tangents = null;
        }

        private void OnDrawGizmos()
        {
            if (gizmoMode == GizmoMode.None || _mesh == null) 
                return;
            
            var drawVertices = (gizmoMode & GizmoMode.Vertices) != 0;
            var drawNormals = (gizmoMode & GizmoMode.Normals) != 0;
            var drawTangents = (gizmoMode & GizmoMode.Tangents) != 0;

            if (_vertices == null)
            {
                _vertices = _mesh.vertices;
            }
            
            if (drawNormals && _normals == null)
            {
                _normals = _mesh.normals;
            }
            
            if (drawTangents && _tangents == null)
            {
                _tangents = _mesh.tangents;
            }

            var transformCmp = transform;
            for (int i = 0; i < _vertices.Length; i++)
            {
                var posTemp = transformCmp.TransformPoint(_vertices[i]);
                if (drawVertices)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(posTemp, 0.02f);
                }

                if (drawNormals)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(posTemp, transformCmp.TransformDirection(_normals[i] * 0.2f));
                }

                if (drawTangents)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawRay(posTemp, transformCmp.TransformDirection(_tangents[i] * 0.2f));
                }
                
            }
        }
    }
}