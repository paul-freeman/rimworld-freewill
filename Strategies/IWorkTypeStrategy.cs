using RimWorld;
using Verse;

namespace FreeWill
{
    /// <summary>
    /// Interface for work type specific priority calculation strategies.
    /// Each work type has its own unique combination of considerations.
    /// </summary>
    public interface IWorkTypeStrategy
    {
        /// <summary>
        /// The work type this strategy handles.
        /// </summary>
        WorkTypeDef WorkType { get; }

        /// <summary>
        /// Calculates the priority for the given work type using the provided priority calculator.
        /// </summary>
        /// <param name="priority">The priority calculator instance to use.</param>
        /// <returns>The priority instance after applying all considerations.</returns>
        Priority CalculatePriority(Priority priority);
    }
}
