using System;
using System.Collections.Generic;
using UnityEngine;

namespace YanickSenn.ProjectInitializer.Editor.Anchors
{
    [CreateAssetMenu(fileName = "AudioAnchor", menuName = "Project Initializer/Anchors/AudioAnchor")]
    public class AudioAnchor : AbstractAnchor
    {
        public override HashSet<Type> GetAssetTypes()
        {
            return new HashSet<Type>
            {
                typeof(AudioClip)
            };
        }

        public override IFileNamingStrategy GetFileNamingStrategy()
        {
            return new SnakeCaseWithPrefix("audio");
        }
    }
}
