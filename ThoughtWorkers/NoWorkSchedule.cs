using RimWorld;
using Verse;
using FreeWill;

public class ThoughtWorker_Precept_NoWorkSchedule : ThoughtWorker_Precept
{
    protected override ThoughtState ShouldHaveThought(Pawn pawn)
    {
        if (pawn.IsColonistPlayerControlled || pawn.IsSlaveOfColony)
        {
            var worldComp = Find.World.GetComponent<FreeWill_WorldComponent>();
            // free pawns should have this thought
            return worldComp.HasFreeWill(pawn);
        }
        return false;
    }
}
