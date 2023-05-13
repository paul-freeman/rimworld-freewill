using RimWorld;
using Verse;
using FreeWill;

public class ThoughtWorker_Precept_EnforcedWorkSchedule : ThoughtWorker_Precept
{
    protected override ThoughtState ShouldHaveThought(Pawn pawn)
    {
        if (pawn.IsColonistPlayerControlled || pawn.IsSlaveOfColony)
        {
            var worldComp = Find.World.GetComponent<FreeWill_WorldComponent>();
            // free pawns should not have this thought
            if (worldComp.HasFreeWill(pawn, pawn.GetUniqueLoadID()))
            {
                return ThoughtState.Inactive;
            }
            return ThoughtState.ActiveDefault;
        }
        return ThoughtState.Inactive;
    }
}