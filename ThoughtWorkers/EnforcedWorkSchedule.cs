using RimWorld;
using Verse;
using FreeWill;

public class ThoughtWorker_Precept_EnforcedWorkSchedule : ThoughtWorker_Precept
{
    private PreceptDef freeWillProhibited;
    private PreceptDef freeWillDisapproved;
    private PreceptDef freeWillPreferred;
    private PreceptDef freeWillMandatory;

    public ThoughtWorker_Precept_EnforcedWorkSchedule()
    {
        freeWillProhibited = DefDatabase<PreceptDef>.GetNamed("Free_Will_Prohibited");
        freeWillDisapproved = DefDatabase<PreceptDef>.GetNamed("Free_Will_Disapproved");
        freeWillPreferred = DefDatabase<PreceptDef>.GetNamed("Free_Will_Preferred");
        freeWillMandatory = DefDatabase<PreceptDef>.GetNamed("Free_Will_Mandatory");
    }

    protected override ThoughtState ShouldHaveThought(Pawn pawn)
    {
        var mapComp = pawn.Map.GetComponent<FreeWill_MapComponent>();
        var pawnKey = pawn.GetUniqueLoadID();
        if (!mapComp.pawnFree.ContainsKey(pawnKey))
        {
            if (pawn.Ideo.HasPrecept(freeWillProhibited) || pawn.Ideo.HasPrecept(freeWillDisapproved))
            {
                mapComp.pawnFree[pawnKey] = false;
                return false;
            }
            else
            {
                mapComp.pawnFree[pawnKey] = true;
                return true;
            }
        }
        // free pawns should not have this thought
        return !mapComp.pawnFree[pawnKey];
    }
}