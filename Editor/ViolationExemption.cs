using UnityEngine;

namespace YanickSenn.ProjectInitializer.Editor
{
    [CreateAssetMenu(fileName = "ViolationExemption", menuName = "Violation Exemption")]
    public class ViolationExemption : ScriptableObject
    {
        [TextArea]
        public string Description;
    }
}
