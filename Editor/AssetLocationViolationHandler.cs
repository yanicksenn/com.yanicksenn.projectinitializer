using UnityEditor;
using UnityEngine;

namespace YanickSenn.ProjectInitializer.Editor
{
    [ViolationHandler(typeof(AssetLocationViolation))]
    internal class AssetLocationViolationHandler : IViolationHandler
    {
        public void Fix(IViolation violation)
        {
            if (violation is AssetLocationViolation assetViolation)
            {
                var error = AssetDatabase.MoveAsset(assetViolation.AssetPath, assetViolation.TargetPath);
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError($"Failed to fix {assetViolation.AssetPath}: {error}");
                }
            }
        }
    }
}
