using RimWorld;
using Verse;
using FreeWill;

public class ThoughtWorker_Precept_EnforcedWorkSchedule : ThoughtWorker_Precept
{
    protected override ThoughtState ShouldHaveThought(Pawn pawn)
    {
        if (pawn.IsColonistPlayerControlled || pawn.IsSlaveOfColony)
        {
            FreeWill_WorldComponent worldComp = Find.World.GetComponent<FreeWill_WorldComponent>();
            // free pawns should not have this thought
            return worldComp.HasFreeWill(pawn, pawn.GetUniqueLoadID()) ? ThoughtState.Inactive : ThoughtState.ActiveDefault;
        }
        return ThoughtState.Inactive;
    }
}