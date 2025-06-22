using Verse;

namespace FreeWill
{
    /// <summary>
    /// Strategy for animal handling work type priority calculation.
    /// Animal handling includes training, taming, and managing animal behavior.
    /// Priority increases when animals are roaming and need management.
    /// Movement speed is important for catching and handling animals effectively.
    /// </summary>
    public class HandlingStrategy : BaseWorkTypeStrategy
    {
        public override WorkTypeDef WorkType => GetWorkTypeDef("Handling");

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
                .ConsiderAnimalsRoaming() // Higher priority when animals need managing
                .ConsiderColonistLeftUnburied()
                .ConsiderMovementSpeed() // Important for catching animals
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