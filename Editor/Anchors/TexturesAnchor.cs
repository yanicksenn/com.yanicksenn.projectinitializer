using System;
using System.Collections.Generic;
using UnityEngine;

namespace YanickSenn.ProjectInitializer.Editor.Anchors
{
    [CreateAssetMenu(fileName = "TexturesAnchor", menuName = "Project Initializer/Anchors/TexturesAnchor")]
    public class TexturesAnchor : AbstractAnchor
    {
        public override HashSet<Type> GetAssetTypes()
        {
            return new HashSet<Type>
            {
                typeof(Texture)
            };
        }

        public override IFileNamingStrategy GetFileNamingStrategy()
        {
            return new SnakeCaseWithPrefix("texture");
        }
    }
}
