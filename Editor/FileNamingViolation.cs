using System.IO;

namespace YanickSenn.ProjectInitializer.Editor
{
    internal class FileNamingViolation : IViolation
    {
        public string AssetPath;
        public string TargetName;

        public string Title => Path.GetFileName(AssetPath);
        public string SubTitle => AssetPath;
        public string Description => $"Rename to {TargetName}";
        public bool IsSelected { get; set; } = true;
    }
}
