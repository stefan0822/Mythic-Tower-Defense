
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Banner : MonoBehaviour
{
    void OnEnable()
    {
        CreateBanner();
    }

    void CreateBanner()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Banner";

        mesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f,  0.5f, 0),
            new Vector3( 0.5f,  0.5f, 0),
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3( 0.0f, -0.25f, 0),
            new Vector3( 0.5f, -0.5f, 0)
        };

        mesh.triangles = new int[]
        {
            0, 2, 3,
            0, 3, 1,
            1, 3, 4,
            3, 2, 0,
            1, 3, 0,
            4, 3, 1
        };
        mesh.uv = new Vector2[]
        {
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(0, 0),
            new Vector2(0.5f, 0.25f),
            new Vector2(1, 0)
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}

