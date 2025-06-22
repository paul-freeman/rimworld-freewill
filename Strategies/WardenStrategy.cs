using Verse;

namespace FreeWill
{
    /// <summary>
    /// Strategy for warden work type priority calculation.
    /// Wardens manage prisoners, including feeding, recruitment attempts, and prison maintenance.
    /// Priority decreases during food shortages as prisoner care becomes less critical.
    /// </summary>
    public class WardenStrategy : BaseWorkTypeStrategy
    {
        public override WorkTypeDef WorkType => GetWorkTypeDef("Warden");

        public override Priority CalculatePriority(Priority priority)
        {
            return priority
                .ConsiderRelevantSkills()
                .ConsiderCarryingCapacity()
                .ConsiderIsAnyoneElseDoing()
                .ConsiderBestAtDoing()
                .ConsiderPassion()
                .ConsiderThoughts()
                .ConsiderInspiration()
                .ConsiderLowFood(-0.3f) // Prisoner care becomes less important when colonists are hungry
                .ConsiderSuppressionNeed()
                .ConsiderColonistLeftUnburied()
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