using UnityEditor;
using UnityEngine;

namespace YanickSenn.ProjectInitializer.Editor
{
    [ViolationHandler(typeof(FileNamingViolation))]
    internal class FileNamingViolationHandler : IViolationHandler
    {
        public void Fix(IViolation violation)
        {
            if (violation is FileNamingViolation namingViolation)
            {
                var error = AssetDatabase.RenameAsset(namingViolation.AssetPath, namingViolation.TargetName);
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError($"Failed to rename {namingViolation.AssetPath} to {namingViolation.TargetName}: {error}");
                }
            }
        }
    }
}
