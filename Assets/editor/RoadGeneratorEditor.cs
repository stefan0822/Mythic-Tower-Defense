
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NewRoadGeneratorEditor : EditorWindow
{
    private readonly List<Vector3> points = new();

    private Material roadMaterial;
    private float groundY = 3.1f;
    private float roadWidth = 7f;
    private float textureLength = 7f;

    private bool drawing = false;

    [MenuItem("Tools/New Road Generator")]
    public static void Open()
    {
        GetWindow<NewRoadGeneratorEditor>("New Road Generator");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        GUILayout.Label("Tower Defense - Road Generator",
            EditorStyles.boldLabel);

        groundY = EditorGUILayout.FloatField(
            "Road Height", groundY);

        roadWidth = EditorGUILayout.FloatField(
            "Road Width", roadWidth);

        textureLength = EditorGUILayout.FloatField(
            "Texture Length", textureLength);

        roadMaterial = (Material)EditorGUILayout.ObjectField(
            "Road Material", roadMaterial,
            typeof(Material), false);

        GUILayout.Space(10);

        drawing = GUILayout.Toggle(
            drawing, "Enable Shift + Click", "Button");

        EditorGUILayout.HelpBox(
            "Shift + Left Click to add road points.\n" +
            "Use a top-down Scene view.",
            MessageType.Info);

        GUILayout.Label("Points: " + points.Count);

        if (GUILayout.Button("Undo Last Point"))
        {
            if (points.Count > 0)
                points.RemoveAt(points.Count - 1);

            SceneView.RepaintAll();
        }

        if (GUILayout.Button("Clear Points"))
        {
            points.Clear();
            SceneView.RepaintAll();
        }

        if (GUILayout.Button("Generate Road"))
            GenerateRoad();
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        Handles.color = Color.cyan;

        for (int i = 0; i < points.Count; i++)
        {
            float size =
                HandleUtility.GetHandleSize(points[i]) * 0.12f;

            Handles.SphereHandleCap(
                0, points[i], Quaternion.identity,
                size, EventType.Repaint);

            if (i > 0)
                Handles.DrawLine(points[i - 1], points[i]);
        }

        if (!drawing)
            return;

        if (e.shift && !e.alt &&
            e.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(
                GUIUtility.GetControlID(FocusType.Passive));
        }

        if (e.type == EventType.MouseDown &&
            e.button == 0 && e.shift && !e.alt)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(
                e.mousePosition);

            Plane plane = new Plane(
                Vector3.up, new Vector3(0, groundY, 0));

            if (plane.Raycast(ray, out float distance))
            {
                Vector3 p = ray.GetPoint(distance);
                p.y = groundY;

                if (points.Count == 0 ||
                    Vector3.Distance(points[points.Count - 1], p)
                    > 0.1f)
                {
                    points.Add(p);
                }

                Repaint();
                sceneView.Repaint();
            }

            e.Use();
        }
    }

    private void GenerateRoad()
    {
        if (points.Count < 2 || roadWidth <= 0f ||
            textureLength <= 0f)
        {
            EditorUtility.DisplayDialog(
                "Invalid Road",
                "Add at least 2 points and use positive dimensions.",
                "OK");
            return;
        }

        Mesh mesh = new Mesh();
        mesh.name = "RoadMesh";

        int n = points.Count;

        Vector3[] vertices = new Vector3[n * 2];
        Vector2[] uv = new Vector2[n * 2];
        int[] triangles = new int[(n - 1) * 6];

        float distanceAlong = 0f;

        for (int i = 0; i < n; i++)
        {
            Vector3 forward;

            if (i == 0)
                forward = (points[1] - points[0]).normalized;
            else if (i == n - 1)
                forward = (points[i] - points[i - 1]).normalized;
            else
                forward = (points[i + 1] - points[i - 1]).normalized;

            forward.y = 0f;
            forward.Normalize();

            Vector3 side = new Vector3(
                -forward.z, 0f, forward.x);

            // Compensate the width at corners.
            float halfWidth = roadWidth * 0.5f;

            if (i > 0 && i < n - 1)
            {
                Vector3 prev =
                    (points[i] - points[i - 1]).normalized;

                Vector3 next =
                    (points[i + 1] - points[i]).normalized;

                prev.y = 0f;
                next.y = 0f;

                prev.Normalize();
                next.Normalize();

                Vector3 prevSide =
                    new Vector3(-prev.z, 0f, prev.x);

                Vector3 nextSide =
                    new Vector3(-next.z, 0f, next.x);

                Vector3 miter = prevSide + nextSide;

                if (miter.sqrMagnitude > 0.0001f)
                {
                    miter.Normalize();

                    float denominator =
                        Vector3.Dot(miter, nextSide);

                    if (denominator > 0.01f)
                    {
                        side = miter;

                        halfWidth = Mathf.Min(
                            halfWidth / denominator,
                            roadWidth);
                    }
                }
            }

            Vector3 center = points[i];

            vertices[i * 2] =
                center + side * halfWidth;

            vertices[i * 2 + 1] =
                center - side * halfWidth;

            if (i > 0)
                distanceAlong += Vector3.Distance(
                    points[i - 1], points[i]);

            float v = distanceAlong / textureLength;

            uv[i * 2] = new Vector2(0f, v);
            uv[i * 2 + 1] = new Vector2(1f, v);
        }

        for (int i = 0; i < n - 1; i++)
        {
            int a = i * 2;
            int b = a + 1;
            int c = a + 2;
            int d = a + 3;

            int t = i * 6;

            triangles[t] = a;
            triangles[t + 1] = c;
            triangles[t + 2] = b;

            triangles[t + 3] = b;
            triangles[t + 4] = c;
            triangles[t + 5] = d;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Save the mesh as an asset so it persists.
        const string folder = "Assets/GeneratedRoadMeshes";

        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder(
                "Assets", "GeneratedRoadMeshes");

        string path = AssetDatabase.GenerateUniqueAssetPath(
            folder + "/RoadMesh.asset");

        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();

        GameObject road = new GameObject("NewGeneratedRoad");
        Undo.RegisterCreatedObjectUndo(
            road, "Generate Road");

        MeshFilter filter = road.AddComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        MeshRenderer renderer =
            road.AddComponent<MeshRenderer>();

        if (roadMaterial != null)
        {
            renderer.sharedMaterial = roadMaterial;
        }
        else
        {
            Debug.LogWarning(
                "No Road Material assigned. " +
                "Assign an opaque material.");
        }

        MeshCollider collider =
            road.AddComponent<MeshCollider>();
        collider.sharedMesh = mesh;

        Selection.activeGameObject = road;

        drawing = false;
        points.Clear();

        EditorSceneRepaint();

        Debug.Log("Road generated successfully!");
    }

    private void EditorSceneRepaint()
    {
        SceneView.RepaintAll();
        Repaint();
    }
}
