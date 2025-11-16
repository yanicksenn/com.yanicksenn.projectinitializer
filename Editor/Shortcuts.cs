using UnityEditor;
using UnityEngine;
using YanickSenn.ProjectInitializer.Editor.Anchors;

namespace YanickSenn.ProjectInitializer.Editor {
    public static class Shortcuts {
        [MenuItem("Tools/Shortcuts/Create Prefab")]
        [MenuItem("GameObject/Create Prefab")]
        private static void CreatePrefab() {
            var selectedObject = Selection.activeGameObject;
            if (selectedObject == null) {
                Debug.Log("No object selected.");
                return;
            }

            var prefabPath = GetPrefabPath();
            if (string.IsNullOrEmpty(prefabPath)) {
                Debug.LogError("Prefabs folder not found.");
                return;
            }

            var localPath = $"{prefabPath}/{selectedObject.name}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(selectedObject, localPath, InteractionMode.UserAction);
            if (prefab == null) {
                Debug.LogError("Failed to create prefab.");
            }
        }

        private static string GetPrefabPath() {
            var guids = AssetDatabase.FindAssets($"t:{nameof(PrefabsAnchor)}");
            if (guids.Length == 0) {
                return string.Empty;
            }
            
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var anchor = AssetDatabase.LoadAssetAtPath<PrefabsAnchor>(path);
            return anchor.GetParentDirectory();
        }

        [MenuItem("GameObject/Create Prefab", true)]
        private static bool ValidateCreatePrefab() {
            var selectedObject = Selection.activeGameObject;
            if (selectedObject == null) {
                return false;
            }

            return !PrefabUtility.IsPartOfPrefabAsset(selectedObject);
        }
    }
}
