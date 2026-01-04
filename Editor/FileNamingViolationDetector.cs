using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace YanickSenn.ProjectInitializer.Editor
{
        [ViolationDetector]
        public class FileNamingViolationDetector : IViolationDetector
        {
            public IEnumerable<IViolation> Detect()
            {
                ViolationExemptionUtils.Refresh();
                var violations = new List<IViolation>();
    
                var anchorGuids = AssetDatabase.FindAssets($"t:{typeof(FileAnchor)}");
                var anchors = anchorGuids
                    .Select(guid => AssetDatabase.LoadAssetAtPath<FileAnchor>(AssetDatabase.GUIDToAssetPath(guid)))
                    .Where(a => a != null)
                    .ToList();
    
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
                        
                        // Check for Naming Violation
                        var namingStrategy = anchor.GetFileNamingStrategy();
                        if (namingStrategy.TryGetCorrectFileName(assetPath, out var correctName)) {
                            var namingViolation = new FileNamingViolation {
                                AssetPath = assetPath,
                                TargetName = correctName,
                                IsSelected = true
                            };
                            violations.Add(namingViolation);
                        }
                    }
                }
    
                return violations;
            }
        }
}