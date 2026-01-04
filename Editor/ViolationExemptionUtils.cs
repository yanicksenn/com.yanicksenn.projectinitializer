using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace YanickSenn.ProjectInitializer.Editor {
    internal static class ViolationExemptionUtils {
        private static List<string> _exemptionDirectories;

        public static void Refresh() {
            var guids = AssetDatabase.FindAssets($"t:{typeof(ViolationExemption)}");
            _exemptionDirectories = guids
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => Path.GetDirectoryName(path).Replace("\\", "/"))
                .ToList();
        }

        public static bool IsExempt(string assetPath) {
            if (_exemptionDirectories == null) {
                Refresh();
            }

            var normalizedPath = assetPath.Replace("\\", "/");
            return _exemptionDirectories.Any(dir => normalizedPath.StartsWith(dir + "/"));
        }
    }
}
