using Verse;

namespace FreeWill
{
    /// <summary>
    /// Strategy for crafting work type priority calculation.
    /// Crafting involves creating tools, weapons, furniture, and various manufactured items.
    /// Priority increases when colonists have high beauty expectations for crafted items.
    /// Priority decreases during food shortages as crafting is less critical than immediate survival.
    /// Considers relevant crafting skills and the ability to create quality items.
    /// </summary>
    public class CraftingStrategy : BaseWorkTypeStrategy
    {
        public override WorkTypeDef WorkType => GetWorkTypeDef("Crafting");

        public override Priority CalculatePriority(Priority priority)
        {
            return priority
                .ConsiderRelevantSkills()
                .ConsiderCarryingCapacity()
                .ConsiderIsAnyoneElseDoing()
                .ConsiderBestAtDoing()
                .ConsiderBeautyExpectations() // Quality crafted items improve colony beauty
                .ConsiderPassion()
                .ConsiderThoughts()
                .ConsiderInspiration()
                .ConsiderLowFood(-0.3f) // Crafting becomes less important when food is scarce
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