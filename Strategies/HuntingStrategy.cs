using Verse;

namespace FreeWill
{
    /// <summary>
    /// Strategy for hunting work type priority calculation.
    /// Hunting involves tracking and killing wild animals for food and resources.
    /// Priority increases during food shortages as hunting provides essential meat.
    /// Requires appropriate weapons and considers combat abilities and movement speed.
    /// </summary>
    public class HuntingStrategy : BaseWorkTypeStrategy
    {
        public override WorkTypeDef WorkType => GetWorkTypeDef("Hunting");

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
                .ConsiderLowFood(0.3f) // Hunting becomes more important when food is scarce
                .ConsiderWeaponRange()
                .ConsiderColonistLeftUnburied()
                .ConsiderMovementSpeed() // Important for tracking animals
                .ConsiderHealth()
                .ConsiderAteRawFood()
                .ConsiderBored()
                .ConsiderHasHuntingWeapon() // Must have appropriate weapon
                .ConsiderBrawlersNotHunting() // Brawlers are less effective hunters
                .ConsiderFire()
                .ConsiderBuildingImmunity()
                .ConsiderCompletingTask()
                .ConsiderColonistsNeedingTreatment()
                .ConsiderDownedColonists()
                .ConsiderColonyPolicy();
        }
    }
}