using System;
using System.Collections.Generic;
using UnityEngine;

namespace YanickSenn.ProjectInitializer.Editor.Anchors
{
    [CreateAssetMenu(fileName = "SettingsAnchor", menuName = "Project Initializer/Anchors/SettingsAnchor")]
    public class SettingsAnchor : AbstractAnchor
    {
        public override HashSet<Type> GetAssetTypes()
        {
            return new HashSet<Type>();
        }

        public override IFileNamingStrategy GetFileNamingStrategy()
        {
            return new SnakeCaseWithPrefix("settings");
        }
    }
}
