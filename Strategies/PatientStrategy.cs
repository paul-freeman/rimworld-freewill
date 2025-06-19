using RimWorld;
using Verse;

namespace FreeWill
{
    /// <summary>
    /// Strategy for patient work type priority calculation.
    /// Patient work is always high priority when health is poor.
    /// </summary>
    public class PatientStrategy : BaseWorkTypeStrategy
    {
        public override WorkTypeDef WorkType => GetWorkTypeDef("Patient");

        public override Priority CalculatePriority(Priority priority)
        {
            priority.Set(0.0f, "FreeWillPriorityPatientDefault".TranslateSimple);
            return priority
                .AlwaysDo("FreeWillPriorityPatientAlways".TranslateSimple)
                .ConsiderHealth()
                .ConsiderBuildingImmunity()
                .ConsiderCompletingTask()
                .ConsiderColonistsNeedingTreatment()
                .ConsiderDownedColonists()
                .ConsiderOperation()
                .ConsiderColonyPolicy();
        }
    }
}
