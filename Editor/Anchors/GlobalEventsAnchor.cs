using System;
using System.Collections.Generic;
using UnityEngine;
using YanickSenn.Utils;

namespace YanickSenn.ProjectInitializer.Editor.Anchors
{
    [CreateAssetMenu(fileName = "GlobalEventsAnchor", menuName = "Project Initializer/Anchors/GlobalEventsAnchor")]
    public class GlobalEventsAnchor : AbstractAnchor {
        public override HashSet<Type> GetAssetTypes() {
            return new HashSet<Type>() {
                typeof(GlobalEvent)
            };
        }

        public override IFileNamingStrategy GetFileNamingStrategy()
        {
            return new SnakeCaseWithPrefix("event");
        }
    }
}
