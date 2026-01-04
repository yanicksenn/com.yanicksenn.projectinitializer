using System.IO;

namespace YanickSenn.ProjectInitializer.Editor
{
    internal class AssetLocationViolation : IViolation
    {
        public string AssetPath;
        public string TargetPath;
        
        public string Title => Path.GetFileName(AssetPath);
        public string SubTitle => AssetPath;
        public string Description { get; set; }
        public bool IsSelected { get; set; } = true;
    }
}
