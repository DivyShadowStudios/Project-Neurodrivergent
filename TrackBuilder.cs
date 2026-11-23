
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TrackBuilder : MonoBehaviour
{
    [Header("Track Dimensions")]
    public float radiusX = 16f;
    public float radiusY = 10f;
    public float roadWidth = 5f;
    [Range(32, 256)]
    public int segments = 160;

    [Header("Appearance")]
    public Color roadColor = new Color(0.18f, 0.19f, 0.21f);
    public Color wallColor = Color.white;
    public float wallThickness = 0.25f;

    void Start()
    {
        GenerateTrack();
    }

    [ContextMenu("Generate Track")]
    public void GenerateTrack()
    {
        GenerateRoad();
        GenerateBoundary("OuterWall", 1);
        GenerateBoundary("InnerWall", -1);
    }

    Vector2 GetCenter(float angle)
    {
        return new Vector2(
            radiusX * Mathf.Cos(angle),
            radiusY * Mathf.Sin(angle)
        );
    }

    Vector2 GetNormal(float angle)
    {
        Vector2 tangent = new Vector2(
            -radiusX * Mathf.Sin(angle),
            radiusY * Mathf.Cos(angle)
        );

        return new Vector2(tangent.y, -tangent.x).normalized;
    }

    Vector2 GetBoundaryPoint(float angle, int side)
    {
        float half = roadWidth * 0.5f;

        return GetCenter(angle) +
               GetNormal(angle) * half * side;
    }

    void GenerateRoad()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Procedural Racing Track";

        Vector3[] vertices = new Vector3[segments * 2];
        int[] triangles = new int[segments * 6];

        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;

            Vector2 center = GetCenter(angle);
            Vector2 normal = GetNormal(angle);

            vertices[i * 2] =
                center + normal * roadWidth * 0.5f;

            vertices[i * 2 + 1] =
                center - normal * roadWidth * 0.5f;

            int next = (i + 1) % segments;

            int t = i * 6;

            triangles[t] = i * 2;
            triangles[t + 1] = next * 2;
            triangles[t + 2] = i * 2 + 1;

            triangles[t + 3] = next * 2;
            triangles[t + 4] = next * 2 + 1;
            triangles[t + 5] = i * 2 + 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().sharedMesh = mesh;

        Material material = new Material(
            Shader.Find("Universal Render Pipeline/Unlit")
        );

        material.color = roadColor;

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.sortingOrder = -10;
    }

    void GenerateBoundary(string wallName, int side)
    {
        Transform existing = transform.Find(wallName);

        if (existing != null)
            Destroy(existing.gameObject);

        GameObject wall = new GameObject(wallName);
        wall.transform.SetParent(transform, false);

        int layer = LayerMask.NameToLayer("TrackWall");

        if (layer >= 0)
            wall.layer = layer;

        Vector2[] points = new Vector2[segments + 1];

        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            points[i] = GetBoundaryPoint(angle, side);
        }

        points[segments] = points[0];

        EdgeCollider2D edge = wall.AddComponent<EdgeCollider2D>();
        edge.points = points;
        edge.edgeRadius = wallThickness * 0.5f;

        LineRenderer line = wall.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.positionCount = points.Length;
        line.startWidth = wallThickness;
        line.endWidth = wallThickness;
        line.loop = false;
        line.sortingOrder = 0;

        Material wallMaterial = new Material(
            Shader.Find("Sprites/Default")
        );

        wallMaterial.color = wallColor;
        line.sharedMaterial = wallMaterial;

        for (int i = 0; i < points.Length; i++)
        {
            line.SetPosition(i, points[i]);
        }
    }
}
