using RimWorld;
using Verse;
using FreeWill;

public class ThoughtWorker_Precept_EnforcedWorkSchedule : ThoughtWorker_Precept
{
    protected override ThoughtState ShouldHaveThought(Pawn pawn)
    {
        var worldComp = Find.World.GetComponent<FreeWill_WorldComponent>();
        // free pawns should not have this thought
        return !worldComp.HasFreeWill(pawn);
    }
}