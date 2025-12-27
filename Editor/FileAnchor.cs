using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using YanickSenn.Utils.Control;
using YanickSenn.Utils.RegistryGeneration;

namespace YanickSenn.ProjectInitializer.Editor {
    [CreateAssetMenu(fileName = "FileAnchor", menuName = "File Anchor")]
    public class FileAnchor : ScriptableObject {
        [SerializeField] private bool disableAutoFixing;
        [SerializeField] private string fileNamePrefix;
        
        [SerializeField]
        [ClassTypeConstraint(baseType: typeof(UnityEngine.Object))]
        private ClassTypeReference classType = new();

        public bool DisableAutoFixing {
            get => disableAutoFixing;
            set => disableAutoFixing = value;
        }
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
            var fileName = Path.GetFileNameWithoutExtension(assetPath);
            var extension = Path.GetExtension(assetPath);
            var directory = Path.GetDirectoryName(assetPath);

            var violationFound = false;
            
            if (!fileName.StartsWith(_filePrefix + "_")) {
                violationFound = true;
                fileName = _filePrefix + "_" + fileName;
            }

            var newFileName = Regex.Replace(fileName, "(?<!^)(?<!_)([A-Z])", "_$1").ToLower();
            if (fileName != newFileName) {
                violationFound = true;
                fileName = newFileName;
            }

            if (!violationFound) {
                return false;
            }

            var newPath = directory != null 
                ? Path.GetFileName(Path.Combine(directory, fileName + extension)) 
                : fileName + extension;
            AssetDatabase.RenameAsset(assetPath, newPath);
            return true;
        }
    }
    
    public interface IFileNamingStrategy {
        bool Rename(string assetPath);
    }
}