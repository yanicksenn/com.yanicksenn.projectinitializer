namespace YanickSenn.ProjectInitializer.Editor
{
    /// <summary>
    /// Defines a violation payload containing all information needed
    /// to depict and fix the violation.
    /// </summary>
    public interface IViolation
    {
        /// <summary>
        /// Title of the violation.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Subtitle or detailed path of the violation.
        /// </summary>
        string SubTitle { get; }

        /// <summary>
        /// Description of what is wrong and how it will be fixed.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Whether this violation is selected for fixing.
        /// </summary>
        bool IsSelected { get; set; }
    }
}
