using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using YanickSenn.ProjectInitializer.Editor.Shared;

namespace YanickSenn.ProjectInitializer.Editor
{
    public class PackageCreatorWindow : EditorWindow {
        private string _packageName = "com.company.packagename";
        private string _version = "1.0.0";
        private string _displayName = "Package Name";
        private string _description = "This is a description of the package.";
        private string _rootNamespace = "MyPackage";
        private string _unityVersion;
        private string _unityRelease;
        private string _authorName = "Company Name";
        private string _authorEmail = "support@company.com";
        private string _authorUrl = "https://www.company.com";

        public static void ShowWindow()
        {
            GetWindow<PackageCreatorWindow>("Create Unity Package");
        }

        private void OnEnable() {
            string fullVersion = Application.unityVersion;
            var versionParts = fullVersion.Split('.');
            if (versionParts.Length >= 2)
            {
                _unityVersion = $"{versionParts[0]}.{versionParts[1]}";
            }
            if (versionParts.Length >= 3)
            {
                _unityRelease = versionParts[2];
            }
            LoadConfiguration();
        }

        private void LoadConfiguration() {
            var config = ProjectConfiguration.GetOrCreateSettings();
            if (config == null) return;
            _authorName = config.defaultAuthorName;
            _authorEmail = config.defaultAuthorEmail;
            _authorUrl = config.defaultAuthorUrl;
            _rootNamespace = config.defaultRootNamespace;
            _packageName = config.defaultPackageName;
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
            _packageName = EditorGUILayout.TextField("Package Name", _packageName);
        
            string packageRootPath = GetPackageRootPath();
            bool packageAlreadyExists = Directory.Exists(packageRootPath);
            if (packageAlreadyExists) {
                EditorGUILayout.HelpBox("Package already exists. Creating missing files only.", MessageType.Warning);
                return true;
            }
        
            GUI.enabled = !packageAlreadyExists;
            _version = EditorGUILayout.TextField("Version", _version);
            _displayName = EditorGUILayout.TextField("Display Name", _displayName);
            _description = EditorGUILayout.TextField("Description", _description);
            _rootNamespace = EditorGUILayout.TextField("Root Namespace", _rootNamespace);
            GUI.enabled = true;

            GUI.enabled = false;
            EditorGUILayout.TextField("Unity Version", _unityVersion);
            EditorGUILayout.TextField("Unity Release", _unityRelease);
            GUI.enabled = true;

            GUI.enabled = !packageAlreadyExists;
            _authorName = EditorGUILayout.TextField("Author Name", _authorName);
            _authorEmail = EditorGUILayout.TextField("Author Email", _authorEmail);
            _authorUrl = EditorGUILayout.TextField("Author URL", _authorUrl);
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

            if (!Regex.IsMatch(_packageName, @"^com\.([a-z0-9_]+)\.([a-z0-9_]+)$"))
            {
                validationErrors.Add("Package name must be in the format 'com.company.packagename'.");
            }

            if (!Regex.IsMatch(_version, @"^\d+\.\d+\.\d+$"))
            {
                validationErrors.Add("Version must be in the format 'major.minor.patch'.");
            }

            if (string.IsNullOrWhiteSpace(_displayName))
            {
                validationErrors.Add("Display name cannot be empty.");
            }

            if (!Regex.IsMatch(_rootNamespace, @"^[A-Z][a-zA-Z0-9]*(?:\.[A-Z][a-zA-Z0-9]*)*$"))
            {
                validationErrors.Add("Root Namespace must match the format 'CamelCase.CamelCase'.");
            }

            if (string.IsNullOrWhiteSpace(_authorName))
            {
                validationErrors.Add("Author name cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(_authorEmail) || !Regex.IsMatch(_authorEmail, @"^(.+)@(.+)$"))
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
            var packageJson = new PackageJson {
                name = _packageName,
                version = _version,
                displayName = _displayName,
                description = _description,
                unity = _unityVersion,
                unityRelease = _unityRelease,
                author = new Author {
                    name = _authorName,
                    email = _authorEmail,
                    url = _authorUrl
                }
            };
            
            string packageJsonContent = JsonUtility.ToJson(packageJson, true);
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
            string asmdefName = _rootNamespace;
            CreateAsmdef(runtimePath, asmdefName, _rootNamespace, AsmdefType.Runtime);
            CreateAsmdef(editorPath, asmdefName + ".Editor", _rootNamespace, AsmdefType.Editor, new[] { asmdefName });
            CreateAsmdef(runtimeTestsPath, asmdefName + ".Tests", _rootNamespace, AsmdefType.Tests,
                new[] { asmdefName, "UnityEngine.TestRunner", "UnityEditor.TestRunner" });
            CreateAsmdef(editorTestsPath, asmdefName + ".Editor.Tests", _rootNamespace, AsmdefType.EditorTests,
                new[] { asmdefName, asmdefName + ".Editor", "UnityEngine.TestRunner", "UnityEditor.TestRunner" });

            AssetDatabase.Refresh();
            Client.Resolve();
        }

        private string GetPackageRootPath() {
            return "Packages/" + _packageName;
        }

        private enum AsmdefType
        {
            Runtime, Editor, Tests, EditorTests
        }
    
        private void CreateAsmdef(string path, string name, string rootNamespace, AsmdefType type, string[] references = null) {
            var asmdefPath = Path.Combine(path, name + ".asmdef");
            var isEditor = type is AsmdefType.Editor or AsmdefType.EditorTests;
            var isTest = type is AsmdefType.Tests or AsmdefType.EditorTests;
            var asmdef = new Asmdef {
                name = name,
                rootNamespace = rootNamespace,
                references = references ?? Array.Empty<string>(),
                includePlatforms = isEditor ? new[] { "Editor" } : Array.Empty<string>(),
                excludePlatforms = Array.Empty<string>(),
                allowUnsafeCode = false,
                overrideReferences = isEditor,
                precompiledReferences = isEditor ? new[] { "nunit.framework.dll" } : Array.Empty<string>(),
                autoReferenced = !isTest,
                defineConstraints = isTest ? new[] { "UNITY_INCLUDE_TESTS" } : Array.Empty<string>(),
                versionDefines = Array.Empty<string>(),
                noEngineReferences = false
            };

            asmdef.rootNamespace = type switch {
                AsmdefType.Editor => $"{rootNamespace}.Editor",
                AsmdefType.Tests => $"{rootNamespace}.Tests",
                AsmdefType.EditorTests => $"{rootNamespace}.Editor.Tests",
                _ => asmdef.rootNamespace
            };

            string content = JsonUtility.ToJson(asmdef, true);
            File.WriteAllText(asmdefPath, content);
        }
    }
}