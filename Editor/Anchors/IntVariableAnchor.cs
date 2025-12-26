using System;
using System.Collections.Generic;
using UnityEngine;
using YanickSenn.Utils;

namespace YanickSenn.ProjectInitializer.Editor.Anchors
{
    [CreateAssetMenu(fileName = "IntVariableAnchor", menuName = "Project Initializer/Anchors/IntVariableAnchor")]
    public class IntVariableAnchor : VariableAnchor<int> {
        public override HashSet<Type> GetAssetTypes() {
            return new HashSet<Type>() {
                typeof(IntVariable)
            };
        }

        public override IFileNamingStrategy GetFileNamingStrategy() {
            return new SnakeCaseWithPrefix("int");
        }
    }
}
