using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace YanickSenn.ProjectInitializer.Editor {
    public class StructureViolationsWindow : EditorWindow {
        private List<Violation> _violations = new();
        private Vector2 _scrollPosition;
        private bool _hasScanned = false;

        [MenuItem("Tools/Project Setup/Structure Violations")]
        public static void ShowWindow() {
            GetWindow<StructureViolationsWindow>("Structure Violations");
        }

        private void OnGUI() {
            GUILayout.Label("Project Structure Violations", EditorStyles.boldLabel);

            if (GUILayout.Button("Scan for Violations")) {
                Scan();
            }

            if (!_hasScanned) {
                return;
            }

            if (_violations.Count == 0) {
                EditorGUILayout.HelpBox("No violations found!", MessageType.Info);
            } else {
                DrawViolationsList();
                DrawActions();
            }
        }

        private void Scan() {
            _violations.Clear();
            _hasScanned = true;

            var anchorGuids = AssetDatabase.FindAssets($"t:{typeof(FileAnchor)}");
            var anchors = anchorGuids
                .Select(guid => AssetDatabase.LoadAssetAtPath<FileAnchor>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(a => a != null)
                .ToList();

            var assetClaims = new Dictionary<string, List<Claim>>();
            foreach (var anchor in anchors) {
                var validTypes = anchor.GetAssetTypes()
                    ?.Where(t => t != null)
                    .ToList() ?? new List<Type>();

                var anchorDirectory = anchor.GetParentDirectory().Replace("\\", "/");
                
                var globalGuids = Enumerable.Empty<string>();
                if (validTypes.Count > 0) {
                    var filter = $"t:{string.Join(",", validTypes.Select(t => t.Name))}";
                    globalGuids = AssetDatabase.FindAssets(filter);
                }
                
                var localGuids = AssetDatabase.FindAssets("t:Object", new[] { anchorDirectory });
                var assetGuids = globalGuids.Union(localGuids);
                
                var anchorPath = AssetDatabase.GetAssetPath(anchor);

                foreach (var assetGuid in assetGuids) {
                    var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                    if (!assetPath.StartsWith("Assets/")) {
                        continue;
                    }

                    if (assetPath == anchorPath) {
                        continue;
                    }

                    var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                    if (asset == null) {
                        continue;
                    }

                    var isTypeMatch = validTypes.Any(t => t.IsAssignableFrom(asset.GetType()));
                    var isNested = assetPath.StartsWith(anchorDirectory + "/");

                    if (!isTypeMatch && !isNested) {
                        continue;
                    }

                    if (!assetClaims.ContainsKey(assetPath)) {
                        assetClaims[assetPath] = new List<Claim>();
                    }

                    Violation violation = null;
                    var currentDirectory = Path.GetDirectoryName(assetPath).Replace("\\", "/");
                    var currentFileName = Path.GetFileName(assetPath);

                    var targetDirectory = anchorDirectory;
                    
                    var namingStrategy = anchor.GetFileNamingStrategy();
                    var targetFileName = currentFileName;
                    if (namingStrategy.TryGetCorrectFileName(assetPath, out var correctName)) {
                        targetFileName = correctName;
                    }

                    var targetPath = Path.Combine(targetDirectory, targetFileName).Replace("\\", "/");
                    if (assetPath != targetPath) {
                        var description = new List<string>();
                        if (currentDirectory != targetDirectory) description.Add($"Move to {targetDirectory}");
                        if (currentFileName != targetFileName) description.Add($"Rename to {targetFileName}");

                        violation = new Violation {
                            AssetPath = assetPath,
                            TargetPath = targetPath,
                            Description = string.Join(", ", description),
                            IsSelected = true
                        };
                    }

                    assetClaims[assetPath].Add(new Claim {
                        Anchor = anchor,
                        Violation = violation
                    });
                }
            }

            foreach (var (_, claims) in assetClaims) {
                if (claims.Any(claim => claim.Anchor.ContentDoesNotProduceViolations)) {
                    continue;
                }

                foreach (var claim in claims) {
                    if (claim.Violation != null) {
                        _violations.Add(claim.Violation);
                    }
                }
            }
        }

        private void DrawViolationsList() {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All")) SetAll(true);
            if (GUILayout.Button("Deselect All")) SetAll(false);
            EditorGUILayout.EndHorizontal();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            foreach (var violation in _violations) {
                EditorGUILayout.BeginHorizontal("box");
                violation.IsSelected = EditorGUILayout.Toggle(violation.IsSelected, GUILayout.Width(20));
                
                EditorGUILayout.BeginVertical();
                GUILayout.Label(Path.GetFileName(violation.AssetPath), EditorStyles.boldLabel);
                GUILayout.Label(violation.AssetPath, EditorStyles.miniLabel);
                GUILayout.Label($"-> {violation.Description}", EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawActions() {
            var selectedCount = _violations.Count(v => v.IsSelected);
            EditorGUI.BeginDisabledGroup(selectedCount == 0);
            if (GUILayout.Button($"Fix Selected ({selectedCount})")) {
                FixViolations();
            }
            EditorGUI.EndDisabledGroup();
        }

        private void SetAll(bool selected) {
            foreach (var v in _violations) v.IsSelected = selected;
        }

        private void FixViolations() {
            var executed = 0;
            foreach (var violation in _violations.Where(v => v.IsSelected).ToList()) {
                var error = AssetDatabase.MoveAsset(violation.AssetPath, violation.TargetPath);
                if (string.IsNullOrEmpty(error)) {
                    executed++;
                    _violations.Remove(violation);
                } else {
                    Debug.LogError($"Failed to fix {violation.AssetPath}: {error}");
                }
            }
            
            if (executed > 0) {
                AssetDatabase.Refresh();
                Scan(); // Rescan to update list
            }
        }
    }
}
