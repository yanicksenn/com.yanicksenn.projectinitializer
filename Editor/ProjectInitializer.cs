using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace YanickSenn.ProjectInitializer.Editor
{
    public static class ProjectInitializer {
        [MenuItem("Tools/Project Setup/Initialize Project")]
        public static void InitializeProject() {
            CreateFolders();
            AddAndResolvePackages();
        }

        [MenuItem("Tools/Project Setup/Create Embedded Unity Package")]
        public static void ShowPackageCreatorWindow() {
            PackageCreatorWindow.ShowWindow();
        }

        private static void CreateFolders() {
            List<string> folders = new List<string> {
                "_Project",
                "Art",
                "Audio",
                "Materials",
                "Models",
                "Prefabs",
                "Scripts",
                "Settings",
                "Shaders",
                "Textures",
            };

            foreach (string folder in folders) {
                FileUtils.CreateDirectoryIfNeeded("Assets/" + folder);
            }

            List<string> projectSubFolders = new List<string> {
                "Scenes",
                "Timeline"
            };

            foreach (string subFolder in projectSubFolders) {
                FileUtils.CreateDirectoryIfNeeded("Assets/_Project/" + subFolder);
            }

            AssetDatabase.Refresh();
        }

        private static void AddAndResolvePackages()
        {
            Client.AddAndRemove(new[] {
                "com.unity.multiplayer.playmode",
                "com.unity.multiplayer.tools",
                "com.unity.netcode.gameobjects",
                "com.unity.postprocessing",
                "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.16.8"
            });
            Client.Resolve();
        }
    }

    public class PackageCreatorWindow : EditorWindow
    {
        private string packageName = "com.company.packagename";
        private string version = "1.0.0";
        private string displayName = "Package Name";
        private string description = "This is a description of the package.";
        private string rootNamespace = "MyPackage";
        private string unityVersion;
        private string unityRelease;
        private string authorName = "Company Name";
        private string authorEmail = "support@company.com";
        private string authorUrl = "https://www.company.com";

        public static void ShowWindow()
        {
            GetWindow<PackageCreatorWindow>("Create Unity Package");
        }

        private void OnEnable() {
            string fullVersion = Application.unityVersion;
            var versionParts = fullVersion.Split('.');
            if (versionParts.Length >= 2)
            {
                unityVersion = $"{versionParts[0]}.{versionParts[1]}";
            }
            if (versionParts.Length >= 3)
            {
                unityRelease = versionParts[2];
            }
        }

        private void OnGUI()
        {
            var createEnabled = DrawForm();

            GUI.enabled = createEnabled;
            if (GUILayout.Button("Create")) {
                CreatePackageLayout();
                Close();
            }
            GUI.enabled = true;

            if (GUILayout.Button("Cancel")) {
                Close();
            }
        }

        private bool DrawForm()
        {
            GUILayout.Label("Package Information", EditorStyles.boldLabel);
            packageName = EditorGUILayout.TextField("Package Name", packageName);
        
            string packageRootPath = GetPackageRootPath();
            bool packageAlreadyExists = Directory.Exists(packageRootPath);
            if (packageAlreadyExists) {
                EditorGUILayout.HelpBox("Package already exists. Creating missing files only.", MessageType.Warning);
                return true;
            }
        
            GUI.enabled = !packageAlreadyExists;
            version = EditorGUILayout.TextField("Version", version);
            displayName = EditorGUILayout.TextField("Display Name", displayName);
            description = EditorGUILayout.TextField("Description", description);
            rootNamespace = EditorGUILayout.TextField("Root Namespace", rootNamespace);
            GUI.enabled = true;

            GUI.enabled = false;
            EditorGUILayout.TextField("Unity Version", unityVersion);
            EditorGUILayout.TextField("Unity Release", unityRelease);
            GUI.enabled = true;

            GUI.enabled = !packageAlreadyExists;
            authorName = EditorGUILayout.TextField("Author Name", authorName);
            authorEmail = EditorGUILayout.TextField("Author Email", authorEmail);
            authorUrl = EditorGUILayout.TextField("Author URL", authorUrl);
            GUI.enabled = true;

            List<string> validationErrors = ValidateInput();
            if (validationErrors.Count > 0) {
                foreach (string error in validationErrors) {
                    EditorGUILayout.HelpBox(error, MessageType.Error);
                }
            }

            return validationErrors.Count == 0;
        }

        private List<string> ValidateInput()
        {
            List<string> validationErrors = new List<string>();

            if (!Regex.IsMatch(packageName, @"^com\.([a-z0-9_]+)\.([a-z0-9_]+)$"))
            {
                validationErrors.Add("Package name must be in the format 'com.company.packagename'.");
            }

            if (!Regex.IsMatch(version, @"^\d+\.\d+\.\d+$"))
            {
                validationErrors.Add("Version must be in the format 'major.minor.patch'.");
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                validationErrors.Add("Display name cannot be empty.");
            }

            if (!Regex.IsMatch(rootNamespace, @"^[A-Z][a-zA-Z0-9]*(?:\.[A-Z][a-zA-Z0-9]*)*$"))
            {
                validationErrors.Add("Root Namespace must match the format 'CamelCase.CamelCase'.");
            }

            if (string.IsNullOrWhiteSpace(authorName))
            {
                validationErrors.Add("Author name cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(authorEmail) || !Regex.IsMatch(authorEmail, @"^(.+)@(.+)$"))
            {
                validationErrors.Add("Author email must be a valid email address.");
            }
            return validationErrors;
        }

        private void CreatePackageLayout() {
            string packageRootPath = GetPackageRootPath();
            FileUtils.CreateDirectoryIfNeeded(packageRootPath);

            // Create package.json
            string packageJsonPath = Path.Combine(packageRootPath, "package.json");
            string packageJsonContent = $@"
{{
    ""name"": ""{packageName}"",
    ""version"": ""{version}"",
    ""displayName"": ""{displayName}"",
    ""description"": ""{description}"",
    ""unity"": ""{unityVersion}"",
    ""unityRelease"": ""{unityRelease}"",
    ""author"": {{
        ""name"": ""{authorName}"",
        ""email"": ""{authorEmail}"",
        ""url"": ""{authorUrl}"" 
    }}
}}
";
            FileUtils.CreateFileIfNeeded(packageJsonPath, packageJsonContent);

            // Create other files
            FileUtils.CreateFileIfNeeded(Path.Combine(packageRootPath, "README.md"));
            FileUtils.CreateFileIfNeeded(Path.Combine(packageRootPath, "CHANGELOG.md"));
            FileUtils.CreateFileIfNeeded(Path.Combine(packageRootPath, "LICENSE.md"));

            // Create directories
            string runtimePath = Path.Combine(packageRootPath, "Runtime");
            string editorPath = Path.Combine(packageRootPath, "Editor");
            string testsPath = Path.Combine(packageRootPath, "Tests");
            string runtimeTestsPath = Path.Combine(testsPath, "Runtime");
            string editorTestsPath = Path.Combine(testsPath, "Editor");

            FileUtils.CreateDirectoryIfNeeded(runtimePath);
            FileUtils.CreateDirectoryIfNeeded(editorPath);
            FileUtils.CreateDirectoryIfNeeded(testsPath);
            FileUtils.CreateDirectoryIfNeeded(runtimeTestsPath);
            FileUtils.CreateDirectoryIfNeeded(editorTestsPath);

            // Create asmdef files
            string asmdefName = rootNamespace;
            CreateAsmdef(runtimePath, asmdefName, rootNamespace, AsmdefType.Runtime);
            CreateAsmdef(editorPath, asmdefName + ".Editor", rootNamespace, AsmdefType.Editor, new[] { asmdefName });
            CreateAsmdef(runtimeTestsPath, asmdefName + ".Tests", rootNamespace, AsmdefType.Tests,
                new[] { asmdefName, "UnityEngine.TestRunner", "UnityEditor.TestRunner" });
            CreateAsmdef(editorTestsPath, asmdefName + ".Editor.Tests", rootNamespace, AsmdefType.EditorTests,
                new[] { asmdefName, asmdefName + ".Editor", "UnityEngine.TestRunner", "UnityEditor.TestRunner" });

            AssetDatabase.Refresh();
            Client.Resolve();
        }

        private string GetPackageRootPath() {
            return "Packages/" + packageName;
        }

        private enum AsmdefType
        {
            Runtime, Editor, Tests, EditorTests
        }
    
        private void CreateAsmdef(string path, string name, string rootNamespace, AsmdefType type, string[] references = null) {
            string asmdefPath = Path.Combine(path, name + ".asmdef");
            string content = type switch {
                AsmdefType.Runtime => CreateRuntimeAsmdefContent(name, rootNamespace),
                AsmdefType.Editor => CreateEditorAsmdefContent(name, rootNamespace, references),
                AsmdefType.Tests => CreateRuntimeTestsAsmdefContent(name, rootNamespace, references),
                AsmdefType.EditorTests => CreateEditorTestsAsmdefContent(name, rootNamespace, references),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            File.WriteAllText(asmdefPath, content);
        }

        private static string CreateEditorTestsAsmdefContent(string name, string rootNamespace, string[] references) {
            return $@"
{{
    ""name"": ""{name}"",
    ""rootNamespace"": ""{rootNamespace}.Editor.Tests"",
    ""references"": [""{string.Join("\", \"", references)}""],
    ""includePlatforms"": [""Editor""],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": true,
    ""precompiledReferences"": [""nunit.framework.dll""],
    ""autoReferenced"": false,
    ""defineConstraints"": [""UNITY_INCLUDE_TESTS""],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}}
";
        }

        private static string CreateRuntimeTestsAsmdefContent(string name, string rootNamespace, string[] references) {
            return $@"
{{
    ""name"": ""{name}"",
    ""rootNamespace"": ""{rootNamespace}.Tests"",
    ""references"": [""{string.Join("\", \"", references)}""],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": true,
    ""precompiledReferences"": [""nunit.framework.dll""],
    ""autoReferenced"": false,
    ""defineConstraints"": [""UNITY_INCLUDE_TESTS""],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}}
";
        }

        private static string CreateEditorAsmdefContent(string name, string rootNamespace, string[] references) {
            return $@"
{{
    ""name"": ""{name}"",
    ""rootNamespace"": ""{rootNamespace}.Editor"",
    ""references"": [""{string.Join("\", \"", references)}""],
    ""includePlatforms"": [""Editor""],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}}
";
        }

        private static string CreateRuntimeAsmdefContent(string name, string rootNamespace) {
            return $@"
{{
    ""name"": ""{name}"",
    ""rootNamespace"": ""{rootNamespace}"",
    ""references"": [],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}}
";
        }
    }
    public static class FileUtils {
        public static void CreateDirectoryIfNeeded(string path) {
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
        }

        public static void CreateFileIfNeeded(string path, string content = null) {
            if (File.Exists(path)) return;
            if (content != null) {
                File.WriteAllText(path, content);
            } else {
                File.Create(path).Dispose();
            }
        }
    }
}