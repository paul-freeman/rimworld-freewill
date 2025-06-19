using RimWorld;
using Verse;

namespace FreeWill
{
    /// <summary>
    /// Default strategy for work types that don't have specific implementations.
    /// Provides a comprehensive set of standard considerations.
    /// </summary>
    public class DefaultWorkTypeStrategy : BaseWorkTypeStrategy
    {
        public override WorkTypeDef WorkType => null; // This strategy handles any work type

        public override Priority CalculatePriority(Priority priority)
        {
            return priority
                .ConsiderRelevantSkills()
                .ConsiderCarryingCapacity()
                .ConsiderBeautyExpectations()
                .ConsiderIsAnyoneElseDoing()
                .ConsiderBestAtDoing()
                .ConsiderPassion()
                .ConsiderThoughts()
                .ConsiderInspiration()
                .ConsiderLowFood(-0.3f)
                .ConsiderColonistLeftUnburied()
                .ConsiderMovementSpeed()
                .ConsiderHealth()
                .ConsiderAteRawFood()
                .ConsiderBored()
                .ConsiderFire()
                .ConsiderBuildingImmunity()
                .ConsiderCompletingTask()
                .ConsiderColonistsNeedingTreatment()
                .ConsiderDownedColonists()
                .ConsiderColonyPolicy();
        }
    }
}
