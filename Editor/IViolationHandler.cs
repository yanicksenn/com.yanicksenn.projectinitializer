namespace YanickSenn.ProjectInitializer.Editor
{
    /// <summary>
    /// Interface for a handler that can resolve a specific type of violation.
    /// </summary>
    public interface IViolationHandler
    {
        /// <summary>
        /// Fixes the given violation.
        /// </summary>
        /// <param name="violation">Violation to fix.</param>
        void Fix(IViolation violation);
    }
}
