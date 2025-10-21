using System;
using System.Collections.Generic;
using UnityEngine;

namespace YanickSenn.ProjectInitializer.Editor.Anchors
{
    [CreateAssetMenu(fileName = "PrefabsAnchor", menuName = "Project Initializer/Anchors/PrefabsAnchor")]
    public class PrefabsAnchor : AbstractAnchor
    {
        public override HashSet<Type> GetAssetTypes()
        {
            return new HashSet<Type>
            {
                typeof(GameObject)
            };
        }

        public override IFileNamingStrategy GetFileNamingStrategy()
        {
            return new SnakeCaseWithPrefix("prefab");
        }
    }
}
