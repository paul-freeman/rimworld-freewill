using Verse;

namespace FreeWill
{
    /// <summary>
    /// Strategy for tailoring work type priority calculation.
    /// Tailoring involves creating clothing, armor, and textile items for colonist needs.
    /// Priority increases when colonists need warm clothes for survival.
    /// Priority decreases during food shortages as clothing production is less critical than food.
    /// Considers beauty expectations and crafting skills.
    /// </summary>
    public class TailoringStrategy : BaseWorkTypeStrategy
    {
        public override WorkTypeDef WorkType => GetWorkTypeDef("Tailoring");

        public override Priority CalculatePriority(Priority priority)
        {
            return priority
                .ConsiderRelevantSkills()
                .ConsiderCarryingCapacity()
                .ConsiderIsAnyoneElseDoing()
                .ConsiderBestAtDoing()
                .ConsiderBeautyExpectations()
                .ConsiderPassion()
                .ConsiderThoughts()
                .ConsiderInspiration()
                .ConsiderLowFood(-0.3f) // Clothing production becomes less important when food is scarce
                .ConsiderNeedingWarmClothes() // Critical when colonists need protection from cold
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