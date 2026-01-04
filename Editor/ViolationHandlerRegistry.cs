using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace YanickSenn.ProjectInitializer.Editor
{
    /// <summary>
    /// Registry that discovers and manages violation handlers.
    /// </summary>
    public static class ViolationHandlerRegistry
    {
        private static readonly Dictionary<Type, IViolationHandler> Handlers = new();

        static ViolationHandlerRegistry()
        {
            var handlerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.GetCustomAttribute<ViolationHandlerAttribute>() != null);

            foreach (var type in handlerTypes)
            {
                var attribute = type.GetCustomAttribute<ViolationHandlerAttribute>();
                var violationType = attribute.ViolationType;

                if (Handlers.ContainsKey(violationType))
                {
                    Debug.LogError($"Duplicate violation handler found for type {violationType.Name}: {type.Name} and {Handlers[violationType].GetType().Name}");
                    continue;
                }

                if (!typeof(IViolationHandler).IsAssignableFrom(type))
                {
                    Debug.LogError($"Type {type.Name} has ViolationHandlerAttribute but does not implement IViolationHandler");
                    continue;
                }

                try
                {
                    var handler = (IViolationHandler)Activator.CreateInstance(type);
                    Handlers.Add(violationType, handler);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to create instance of violation handler {type.Name}: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Gets the handler for the specified violation.
        /// </summary>
        /// <param name="violation">Violation to get a handler for.</param>
        /// <returns>The handler, or null if none is found.</returns>
        public static IViolationHandler GetHandler(IViolation violation)
        {
            if (violation == null) return null;
            
            var violationType = violation.GetType();
            if (Handlers.TryGetValue(violationType, out var handler))
            {
                return handler;
            }

            Debug.LogWarning($"No violation handler registered for violation type {violationType.Name}");
            return null;
        }
    }
}
