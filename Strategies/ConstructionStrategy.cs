using Verse;

namespace FreeWill
{
    /// <summary>
    /// Strategy for construction work type priority calculation.
    /// Construction involves building structures, walls, furniture, and infrastructure.
    /// Priority decreases during food shortages as construction is less critical than survival needs.
    /// Considers animal pen requirements and overall colony development needs.
    /// </summary>
    public class ConstructionStrategy : BaseWorkTypeStrategy
    {
        public override WorkTypeDef WorkType => GetWorkTypeDef("Construction");

        public override Priority CalculatePriority(Priority priority)
        {
            priority.ConsiderRelevantSkills()
                .ConsiderCarryingCapacity()
                .ConsiderIsAnyoneElseDoing()
                .ConsiderBestAtDoing()
                .ConsiderPassion()
                .ConsiderThoughts()
                .ConsiderInspiration();

            priority.ConsiderAnimalPen(); // Animal pen construction needs priority

            return priority
                .ConsiderLowFood(-0.3f) // Construction becomes less important when food is scarce
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