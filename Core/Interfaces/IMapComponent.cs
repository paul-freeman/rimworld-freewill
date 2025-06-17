using System.Collections.Generic;
using RimWorld;
using Verse;

namespace FreeWill.Core.Interfaces
{
    // Placeholder for FreeWill_MapComponent
    public interface IMapComponent
    {
        int NumPawns { get; }
        float PercentPawnsNeedingTreatment { get; }
        int NumPetsNeedingTreatment { get; }
        int NumPrisonersNeedingTreatment { get; }
        float PercentPawnsDowned { get; }
        float PercentPawnsMechHaulers { get; }
        float SuppressionNeed { get; }
        Thing ThingsDeteriorating { get; }
        int MapFires { get; }
        bool HomeFire { get; }
        bool RefuelNeededNow { get; }
        bool RefuelNeeded { get; }
        bool PlantsBlighted { get; }
        bool AlertNeedWarmClothes { get; }
        bool AlertColonistLeftUnburied { get; }
        bool AlertAnimalRoaming { get; }
        bool AlertLowFood { get; }
        bool AlertMechDamaged { get; }
        bool AlertAnimalPenNeeded { get; }
        bool AlertAnimalPenNotEnclosed { get; }
        bool AreaHasHaulables { get; }
        bool AreaHasFilth { get; }
        IEnumerable<Thought> AllThoughts { get; }
        IEnumerable<IWorkTypeDef> DisabledWorkTypes { get; }
        void UpdateLastBored(IPawn pawn);
        int GetLastBored(IPawn pawn);
    }
}
