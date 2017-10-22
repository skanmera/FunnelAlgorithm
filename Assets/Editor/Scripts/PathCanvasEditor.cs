using System.Linq;
using UnityEngine;
using UnityEditor;
using FunnelAlgorithm;

[CustomEditor(typeof(PathCanvas))]
public class PathCanvasEditor : Editor
{
    private void OnSceneGUI()
    {
        var canvas = target as PathCanvas;

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            var camera = SceneView.lastActiveSceneView.camera;
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            var hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.GetComponent<PathCanvas>() != canvas)
                    return;

                canvas.AddVertex(hit.point);
            }
        }

        Draw(canvas);
    }

    private void Draw(PathCanvas canvas)
    {
        if (canvas.Triangles == null)
            return;

        var color = Color.cyan;

        foreach (var t in canvas.Triangles)
        {
            color = Color.cyan;
            color.a = 0.3f;
            Handles.color = color;

            if (t.Equals(canvas.Triangles.Last()))
            {
                color = Color.magenta;
                color.a = 0.3f;
                Handles.color = color;
            }

            Handles.DrawAAConvexPolygon(t.Vertices.ToArray());

            color.a = 1.0f;
            Handles.color = color;
            Handles.DrawPolyLine(t.Vertices.ToArray());
        }

        color = Color.cyan;
        Handles.color = color;

        foreach (var v in canvas.Vertices)
        {
            Handles.SphereCap(0, v, Quaternion.identity, HandleUtility.GetHandleSize(v) / 10);
        }

        if (canvas.Path != null && canvas.Path.Positions != null)
        {
            for (int i = 0; i < canvas.Path.Positions.Length; i++)
            {
                if (canvas.Path.Positions.Length > i + 1)
                {
                    color = new Color(0f, 1.0f, 0f);
                    Handles.color = color;

                    var start = canvas.Path.Positions[i];
                    var end = canvas.Path.Positions[i + 1];
                    Handles.DrawLine(start, end);
                    Handles.SphereCap(0, start, Quaternion.identity, HandleUtility.GetHandleSize(start) / 12);
                    Handles.SphereCap(0, end, Quaternion.identity, HandleUtility.GetHandleSize(end) / 12);
                }

                color = new Color(0f, 0.0f, 1.0f);
                Handles.color = color;

                var origin = canvas.Path.Positions[i];
                var dir = canvas.Path.Normals[i];
                Handles.DrawLine(origin, origin + dir);
                Handles.SphereCap(0, origin + dir, Quaternion.identity, HandleUtility.GetHandleSize(origin + dir) / 12);
            }
        }
    }

    [MenuItem("Funnel/Clear")]
    private static void Clear()
    {
        var canvas = Selection.activeGameObject.GetComponent<PathCanvas>();
        canvas.Clear();

        SceneView.RepaintAll();
    }

    [MenuItem("Funnel/Clear", true)]
    private static bool CanClear()
    {
        if (Selection.activeGameObject == null)
            return false;

        return Selection.activeGameObject.GetComponent<PathCanvas>() != null;
    }

    [MenuItem("Funnel/Remove")]
    private static void Remove()
    {
        var canvas = Selection.activeGameObject.GetComponent<PathCanvas>();
        canvas.Remove();

        SceneView.RepaintAll();
    }

    [MenuItem("Funnel/Remove", true)]
    private static bool CanRemove()
    {
        if (Selection.activeGameObject == null)
            return false;

        var canvas = Selection.activeGameObject.GetComponent<PathCanvas>();
        if (canvas == null)
            return false;

        if (canvas.Triangles == null)
            return false;

        return canvas.Vertices.Count > 0;
    }
}
