using Verse;

namespace FreeWill
{
    /// <summary>
    /// Base class for work type strategies providing common functionality.
    /// Contains the work type definition and shared logic.
    /// </summary>
    public abstract class BaseWorkTypeStrategy : IWorkTypeStrategy
    {
        public abstract WorkTypeDef WorkType { get; }

        public abstract Priority CalculatePriority(Priority priority);

        /// <summary>
        /// Gets a work type definition by its def name.
        /// </summary>
        /// <param name="defName">The def name of the work type.</param>
        /// <returns>The work type definition, or null if not found.</returns>
        protected WorkTypeDef GetWorkTypeDef(string defName)
        {
            return DefDatabase<WorkTypeDef>.GetNamedSilentFail(defName);
        }
    }
}
