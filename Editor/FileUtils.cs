using System.IO;

namespace YanickSenn.ProjectInitializer.Editor
{
    public static class FileUtils {
        public static void CreateDirectoryIfNeeded(string path) {
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
        }

        public static void CreateFileIfNeeded(string path, string content = null) {
            if (File.Exists(path)) return;
            if (content != null) {
                File.WriteAllText(path, content);
            } else {
                File.Create(path).Dispose();
            }
        }
    }
}