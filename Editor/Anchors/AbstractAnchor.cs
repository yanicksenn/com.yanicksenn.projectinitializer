using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace YanickSenn.ProjectInitializer.Editor.Anchors {
    public abstract class AbstractAnchor : ScriptableObject {
        public abstract HashSet<Type> GetAssetTypes();

        public virtual IFileNamingStrategy GetFileNamingStrategy() {
            return new None();
        }
        
        public string GetParentDirectory() {
            var assetPath = AssetDatabase.GetAssetPath(this);
            return Path.GetDirectoryName(assetPath);
        }
        
        
    }

    public class None : IFileNamingStrategy {
        public bool Rename(string assetPath) {
            // Do nothing.
            return false;
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