using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YanickSenn.ProjectInitializer.Editor
{
    static class ProjectConfigurationProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            var provider = new SettingsProvider("Project/YanickSenn/Project Initializer", SettingsScope.Project)
            {
                label = "Project Initializer",
                guiHandler = (searchContext) =>
                {
                    var settings = ProjectConfiguration.GetSerializedSettings();
                    settings.Update();
                    
                    EditorGUILayout.PropertyField(settings.FindProperty(nameof(ProjectConfiguration.defaultAuthorName)));
                    EditorGUILayout.PropertyField(settings.FindProperty(nameof(ProjectConfiguration.defaultAuthorEmail)));
                    EditorGUILayout.PropertyField(settings.FindProperty(nameof(ProjectConfiguration.defaultAuthorUrl)));
                    EditorGUILayout.PropertyField(settings.FindProperty(nameof(ProjectConfiguration.defaultRootNamespace)));
                    EditorGUILayout.PropertyField(settings.FindProperty(nameof(ProjectConfiguration.defaultPackageName)));

                    settings.ApplyModifiedProperties();
                },

                keywords = new HashSet<string>(new[] { "Project", "Initializer", "Author", "Namespace" })
            };

            return provider;
        }
    }
}
