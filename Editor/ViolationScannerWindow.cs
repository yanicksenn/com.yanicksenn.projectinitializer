using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace YanickSenn.ProjectInitializer.Editor {
    public class ViolationScannerWindow : EditorWindow {
        private readonly List<IViolation> _violations = new();

        private Vector2 _scrollPosition;
        private bool _hasScanned;

        [MenuItem("Tools/Project Setup/Violation Scanner")]
        public static void ShowWindow() {
            GetWindow<ViolationScannerWindow>("Violation Scanner");
        }

        private void OnGUI() {
            GUILayout.Label("Project Violation Scanner", EditorStyles.boldLabel);

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

            var detectors = ViolationDetectorRegistry.GetDetectors();
            foreach (var detector in detectors) {
                try {
                    var detectedViolations = detector.Detect();
                    if (detectedViolations != null) {
                        _violations.AddRange(detectedViolations);
                    }
                } catch (Exception e) {
                    Debug.LogError($"Detector {detector.GetType().Name} failed: {e.Message}\n{e.StackTrace}");
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
                GUILayout.Label(violation.Title, EditorStyles.boldLabel);
                GUILayout.Label(violation.SubTitle, EditorStyles.miniLabel);
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
                var handler = ViolationHandlerRegistry.GetHandler(violation);
                if (handler != null) {
                    handler.Fix(violation);
                    executed++;
                    _violations.Remove(violation);
                }
            }
            
            if (executed > 0) {
                AssetDatabase.Refresh();
                Scan(); // Rescan to update list
            }
        }
    }
}
