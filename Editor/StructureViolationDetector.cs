using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace YanickSenn.ProjectInitializer.Editor {
    [ViolationDetector]
    public class StructureViolationDetector : IViolationDetector {
        public IEnumerable<IViolation> Detect() {
            ViolationExemptionUtils.Refresh();
            var violations = new List<IViolation>();

            var anchorGuids = AssetDatabase.FindAssets($"t:{typeof(FileAnchor)}");
            var anchors = anchorGuids
                .Select(guid => AssetDatabase.LoadAssetAtPath<FileAnchor>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(a => a != null)
                .ToList();

            // We need to track processed violations to avoid duplicates if multiple anchors report the same issue
            // but distinct violations (e.g. same move target) are fine to just list.
            // However, the previous logic grouped by asset path.
            
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

                    if (ViolationExemptionUtils.IsExempt(assetPath)) {
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

                    var currentDirectory = Path.GetDirectoryName(assetPath).Replace("\\", "/");
                    var currentFileName = Path.GetFileName(assetPath);

                    var targetDirectory = anchorDirectory;
                    
                    // Check for Location Violation
                    if (currentDirectory == targetDirectory) {
                        continue;
                    }

                    // Use current filename for the target path to isolate location change
                    var targetPath = Path.Combine(targetDirectory, currentFileName).Replace("\\", "/");
                    var locationViolation = new AssetLocationViolation {
                        AssetPath = assetPath,
                        TargetPath = targetPath,
                        Description = $"Move to {targetDirectory}",
                        IsSelected = true
                    };
                    violations.Add(locationViolation);
                }
            }

            // Deduplicate violations if necessary? 
            // If multiple anchors claim the same asset and want to move it to different places, that's a conflict.
            // But let's just return all found violations for now.

            return violations;
        }
    }
}
