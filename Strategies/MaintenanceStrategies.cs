using Verse;

namespace FreeWill
{
    /// <summary>
    /// Strategy for hauling work type priority calculation.
    /// </summary>
    public class HaulingStrategy : BaseWorkTypeStrategy
    {
        public override WorkTypeDef WorkType => GetWorkTypeDef("Hauling");

        public override Priority CalculatePriority(Priority priority)
        {
            priority.Set(0.3f, "FreeWillPriorityHaulingDefault".TranslateSimple);
            return priority
                .ConsiderBeautyExpectations()
                .ConsiderCarryingCapacity()
                .ConsiderIsAnyoneElseDoing()
                .ConsiderPassion()
                .ConsiderThoughts()
                .ConsiderInspiration()
                .ConsiderRefueling()
                .ConsiderLowFood(0.2f)
                .ConsiderColonistLeftUnburied()
                .ConsiderMovementSpeed()
                .ConsiderHealth()
                .ConsiderAteRawFood()
                .ConsiderThingsDeteriorating()
                .ConsiderMechHaulers()
                .ConsiderBored()
                .ConsiderFire()
                .ConsiderBuildingImmunity()
                .ConsiderCompletingTask()
                .ConsiderColonistsNeedingTreatment()
                .ConsiderDownedColonists()
                .ConsiderColonyPolicy();
        }
    }

    /// <summary>
    /// Strategy for cleaning work type priority calculation.
    /// </summary>
    public class CleaningStrategy : BaseWorkTypeStrategy
    {
        public override WorkTypeDef WorkType => GetWorkTypeDef("Cleaning");

        public override Priority CalculatePriority(Priority priority)
        {
            priority.Set(0.5f, "FreeWillPriorityCleaningDefault".TranslateSimple);
            return priority
                .ConsiderBeautyExpectations()
                .ConsiderIsAnyoneElseDoing()
                .ConsiderThoughts()
                .ConsiderOwnRoom()
                .ConsiderLowFood(-0.2f)
                .ConsiderFoodPoisoningRisk()
                .ConsiderHealth()
                .ConsiderBored()
                .NeverDoIf(priority.NotInHomeArea(priority.pawn), "FreeWillPriorityNotInHomeArea".TranslateSimple)
                .ConsiderBuildingImmunity()
                .ConsiderCompletingTask()
                .ConsiderColonistsNeedingTreatment()
                .ConsiderDownedColonists()
                .ConsiderColonyPolicy();
        }
    }

    /// <summary>
    /// Strategy for research work type priority calculation.
    /// </summary>
    public class ResearchingStrategy : BaseWorkTypeStrategy
    {
        public override WorkTypeDef WorkType => GetWorkTypeDef("Research");

        public override Priority CalculatePriority(Priority priority)
        {
            return priority
                .ConsiderRelevantSkills()
                .ConsiderIsAnyoneElseDoing()
                .ConsiderBestAtDoing()
                .ConsiderBeautyExpectations()
                .ConsiderPassion()
                .ConsiderThoughts()
                .ConsiderInspiration()
                .ConsiderLowFood(-0.4f)
                .ConsiderColonistLeftUnburied()
                .ConsiderHealth()
                .ConsiderAteRawFood()
                .ConsiderBored()
                .ConsiderFire()
                .ConsiderBuildingImmunity()
                .ConsiderColonistsNeedingTreatment()
                .ConsiderDownedColonists()
                .ConsiderColonyPolicy();
        }
    }

    /// <summary>
    /// Strategy for urgent hauling work type priority calculation.
    /// </summary>
    public class HaulingUrgentStrategy : BaseWorkTypeStrategy
    {
        public override WorkTypeDef WorkType => GetWorkTypeDef("HaulingUrgent");

        public override Priority CalculatePriority(Priority priority)
        {
            priority.Set(0.5f, "FreeWillPriorityUrgentHaulingDefault".TranslateSimple);
            return priority
                .ConsiderBeautyExpectations()
                .ConsiderCarryingCapacity()
                .ConsiderIsAnyoneElseDoing()
                .ConsiderPassion()
                .ConsiderThoughts()
                .ConsiderInspiration()
                .ConsiderRefueling()
                .ConsiderLowFood(0.3f)
                .ConsiderColonistLeftUnburied()
                .ConsiderMovementSpeed()
                .ConsiderHealth()
                .ConsiderAteRawFood()
                .ConsiderThingsDeteriorating()
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
