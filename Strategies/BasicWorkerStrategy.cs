using Verse;

namespace FreeWill
{
    /// <summary>
    /// Strategy for basic worker work type priority calculation.
    /// Basic worker covers essential colony maintenance tasks.
    /// </summary>
    public class BasicWorkerStrategy : BaseWorkTypeStrategy
    {
        public override WorkTypeDef WorkType => GetWorkTypeDef("BasicWorker");

        public override Priority CalculatePriority(Priority priority)
        {
            priority.Set(0.5f, "FreeWillPriorityBasicWorkDefault".TranslateSimple);
            return priority
                .ConsiderThoughts()
                .ConsiderHealth()
                .ConsiderLowFood(-0.3f)
                .ConsiderBored()
                .NeverDoIf(priority.pawn.Downed, "FreeWillPriorityPawnDowned".TranslateSimple)
                .ConsiderBuildingImmunity()
                .ConsiderCompletingTask()
                .ConsiderColonistsNeedingTreatment()
                .ConsiderDownedColonists()
                .ConsiderColonyPolicy();
        }
    }
}
