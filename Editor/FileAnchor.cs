using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using YanickSenn.Utils.Control;

namespace YanickSenn.ProjectInitializer.Editor {
    [CreateAssetMenu(fileName = "FileAnchor", menuName = "File Anchor")]
    public class FileAnchor : ScriptableObject {
        [SerializeField] private string fileNamePrefix;
        
        [SerializeField]
        [ClassTypeConstraint(baseType: typeof(UnityEngine.Object))]
        private ClassTypeReference classType = new();
        
        public string FileNamePrefix {
            get => fileNamePrefix;
            set => fileNamePrefix = value;
        }
        public Type ClassType {
            get => classType.Type;
            set => classType.Type = value;
        }

        public HashSet<Type> GetAssetTypes() {
            return new HashSet<Type> {
                classType.Type
            };
        }

        public IFileNamingStrategy GetFileNamingStrategy() {
            return new SnakeCaseWithPrefix(fileNamePrefix);
        }
        
        public string GetParentDirectory() {
            var assetPath = AssetDatabase.GetAssetPath(this);
            return Path.GetDirectoryName(assetPath);
        }
    }

    public class SnakeCaseWithPrefix : IFileNamingStrategy {
        private readonly string _filePrefix;

        public SnakeCaseWithPrefix(string filePrefix) {
            _filePrefix = filePrefix;
        }

        public bool Rename(string assetPath) {
            if (TryGetCorrectFileName(assetPath, out var newFileName)) {
                AssetDatabase.RenameAsset(assetPath, newFileName);
                return true;
            }
            return false;
        }

        public bool TryGetCorrectFileName(string assetPath, out string newFileName) {
            var fileName = Path.GetFileNameWithoutExtension(assetPath);
            var extension = Path.GetExtension(assetPath);
            var prefix = _filePrefix ?? "";

            var originalFileName = fileName;
            
            if (!string.IsNullOrEmpty(prefix)) {
                if (!fileName.StartsWith(prefix + "_")) {
                    fileName = prefix + "_" + fileName;
                }
            }

            var snakeCaseName = Regex.Replace(fileName, "(?<!^)(?<!_)([A-Z])", "_$1").ToLower();
            if (fileName != snakeCaseName) {
                fileName = snakeCaseName;
            }

            if (originalFileName != fileName) {
                newFileName = fileName + extension;
                return true;
            }

            newFileName = null;
            return false;
        }
    }
    
    public interface IFileNamingStrategy {
        bool Rename(string assetPath);
        bool TryGetCorrectFileName(string assetPath, out string newFileName);
    }
}