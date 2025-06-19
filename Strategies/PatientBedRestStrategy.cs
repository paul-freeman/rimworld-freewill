using RimWorld;
using Verse;

namespace FreeWill
{
    /// <summary>
    /// Strategy for patient bed rest work type priority calculation.
    /// Bedrest is used when pawns need to rest for recovery.
    /// </summary>
    public class PatientBedRestStrategy : BaseWorkTypeStrategy
    {
        public override WorkTypeDef WorkType => GetWorkTypeDef("PatientBedRest");

        public override Priority CalculatePriority(Priority priority)
        {
            priority.Set(0.0f, "FreeWillPriorityBedrestDefault".TranslateSimple);
            priority.AlwaysDo("FreeWillPriorityBedrestAlways".TranslateSimple)
                .ConsiderHealth()
                .ConsiderBuildingImmunity()
                .ConsiderLowFood(-0.2f)
                .ConsiderCompletingTask()
                .ConsiderBored()
                .ConsiderHavingFoodPoisoning();
            priority.ConsiderColonistsNeedingTreatment()
                .ConsiderDownedColonists()
                .ConsiderOperation()
                .ConsiderColonyPolicy();
            return priority;
        }
    }
}
