using RimWorld;
using Verse;
using FreeWill;

public class ThoughtWorker_Precept_FreeWillStreak : ThoughtWorker_Precept
{
    FreeWill_WorldComponent worldComp = null;
    protected override ThoughtState ShouldHaveThought(Pawn pawn)
    {
        if (pawn == null)
        {
            return ThoughtState.Inactive;
        }
        if (worldComp == null)
        {
            worldComp = Find.World.GetComponent<FreeWill_WorldComponent>();
        }
        if (!pawn.IsColonistPlayerControlled)
        {
            return ThoughtState.Inactive;
        }
        if (pawn.IsSlaveOfColony)
        {
            return ThoughtState.Inactive;
        }
        if (!worldComp.HasFreeWill(pawn))
        {
            worldComp.FreeWillOverride(pawn);
            return ThoughtState.Inactive;
        }
        var queue = pawn.jobs.jobQueue;
        for (int i = 0; i < queue.Count; i++)
        {
            var job = queue[i].job;
            if (!job.playerForced)
            {
                continue;
            }
            if (job.def == RimWorld.JobDefOf.TradeWithPawn)
            {
                // trading with pawns is okay
                continue;
            }
            worldComp.FreeWillOverride(pawn);
            return ThoughtState.Inactive;
        }
        var ticks = worldComp.FreeWillTicks(pawn);
        if (ticks > 900000)
        {
            return ThoughtState.ActiveAtStage(3);
        }
        if (ticks > 300000)
        {
            return ThoughtState.ActiveAtStage(2);
        }
        if (ticks > 60000)
        {
            return ThoughtState.ActiveAtStage(1);
        }
        if (ticks > 2500)
        {
            return ThoughtState.ActiveAtStage(0);
        }
        return ThoughtState.Inactive;
    }
}
