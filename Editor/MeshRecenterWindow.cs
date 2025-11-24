using UnityEditor;
using UnityEngine;

public class MeshRecenterWindow : EditorWindow
{
    private Mesh selectedMesh;

    [MenuItem("Tools/Mesh Recenter")]
    public static void ShowWindow()
    {
        GetWindow<MeshRecenterWindow>("Mesh Recenter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Select a Mesh Asset to Recenter", EditorStyles.boldLabel);

        selectedMesh = (Mesh)EditorGUILayout.ObjectField("Mesh Asset", selectedMesh, typeof(Mesh), false);

        if (GUILayout.Button("Recenter Mesh"))
        {
            if (selectedMesh != null)
            {
                RecenterMesh(selectedMesh);
            }
            else
            {
                EditorUtility.DisplayDialog("No Mesh Selected", "Please select a mesh asset.", "OK");
            }
        }
    }

    private void RecenterMesh(Mesh mesh)
    {
        if (mesh == null)
        {
            Debug.LogError("No mesh asset selected.");
            return;
        }

        // Calculate the center of the mesh
        Vector3 center = mesh.bounds.center;

        // If the mesh is already centered, there's nothing to do
        if (center == Vector3.zero)
        {
            Debug.Log($"Mesh '{mesh.name}' is already centered.");
            return;
        }

        // Translate the vertices
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] -= center;
        }

        // Apply the changes to the mesh
        mesh.vertices = vertices;
        mesh.RecalculateBounds(); // Recalculate the bounding box after moving the vertices

        // Mark the mesh as dirty to ensure the changes are saved
        EditorUtility.SetDirty(mesh);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Mesh '{mesh.name}' has been recentered and the asset has been updated.");
    }
}
