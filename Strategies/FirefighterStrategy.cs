using RimWorld;
using Verse;

namespace FreeWill
{
    /// <summary>
    /// Strategy for firefighting work type priority calculation.
    /// Firefighting is always enabled and considers fire-related factors.
    /// </summary>
    public class FirefighterStrategy : BaseWorkTypeStrategy
    {
        public override WorkTypeDef WorkType => GetWorkTypeDef("Firefighter");

        public override Priority CalculatePriority(Priority priority)
        {
            priority.Set(0.0f, "FreeWillPriorityFirefightingDefault".TranslateSimple);
            return priority
                .AlwaysDo("FreeWillPriorityFirefightingAlways".TranslateSimple)
                .NeverDoIf(priority.pawn.Downed, "FreeWillPriorityPawnDowned".TranslateSimple)
                .ConsiderFire()
                .ConsiderBuildingImmunity()
                .ConsiderCompletingTask()
                .ConsiderColonistsNeedingTreatment()
                .ConsiderDownedColonists()
                .ConsiderColonyPolicy();
        }
    }
}
