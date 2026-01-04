using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace YanickSenn.ProjectInitializer.Editor
{
    /// <summary>
    /// Registry that discovers and manages violation detectors.
    /// </summary>
    public static class ViolationDetectorRegistry
    {
        private static readonly List<IViolationDetector> Detectors = new();

        static ViolationDetectorRegistry()
        {
            var detectorTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetCustomAttribute<ViolationDetectorAttribute>() != null);

            foreach (var type in detectorTypes)
            {
                if (!typeof(IViolationDetector).IsAssignableFrom(type))
                {
                    Debug.LogError($"Type {type.Name} has ViolationDetectorAttribute but does not implement IViolationDetector");
                    continue;
                }

                try
                {
                    var detector = (IViolationDetector)Activator.CreateInstance(type);
                    Detectors.Add(detector);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to create instance of violation detector {type.Name}: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Gets all registered violation detectors.
        /// </summary>
        /// <returns>Enumerable of all detectors.</returns>
        public static IEnumerable<IViolationDetector> GetDetectors()
        {
            return Detectors;
        }
    }
}
