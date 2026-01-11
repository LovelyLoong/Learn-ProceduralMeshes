using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class MeshCreater : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    
    private void OnEnable()
    {
        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();
        

        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        var meshTemp = new Mesh
        {
            name = "Procedural Mesh",
            vertices = new[]
            {
                Vector3.zero, 
                Vector3.right, 
                Vector3.up,
                new Vector3(1,1) 
            },
            triangles = new[]
            {
                0, 2, 1,1,2,3
            },
            normals = new []
            {
                Vector3.back, 
                Vector3.back, 
                Vector3.back,
                Vector3.back,
            },
            uv = new []
            {
                Vector2.zero, 
                Vector2.right, 
                Vector2.up, 
                Vector2.one, 
            },
            tangents = new Vector4[] {
            new (1f, 0f, 0f, -1f),
            new (1f, 0f, 0f, -1f),
            new (1f, 0f, 0f, -1f),
            new (1f, 0f, 0f, -1f)
            }
        };

        meshTemp.RecalculateTangents();

        meshFilter.mesh = meshTemp;
    }
}
