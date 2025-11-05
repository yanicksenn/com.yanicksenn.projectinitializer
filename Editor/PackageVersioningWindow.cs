using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using YanickSenn.ProjectInitializer.Editor.Shared;

namespace YanickSenn.ProjectInitializer.Editor
{
    public class PackageVersioningWindow : EditorWindow {
        private string[] _packages;
        private int _selectedPackageIndex;
        private string _selectedPackagePath;
        private string _version;
        private string _originalVersion;

        public static void ShowWindow() {
            GetWindow<PackageVersioningWindow>("Bump Package Version");
        }

        private void OnEnable() {
            _packages = Directory.GetDirectories("Packages/", "com.*")
                .Select(Path.GetFileName)
                .ToArray();

            if (_packages.Length > 0) {
                SelectPackage(0);
            }
        }

        private void OnGUI() {
            if (_packages.Length == 0) {
                EditorGUILayout.HelpBox("No packages found.", MessageType.Warning);
                return;
            }

            var newSelectedPackage = EditorGUILayout.Popup("Package", _selectedPackageIndex, _packages);
            if (newSelectedPackage != _selectedPackageIndex) {
                SelectPackage(newSelectedPackage);
            }

            EditorGUILayout.LabelField("Version", _version);

            if (GUILayout.Button("Bump Major")) {
                BumpVersion(VersionComponent.Major);
            }

            if (GUILayout.Button("Bump Minor")) {
                BumpVersion(VersionComponent.Minor);
            }

            if (GUILayout.Button("Bump Patch")) {
                BumpVersion(VersionComponent.Patch);
            }

            GUI.enabled = _version != _originalVersion;
            if (GUILayout.Button("Release")) {
                if (EditorUtility.DisplayDialog("Confirm Release", 
                    $"Are you sure you want to release version {_version} for package {_packages[_selectedPackageIndex]}?", 
                    "Release", 
                    "Cancel"))
                {
                    Release();
                }
            }
            GUI.enabled = true;
        }

        private void SelectPackage(int index) {
            _selectedPackageIndex = index;
            _selectedPackagePath = Path.GetFullPath("Packages/" + _packages[index]);
            _version = GetPackageVersion();
            _originalVersion = _version;
        }

        private string GetPackageVersion() {
            var packageJsonPath = Path.Combine(_selectedPackagePath, "package.json");
            if (!File.Exists(packageJsonPath)) return "0.0.0";
            var packageJson = File.ReadAllText(packageJsonPath);
            var packageInfo = JsonUtility.FromJson<PackageJson>(packageJson);
            return packageInfo.version;
        }

        private void BumpVersion(VersionComponent component) {
            var versionParts = _version.Split('.').Select(int.Parse).ToArray();
            if (versionParts.Length != 3) {
                throw new ArgumentOutOfRangeException(
                    $"Expected 3 components (major, minor, patch) in package version but got {versionParts.Length} instead");
            }

            switch (component) {
                case VersionComponent.Major:
                    versionParts[0]++;
                    versionParts[1] = 0;
                    versionParts[2] = 0;
                    break;
                case VersionComponent.Minor:
                    versionParts[1]++;
                    versionParts[2] = 0;
                    break;
                case VersionComponent.Patch:
                    versionParts[2]++;
                    break;
            }

            _version = string.Join(".", versionParts);
            SetPackageVersion(_version);
        }

        private void SetPackageVersion(string newVersion) {
            var packageJsonPath = Path.Combine(_selectedPackagePath, "package.json");
            if (!File.Exists(packageJsonPath)) return;
            var packageJson = File.ReadAllText(packageJsonPath);
            var packageInfo = JsonConvert.DeserializeObject<PackageJson>(packageJson);
            packageInfo.version = newVersion;
            JsonConvert.SerializeObject(packageInfo);
            var newPackageJson = JsonConvert.SerializeObject(packageInfo, Formatting.Indented);
            File.WriteAllText(packageJsonPath, newPackageJson);
        }

        private void Release() {
            try {
                EditorUtility.DisplayProgressBar("Releasing Package", "Adding files to git...", 0.2f);
                RunGitCommand($"add .", _selectedPackagePath);

                EditorUtility.DisplayProgressBar("Releasing Package", "Committing changes...", 0.4f);
                RunGitCommand($"commit -m \"chore(release): {_version}\"", _selectedPackagePath);
                
                EditorUtility.DisplayProgressBar("Releasing Package", "Tagging release...", 0.8f);
                RunGitCommand($"tag -a \"{_version}\" -m \"chore(tag): {_version}\"", _selectedPackagePath);

                EditorUtility.DisplayProgressBar("Releasing Package", "Pushing commits to remote...", 1.0f);
                RunGitCommand("push origin main", _selectedPackagePath);

                EditorUtility.DisplayProgressBar("Releasing Package", "Pushing tags to remote...", 1.0f);
                RunGitCommand($"push origin \"{_version}\"", _selectedPackagePath);
            }
            finally {
                EditorUtility.ClearProgressBar();
            }
        }

        private void RunGitCommand(string command, string workingDirectory) {
            var process = new System.Diagnostics.Process {
                StartInfo = {
                    FileName = "git",
                    Arguments = command,
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0) {
                Debug.LogError($"Error running git command: {command}\n{error}");
            } else {
                Debug.Log($"Git command successful: {command}\n{output}");
            }
        }


        private enum VersionComponent {
            Major,
            Minor,
            Patch
        }
    }
}