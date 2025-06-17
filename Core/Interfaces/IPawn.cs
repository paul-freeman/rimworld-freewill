using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace FreeWill.Core.Interfaces
{
    // Placeholder for Verse.Pawn
    public interface IPawn
    {
        IMap Map { get; }
        IntVec3 Position { get; }

        string GetUniqueLoadID();
        string NameShortColored { get; }
        Name Name { get; }

        bool Downed { get; }
        bool Dead { get; }
        bool Awake();
        bool IsColonistPlayerControlled { get; }
        bool IsSlaveOfColony { get; }
        bool IsColonyMechPlayerControlled { get; }
        bool IsCharging();

        Pawn_WorkSettings workSettings { get; }
        PlayerSettings playerSettings { get; }
        Pawn_MindState mindState { get; }
        Pawn_StoryTracker story { get; }
        Pawn_HealthTracker health { get; }
        Pawn_NeedsTracker needs { get; }
        Pawn_SkillTracker skills { get; }
        Pawn_JobTracker jobs { get; }
        Pawn_CarryTracker carryTracker { get; }
        Pawn_EquipmentTracker equipment { get; }
        Pawn_ConnectionsTracker connections { get; }
        RaceProperties RaceProps { get; }

        Room GetRoom();
        bool WorkTypeIsDisabled(WorkTypeDef workType);
        bool CanReserve(Thing t, int stackCount = 1, int max = -1, ReservationLayerDef layer = null, bool ignoreOtherReservations = false);
        Building_Bed CurrentBed();
    }
}
