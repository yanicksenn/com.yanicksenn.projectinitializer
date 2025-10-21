using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YanickSenn.ProjectInitializer.Editor.Anchors
{
    [CreateAssetMenu(fileName = "ScriptsAnchor", menuName = "Project Initializer/Anchors/ScriptsAnchor")]
    public class ScriptsAnchor : AbstractAnchor
    {
        public override HashSet<Type> GetAssetTypes()
        {
            return new HashSet<Type>
            {
                typeof(MonoScript)
            };
        }

        public override IFileNamingStrategy GetFileNamingStrategy()
        {
            return new None();
        }
    }
}
