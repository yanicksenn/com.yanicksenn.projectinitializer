using UnityEditor;
using UnityEngine;

public class MeshRotatorWindow : EditorWindow
{
    private Mesh selectedMesh;
    private Vector3 rotationAngles;

    [MenuItem("Tools/Mesh Rotator")]
    public static void ShowWindow()
    {
        GetWindow<MeshRotatorWindow>("Mesh Rotator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Select a Mesh Asset to Rotate", EditorStyles.boldLabel);

        selectedMesh = (Mesh)EditorGUILayout.ObjectField("Mesh Asset", selectedMesh, typeof(Mesh), false);
        rotationAngles = EditorGUILayout.Vector3Field("Rotation (Euler Angles)", rotationAngles);

        if (GUILayout.Button("Rotate Mesh"))
        {
            if (selectedMesh != null)
            {
                RotateMesh(selectedMesh, rotationAngles);
            }
            else
            {
                EditorUtility.DisplayDialog("No Mesh Selected", "Please select a mesh asset.", "OK");
            }
        }
    }

    private void RotateMesh(Mesh mesh, Vector3 eulerAngles)
    {
        if (mesh == null)
        {
            Debug.LogError("No mesh asset selected.");
            return;
        }

        Quaternion rotation = Quaternion.Euler(eulerAngles);
        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = rotation * vertices[i];
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals(); // Recalculate normals after rotation
        mesh.RecalculateTangents(); // Recalculate tangents after rotation
        mesh.RecalculateBounds(); // Recalculate bounds after rotation

        EditorUtility.SetDirty(mesh);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Mesh '{mesh.name}' has been rotated by {eulerAngles} degrees and the asset has been updated.");
    }
}
