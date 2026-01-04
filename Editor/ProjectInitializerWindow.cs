using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using YanickSenn.Utils.Events;
using YanickSenn.Utils.Features;
using YanickSenn.Utils.Variables;

namespace YanickSenn.ProjectInitializer.Editor
{
    public class ProjectInitializerWindow : EditorWindow
    {
        public static readonly Dictionary<string, AnchorConfig> Folders = new() {
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
            { "Assets/Variables/Floats", new AnchorConfig() { fileNamePrefix = "float", classType = typeof(FloatVariable) } },
            { "Assets/Variables/Bools", new AnchorConfig() { fileNamePrefix = "bool", classType = typeof(BoolVariable) } },
        };
        
        public static readonly List<string> Packages = new() {
            "com.unity.multiplayer.playmode",
            "com.unity.multiplayer.tools",
            "com.unity.netcode.gameobjects",
            "com.unity.postprocessing",
            "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.17.0",
            "https://github.com/yanicksenn/com.yanicksenn.packagebumper.git",
            "https://github.com/yanicksenn/com.yanicksenn.packagegenerator.git",
            "https://github.com/yanicksenn/com.yanicksenn.tableview.git",
            "https://github.com/yanicksenn/com.yanicksenn.taskview.git",
            "https://github.com/yanicksenn/com.yanicksenn.utils.git",
            "https://github.com/yanicksenn/com.yanicksenn.firstpersoncontroller.git",
            "https://github.com/yanicksenn/com.yanicksenn.selectionhistory.git",
        };
        
        private readonly Dictionary<string, bool> _foldersToCreate = new();
        private readonly Dictionary<string, bool> _foldersDisabled = new();
        private readonly Dictionary<string, bool> _packagesToInstall = new();
        private readonly Dictionary<string, bool> _packagesDisabled = new();
        private readonly Dictionary<string, bool> _resourcesToCopy = new();
        private readonly Dictionary<string, bool> _resourcesDisabled = new();

        private bool _createFoldersEnabled = true;
        private bool _installPackagesEnabled = true;
        private bool _copyResourcesEnabled = true;

        private bool _foldersFoldout = true;
        private bool _packagesFoldout = true;
        private bool _resourcesFoldout = true;

        private Vector2 _scrollPosition;

        [MenuItem("Tools/Project Setup/Initialize Project")]
        public static void ShowWindow() {
            GetWindow<ProjectInitializerWindow>("Project Initializer");
        }

        private void OnEnable()
        {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            var packagesPath = Path.Combine(projectRoot, "Packages");
            var manifestPath = Path.Combine(packagesPath, "manifest.json");
            var manifestContent = File.Exists(manifestPath) ? File.ReadAllText(manifestPath) : "";
            
            var localPackageFolders = new List<string>();
            if (Directory.Exists(packagesPath))
            {
                localPackageFolders = Directory.GetDirectories(packagesPath).Select(Path.GetFileName).ToList();
            }

            foreach (var folder in Folders.Keys)
            {
                var exists = Directory.Exists(folder);
                _foldersDisabled[folder] = exists;
                _foldersToCreate[folder] = !exists;
            }

            foreach (var package in Packages)
            {
                var existsInManifest = manifestContent.Contains(package);
                var packageNameFromUrl = GetPackageNameFromUrl(package);
                var existsLocally = localPackageFolders.Contains(package) || (!string.IsNullOrEmpty(packageNameFromUrl) && localPackageFolders.Contains(packageNameFromUrl));
                
                var exists = existsInManifest || existsLocally;
                _packagesDisabled[package] = exists;
                _packagesToInstall[package] = !exists;
            }

            foreach (var resource in GetAvailableResources())
            {
                var exists = File.Exists(Path.Combine(projectRoot, resource));
                _resourcesDisabled[resource] = exists;
                _resourcesToCopy[resource] = !exists;
            }
        }

        private string GetPackageNameFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url) || (!url.StartsWith("http") && !url.StartsWith("git")))
            {
                return null;
            }

            try
            {
                var uri = new Uri(url);
                var path = uri.AbsolutePath;
                var lastSegment = path.Split('/').LastOrDefault();
                if (string.IsNullOrEmpty(lastSegment))
                {
                    return null;
                }

                if (lastSegment.EndsWith(".git"))
                {
                    lastSegment = lastSegment.Substring(0, lastSegment.Length - 4);
                }

                return lastSegment;
            }
            catch
            {
                return null;
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("Project Initializer Configuration", EditorStyles.boldLabel);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawFoldersSection();
            DrawPackagesSection();
            DrawResourcesSection();

            GUILayout.Space(20);

            var hasFoldersToCreate = _createFoldersEnabled && _foldersToCreate.Any(x => x.Value);
            var hasPackagesToInstall = _installPackagesEnabled && _packagesToInstall.Any(x => x.Value);
            var hasResourcesToCopy = _copyResourcesEnabled && _resourcesToCopy.Any(x => x.Value);
            var anythingToDo = hasFoldersToCreate || hasPackagesToInstall || hasResourcesToCopy;

            EditorGUI.BeginDisabledGroup(!anythingToDo);
            if (GUILayout.Button("Initialize Project"))
            {
                InitializeProject();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndScrollView();
        }

        private void DrawFoldersSection()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            _createFoldersEnabled = EditorGUILayout.ToggleLeft("", _createFoldersEnabled, GUILayout.Width(20));
            _foldersFoldout = EditorGUILayout.Foldout(_foldersFoldout, "Folders to Create", true, EditorStyles.foldoutHeader);
            EditorGUILayout.EndHorizontal();
            
            if (_foldersFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUI.BeginDisabledGroup(!_createFoldersEnabled);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Select All")) SetAll(_foldersToCreate, _foldersDisabled, true);
                if (GUILayout.Button("Deselect All")) SetAll(_foldersToCreate, _foldersDisabled, false);
                EditorGUILayout.EndHorizontal();

                foreach (var folder in Folders.Keys.ToList())
                {
                    EditorGUI.BeginDisabledGroup(_foldersDisabled[folder]);
                    _foldersToCreate[folder] = EditorGUILayout.Toggle(folder, _foldersToCreate[folder]);
                    EditorGUI.EndDisabledGroup();
                }

                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;
            }
        }

        private void DrawPackagesSection()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            _installPackagesEnabled = EditorGUILayout.ToggleLeft("", _installPackagesEnabled, GUILayout.Width(20));
            _packagesFoldout = EditorGUILayout.Foldout(_packagesFoldout, "Packages to Install", true, EditorStyles.foldoutHeader);
            EditorGUILayout.EndHorizontal();

            if (_packagesFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUI.BeginDisabledGroup(!_installPackagesEnabled);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Select All")) SetAll(_packagesToInstall, _packagesDisabled, true);
                if (GUILayout.Button("Deselect All")) SetAll(_packagesToInstall, _packagesDisabled, false);
                EditorGUILayout.EndHorizontal();

                foreach (var package in Packages.ToList())
                {
                    EditorGUI.BeginDisabledGroup(_packagesDisabled[package]);
                    _packagesToInstall[package] = EditorGUILayout.Toggle(package, _packagesToInstall[package]);
                    EditorGUI.EndDisabledGroup();
                }

                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;
            }
        }

        private void DrawResourcesSection()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            _copyResourcesEnabled = EditorGUILayout.ToggleLeft("", _copyResourcesEnabled, GUILayout.Width(20));
            _resourcesFoldout = EditorGUILayout.Foldout(_resourcesFoldout, "Resources to Copy", true, EditorStyles.foldoutHeader);
            EditorGUILayout.EndHorizontal();

            if (_resourcesFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUI.BeginDisabledGroup(!_copyResourcesEnabled);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Select All")) SetAll(_resourcesToCopy, _resourcesDisabled, true);
                if (GUILayout.Button("Deselect All")) SetAll(_resourcesToCopy, _resourcesDisabled, false);
                EditorGUILayout.EndHorizontal();

                foreach (var resource in _resourcesToCopy.Keys.ToList())
                {
                    EditorGUI.BeginDisabledGroup(_resourcesDisabled[resource]);
                    _resourcesToCopy[resource] = EditorGUILayout.Toggle(resource, _resourcesToCopy[resource]);
                    EditorGUI.EndDisabledGroup();
                }

                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;
            }
        }

        private void InitializeProject()
        {
            Debug.Log("Starting project initialization...");

            if (_createFoldersEnabled)
            {
                var selectedFolders = _foldersToCreate.Where(x => x.Value).Select(x => x.Key);
                CreateFolders(selectedFolders);
            }

            if (_copyResourcesEnabled)
            {
                var selectedResources = _resourcesToCopy.Where(x => x.Value).Select(x => x.Key);
                CopyResources(selectedResources);
            }

            if (_installPackagesEnabled)
            {
                var selectedPackages = _packagesToInstall.Where(x => x.Value).Select(x => x.Key);
                AddAndResolvePackages(selectedPackages);
            }
            
            Debug.Log("Project initialization completed.");
            Close();
        }

        private void SetAll(Dictionary<string, bool> dictionary, Dictionary<string, bool> disabled, bool value)
        {
            foreach (var key in dictionary.Keys.ToList())
            {
                if (!disabled[key])
                {
                    dictionary[key] = value;
                }
            }
        }


        public static void CreateFolders(IEnumerable<string> foldersToCreate) {
            foreach (var folderPath in foldersToCreate) {
                if (!Folders.TryGetValue(folderPath, out var anchorConfig)) {
                    continue;
                }

                FileUtils.CreateDirectoryIfNeeded(folderPath);

                var folderName = Path.GetFileName(folderPath);
                var anchorPath = $"{folderPath}/_anchor_{folderName.ToLower()}.asset";

                if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(anchorPath) != null) continue;

                var anchor = CreateInstance<FileAnchor>();
                anchor.FileNamePrefix = anchorConfig.fileNamePrefix;
                anchor.ClassType = anchorConfig.classType;
                AssetDatabase.CreateAsset(anchor, anchorPath);
                Debug.Log("Created folder anchor: " + anchorPath);
            }

            AssetDatabase.Refresh();
        }
        
        public static IEnumerable<string> GetAvailableResources() {
            var packagePath = Path.GetFullPath("Packages/com.yanicksenn.projectinitializer");
            var resourcesPath = Path.Combine(packagePath, "Resources");

            if (!Directory.Exists(resourcesPath)) {
                return Enumerable.Empty<string>();
            }

            return Directory.GetFiles(resourcesPath, "resource_*")
                .Select(Path.GetFileName)
                .Select(fileName => fileName.Replace("resource_", ""))
                .Where(fileName => !fileName.EndsWith(".meta"));
        }

        public static void CopyResources(IEnumerable<string> resourcesToCopy) {
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

            foreach (var fileName in resourcesToCopy) {
                var resourceFileName = "resource_" + fileName;
                var resourceFile = Path.Combine(resourcesPath, resourceFileName);
                var newFilePath = Path.Combine(projectRootPath, fileName);

                if (!File.Exists(resourceFile)) {
                    Debug.LogWarning($"Resource file not found: {resourceFile}");
                    continue;
                }

                if (File.Exists(newFilePath)) {
                    continue;
                }
                
                File.Copy(resourceFile, newFilePath);
                Debug.Log($"Copied resource: {fileName}");
            }
        }

        public static void AddAndResolvePackages(IEnumerable<string> packagesToInstall) {
            var packagesList = packagesToInstall.ToArray();
            if (packagesList.Length > 0) {
                Debug.Log($"Installing {packagesList.Length} packages:\n{string.Join("\n", packagesList)}");
                Client.AddAndRemove(packagesList);
                Client.Resolve();
            }
        }
    }
}