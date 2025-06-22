using Verse;

namespace FreeWill
{
    /// <summary>
    /// Strategy for childcare work type priority calculation.
    /// Childcare involves taking care of children, including feeding, playing, and general supervision.
    /// Priority increases when the pawn has relevant skills and passion for social work.
    /// </summary>
    public class ChildcareStrategy : BaseWorkTypeStrategy
    {
        public override WorkTypeDef WorkType => GetWorkTypeDef("Childcare");

        public override Priority CalculatePriority(Priority priority)
        {
            // Base priority for childcare work - moderate importance
            priority.Set(0.5f, "FreeWillPriorityChildcareDefault".TranslateSimple);

            return priority
                .ConsiderRelevantSkills(shouldAdd: true)
                .ConsiderPassion()
                .ConsiderThoughts()
                .ConsiderInspiration()
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