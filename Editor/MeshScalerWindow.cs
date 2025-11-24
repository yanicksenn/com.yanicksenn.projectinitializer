using UnityEditor;
using UnityEngine;

public class MeshScalerWindow : EditorWindow
{
    private Mesh selectedMesh;
    private float scaleFactor = 1.0f;

    [MenuItem("Tools/Mesh Scaler")]
    public static void ShowWindow()
    {
        GetWindow<MeshScalerWindow>("Mesh Scaler");
    }

    private void OnGUI()
    {
        GUILayout.Label("Select a Mesh Asset to Scale", EditorStyles.boldLabel);

        selectedMesh = (Mesh)EditorGUILayout.ObjectField("Mesh Asset", selectedMesh, typeof(Mesh), false);
        scaleFactor = EditorGUILayout.FloatField("Scale Factor", scaleFactor);

        if (GUILayout.Button("Scale Mesh"))
        {
            if (selectedMesh != null)
            {
                ScaleMesh(selectedMesh, scaleFactor);
            }
            else
            {
                EditorUtility.DisplayDialog("No Mesh Selected", "Please select a mesh asset.", "OK");
            }
        }
    }

    private void ScaleMesh(Mesh mesh, float factor)
    {
        if (mesh == null)
        {
            Debug.LogError("No mesh asset selected.");
            return;
        }

        if (factor <= 0)
        {
            EditorUtility.DisplayDialog("Invalid Scale Factor", "Scale factor must be greater than 0.", "OK");
            return;
        }

        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] *= factor;
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals(); // Recalculate normals in case scaling affects them

        EditorUtility.SetDirty(mesh);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Mesh '{mesh.name}' has been scaled by a factor of {factor} and the asset has been updated.");
    }
}
