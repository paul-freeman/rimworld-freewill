using RimWorld;
using Verse;
using FreeWill;

public class ThoughtWorker_Precept_FreeWillStreak : ThoughtWorker_Precept
{
    private FreeWill_WorldComponent worldComp = null;
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
        if (!worldComp.HasFreeWill(pawn, pawn.GetUniqueLoadID()))
        {
            worldComp.FreeWillOverride(pawn);
            return ThoughtState.Inactive;
        }
        Verse.AI.JobQueue queue = pawn.jobs.jobQueue;
        for (int i = 0; i < queue.Count; i++)
        {
            Verse.AI.Job job = queue[i].job;
            if (!job.playerForced)
            {
                continue;
            }
            if (job.def == JobDefOf.TradeWithPawn)
            {
                // trading with pawns is okay
                continue;
            }
            worldComp.FreeWillOverride(pawn);
            return ThoughtState.Inactive;
        }
        int ticks = worldComp.FreeWillTicks(pawn);
        return ticks > 3600000
            ? ThoughtState.ActiveAtStage(3)
            : ticks > 900000
            ? ThoughtState.ActiveAtStage(2)
            : ticks > 300000
            ? ThoughtState.ActiveAtStage(1)
            : ticks > 60000
            ? ThoughtState.ActiveAtStage(0)
            : ThoughtState.Inactive;
    }
}
