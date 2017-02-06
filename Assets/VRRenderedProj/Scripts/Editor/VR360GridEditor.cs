using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VR360Grid))]
[CanEditMultipleObjects]
public class VR360GridEditor : Editor
{
    VR360Grid grid;
    List<Vector3> undoPositions;

    void OnSceneGUI()
    {
        EventType eventType = Event.current.type;

        var positions = grid.positions;
        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 pos = positions[i];
            Vector3 newPos = Handles.FreeMoveHandle(pos, Quaternion.identity, .03f, new Vector3(.5f, .5f, .5f), Handles.SphereCap);
            Handles.Label(pos, "Pos" + i);
            if (pos != newPos)
            {
                newPos = FindPositiontOnProjectionFromCamera(newPos, grid.OffsetFromSphere);
                positions[i] = newPos;
            }
        }

        // Undo
        switch (eventType)
        {
            case EventType.MouseDown:
                undoPositions = new List<Vector3>(grid.positions);
                break;
            case EventType.MouseUp:
                grid.positions = new List<Vector3>(undoPositions);
                Undo.RecordObject(grid, "Move Grid");
                grid.positions = positions;
                break;
        }

        if (positions.Count == 4)
        {
            grid.CreateGrid();
            Handles.DrawLine(positions[0], positions[1]);
            Handles.DrawLine(positions[1], positions[2]);
            Handles.DrawLine(positions[2], positions[3]);
            Handles.DrawLine(positions[3], positions[0]);

            for (int i = 0; i < grid.width + 1; i++)
            {
                for (int j = 0; j < grid.height + 1; j++)
                {
                    Handles.DotCap(0, grid.GetPos(i, j), Quaternion.identity, 0.005f);
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Save mesh to file"))
        {
            SaveMeshToFile();
        }
    }

    static Vector3 FindPositiontOnProjectionFromCamera(Vector3 pos, float offset = 0f)
    {
        Camera cam = SceneView.lastActiveSceneView.camera;

        if (cam != null)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(pos);
            Ray ray = cam.ScreenPointToRay(screenPos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 31))
            {
                Vector3 dir = (hit.transform.position - hit.point).normalized;
                return hit.point + dir * offset;
            }
            else
            {
                return pos;
            }
        }
        else
        {
            return pos;
        }
    }

    void SaveMeshToFile()
    {
        var filter = grid.GetComponent<MeshFilter>();
        var mesh = filter.sharedMesh;

        var path = EditorUtility.SaveFilePanelInProject("Save mesh", grid.name, "asset", "Select the save path");
        Debug.LogFormat("path : {0}", path);

        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();
    }

    // for removing default handle
    Tool LastTool = Tool.None;
    void OnEnable()
    {
        grid = (VR360Grid)target;
        LastTool = Tools.current;
        Tools.current = Tool.None;

        if (grid.positions.Count.Equals(0))
        {
            grid.positions = new List<Vector3>()
            {
                new Vector3(-0.2f, 0.2f, 0.9f),
                new Vector3(0.2f, 0.2f, 0.9f),
                new Vector3(0.2f, -0.2f, 0.9f),
                new Vector3(-0.2f, -0.2f, 0.9f),
            };
        }
    }

    void OnDisable()
    {
        Tools.current = LastTool;
    }
}
