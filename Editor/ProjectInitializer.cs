using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using YanickSenn.Utils;

namespace YanickSenn.ProjectInitializer.Editor
{
    public static class ProjectInitializer {
        
        private static readonly Dictionary<string, AnchorConfig> Folders = new() {
            { "Assets/Art", new AnchorConfig() { fileNamePrefix = "art" } },
            { "Assets/Audio", new AnchorConfig() { fileNamePrefix = "audio" } },
            { "Assets/Features", new AnchorConfig() { fileNamePrefix = "feature", classType = typeof(FeatureFlag)  } },
            { "Assets/Events", new AnchorConfig() { fileNamePrefix = "event", classType = typeof(GlobalEvent)  } },
            { "Assets/Materials", new AnchorConfig() { fileNamePrefix = "material", classType = typeof(Material)  } },
            { "Assets/Models", new AnchorConfig() { } },
            { "Assets/Prefabs", new AnchorConfig() { fileNamePrefix = "prefab", classType = typeof(GameObject)  } },
            { "Assets/Scenes", new AnchorConfig() { fileNamePrefix = "scene", classType = typeof(SceneAsset)  } },
            { "Assets/Scripts", new AnchorConfig() { disableAutoFixing = true } },
            { "Assets/Settings", new AnchorConfig() { } },
            { "Assets/Shaders", new AnchorConfig() { fileNamePrefix = "shader", classType = typeof(Shader)  } },
            { "Assets/Textures", new AnchorConfig() { fileNamePrefix = "texture" } },
            { "Assets/Variables/Ints", new AnchorConfig() { fileNamePrefix = "int", classType = typeof(IntVariable) } },
        };
        
        [MenuItem("Tools/Project Setup/Initialize Project", priority = 0)]
        public static void InitializeProject() {
            CreateFolders();
            CopyResources();
            AddAndResolvePackages();
        }
        
        [MenuItem("Tools/Project Setup/Create missing folders only", priority = 1)]
        public static void CreateMissingFoldersOnly() {
            CreateFolders();
        }

        [MenuItem("Tools/Project Setup/Create Embedded Unity Package", priority = 2)]
        public static void ShowPackageCreatorWindow() {
            PackageCreatorWindow.ShowWindow();
        }
        
        [MenuItem("Tools/Project Setup/Auto-fix Violations", priority = 3)]
        public static void AutoFixViolations() {
            var violationsFound = false;
            var anchorGuids = AssetDatabase.FindAssets($"t:{typeof(Anchor)}");
            foreach (var anchorGuid in anchorGuids) {
                var anchorPath = AssetDatabase.GUIDToAssetPath(anchorGuid);
                var anchor = AssetDatabase.LoadAssetAtPath<Anchor>(anchorPath);
                if (anchor.DisableAutoFixing) {
                    continue;
                }
                var assetTypes = anchor.GetAssetTypes();
                var anchorDirectory = anchor.GetParentDirectory();

                if (assetTypes == null || assetTypes.Count == 0) {
                    continue;
                }

                var assetGuids = AssetDatabase.FindAssets($"t:{string.Join(",", assetTypes.Select(t => t.Name))}");
                foreach (var assetGuid in assetGuids) {
                    var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                    if (!assetPath.StartsWith("Assets/")) {
                        continue;
                    }

                    var assetDirectory = Path.GetDirectoryName(assetPath);
                    if (assetDirectory != anchorDirectory) {
                        violationsFound = true;
                        var fileName = Path.GetFileName(assetPath);
                        var newPath = Path.Combine(anchorDirectory, fileName);
                        AssetDatabase.MoveAsset(assetPath, newPath);
                        assetPath = newPath;
                    }

                    if (anchor.GetFileNamingStrategy().Rename(assetPath)) {
                        violationsFound = true;
                    }
                }
            }

            if (violationsFound) {
                AssetDatabase.Refresh();
                Debug.Log("Asset violations fixed.");
            } else {
                Debug.Log("No asset violations found.");
            }
        }
        
        [MenuItem("Tools/Project Setup/Bump & Release Package Version", priority = 3)]
        public static void ShowPackageVersioningWindow() {
            PackageVersioningWindow.ShowWindow();
        }
        
        private static void CopyResources() {
            var packagePath = Path.GetFullPath("Packages/com.yanicksenn.projectinitializer");
            var resourcesPath = Path.Combine(packagePath, "Resources");
            var projectRootPath = Directory.GetParent(Application.dataPath)?.FullName;

            if (projectRootPath == null) {
                Debug.LogError("Could not determine project root path.");
                return;
            }

            if (!Directory.Exists(resourcesPath)) {
                return;
            }
            
            var resourceFiles = Directory.GetFiles(resourcesPath, "resource_*");

            foreach (var resourceFile in resourceFiles) {
                var fileName = Path.GetFileName(resourceFile);
                var newFileName = fileName.Replace("resource_", "");
                var newFilePath = Path.Combine(projectRootPath, newFileName);

                if (File.Exists(newFilePath)) {
                    continue;
                }
                
                File.Copy(resourceFile, newFilePath);
            }
        }


        private static void CreateFolders() {
            foreach (var folderEntry in Folders) {
                var folderPath = folderEntry.Key;
                var anchorConfig = folderEntry.Value;

                FileUtils.CreateDirectoryIfNeeded(folderPath);

                var folderName = Path.GetFileName(folderPath);
                var anchorPath = $"{folderPath}/_anchor_{folderName.ToLower()}.asset";

                if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(anchorPath) != null) continue;

                var anchor = ScriptableObject.CreateInstance<Anchor>();
                anchor.DisableAutoFixing = anchorConfig.disableAutoFixing;
                anchor.FileNamePrefix = anchorConfig.fileNamePrefix;
                anchor.ClassType = anchorConfig.classType;
                AssetDatabase.CreateAsset(anchor, anchorPath);
                Debug.Log("Created anchor: " + anchorPath);
            }

            AssetDatabase.Refresh();
        }

        private static void AddAndResolvePackages() {
            Client.AddAndRemove(new[] {
                "com.unity.multiplayer.playmode",
                "com.unity.multiplayer.tools",
                "com.unity.netcode.gameobjects",
                "com.unity.postprocessing",
                "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.17.0"
            });
            Client.Resolve();
        }
    }
}