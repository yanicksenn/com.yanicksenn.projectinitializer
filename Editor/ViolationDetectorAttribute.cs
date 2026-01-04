using System;

namespace YanickSenn.ProjectInitializer.Editor
{
    /// <summary>
    /// Attribute to register a violation detector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ViolationDetectorAttribute : Attribute
    {
    }
}
