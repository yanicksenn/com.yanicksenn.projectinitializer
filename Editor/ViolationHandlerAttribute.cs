using System;

namespace YanickSenn.ProjectInitializer.Editor
{
    /// <summary>
    /// Attribute to register a violation handler for a specific violation type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ViolationHandlerAttribute : Attribute
    {
        public Type ViolationType { get; }

        public ViolationHandlerAttribute(Type violationType)
        {
            ViolationType = violationType;
        }
    }
}
