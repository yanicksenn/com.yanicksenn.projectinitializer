using System;
using System.Collections.Generic;
using UnityEngine;

namespace YanickSenn.ProjectInitializer.Editor.Anchors
{
    [CreateAssetMenu(fileName = "ModelsAnchor", menuName = "Project Initializer/Anchors/ModelsAnchor")]
    public class ModelsAnchor : AbstractAnchor
    {
        public override HashSet<Type> GetAssetTypes()
        {
            return new HashSet<Type>()
            {
                typeof(Mesh)
            };
        }

        public override IFileNamingStrategy GetFileNamingStrategy()
        {
            return new SnakeCaseWithPrefix("model");
        }
    }
}
