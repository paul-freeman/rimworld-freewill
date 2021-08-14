using RimWorld;
using Verse;
using FreeWill;

public class ThoughtWorker_Precept_NoWorkSchedule : ThoughtWorker_Precept
{
    private PreceptDef freeWillProhibited;
    private PreceptDef freeWillDisapproved;
    private PreceptDef freeWillPreferred;
    private PreceptDef freeWillMandatory;

    public ThoughtWorker_Precept_NoWorkSchedule()
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
            if (pawn.Ideo.HasPrecept(freeWillMandatory) || pawn.Ideo.HasPrecept(freeWillPreferred))
            {
                mapComp.pawnFree[pawnKey] = true;
                return false;
            }
            else
            {
                mapComp.pawnFree[pawnKey] = false;
                return false;
            }
        }
        // free pawns should have this thought
        return mapComp.pawnFree[pawnKey];
    }
}
