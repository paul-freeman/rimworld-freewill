using Verse;
using RimWorld;

namespace FreeWill.Core.Interfaces
{
    // Placeholder for RimWorld.Planet.WorldComponent
    public interface IWorldComponent
    {
        FreeWill_ModSettings Settings { get; }

        bool HasFreeWill(IPawn pawn, string pawnKey);
        bool FreeWillCanChange(IPawn pawn, string pawnKey);
        bool TryGiveFreeWill(IPawn pawn);
        bool TryRemoveFreeWill(IPawn pawn);
        void EnsureFreeWillStatusIsCorrect(IPawn pawn, string pawnKey);
        void FreeWillOverride(IPawn pawn);
        int FreeWillTicks(IPawn pawn);
    }
}
