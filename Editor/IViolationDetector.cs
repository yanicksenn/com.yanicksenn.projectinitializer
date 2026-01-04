using System.Collections.Generic;

namespace YanickSenn.ProjectInitializer.Editor
{
    /// <summary>
    /// Interface for a detector that scans the project for violations.
    /// </summary>
    public interface IViolationDetector
    {
        /// <summary>
        /// Scans the project and returns a list of violations.
        /// </summary>
        /// <returns>Enumerable of detected violations.</returns>
        IEnumerable<IViolation> Detect();
    }
}
