using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YanickSenn.ProjectInitializer.Editor.Anchors
{
    [CreateAssetMenu(fileName = "ScenesAnchor", menuName = "Project Initializer/Anchors/ScenesAnchor")]
    public class ScenesAnchor : AbstractAnchor
    {
        public override HashSet<Type> GetAssetTypes()
        {
            return new HashSet<Type>
            {
                typeof(SceneAsset)
            };
        }

        public override IFileNamingStrategy GetFileNamingStrategy()
        {
            return new SnakeCaseWithPrefix("scene");
        }
    }
}
