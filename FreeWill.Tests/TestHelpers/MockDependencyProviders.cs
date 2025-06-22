using System.Collections.Generic;
using RimWorld;
using Verse;

namespace FreeWill.Tests.TestHelpers
{
    /// <summary>
    /// Mock implementation of IWorldStateProvider for testing.
    /// Provides controlled test values for world-level game state.
    /// </summary>
    public class MockWorldStateProvider : IWorldStateProvider
    {
        public FreeWill_ModSettings Settings { get; set; }

        public MockWorldStateProvider()
        {
            // Create default mock settings
            Settings = new FreeWill_ModSettings();
        }
    }    /// <summary>
         /// Mock implementation of IMapStateProvider for testing.
         /// Provides controlled test values for map-level game state.
         /// </summary>
    public class MockMapStateProvider : IMapStateProvider
    {
        public List<WorkTypeDef> DisabledWorkTypes { get; set; } = new List<WorkTypeDef>();
        public List<Thought> AllThoughts { get; set; } = new List<Thought>();
        public bool AlertNeedWarmClothes { get; set; } = false;
        public bool AlertAnimalRoaming { get; set; } = false;
        public float SuppressionNeed { get; set; } = 0f;
        public bool AlertColonistLeftUnburied { get; set; } = false;
        public bool AlertBoredom { get; set; } = false;
        public bool AlertLowFood { get; set; } = false;
        public bool AlertColonistsIdle { get; set; } = false;
        public bool AlertNeedJoy { get; set; } = false;
        public bool AlertFireInHomeArea { get; set; } = false;
        public bool AlertFilthInHomeArea { get; set; } = false;
        public bool AlertNeedWarmers { get; set; } = false;
        public bool AlertUnforbidStartingEquipment { get; set; } = false;
        public bool AlertUnforbidStartingResources { get; set; } = false;
        public int HuntersWithoutWeapons { get; set; } = 0;
        public int ColonyPopulation { get; set; } = 1;
        public int TasksAssignedToOthers { get; set; } = 0;
        public bool AlertMechGestatorCompleted { get; set; } = false;
        public int DownedColonistCount { get; set; } = 0;
        public int InjuredColonistCount { get; set; } = 0;
        public int InjuredPetCount { get; set; } = 0;
        public int InjuredPrisonerCount { get; set; } = 0;
        public bool AlertThingsDeteriorating { get; set; } = false;
        public bool AlertBlight { get; set; } = false;
        public bool AlertRefueling { get; set; } = false;
        public bool DoingWorkCurrently { get; set; } = false;
        public List<Pawn> AllPawns { get; set; } = new List<Pawn>();
        public bool AlertGauranlenPruning { get; set; } = false;

        // Additional required properties from the interface
        public float PercentPawnsDowned { get; set; } = 0f;
        public bool RefuelNeededNow { get; set; } = false;
        public bool RefuelNeeded { get; set; } = false;
        public bool HomeFire { get; set; } = false;
        public int MapFires { get; set; } = 0;
        public float PercentPawnsNeedingTreatment { get; set; } = 0f;
        public float NumPetsNeedingTreatment { get; set; } = 0f;
        public float NumPrisonersNeedingTreatment { get; set; } = 0f;
        public int NumPawns { get; set; } = 1;
        public float PercentPawnsMechHaulers { get; set; } = 0f;
        public bool AlertAnimalPenNeeded { get; set; } = false;
        public bool AlertAnimalPenNotEnclosed { get; set; } = false;
        public bool AlertMechDamaged { get; set; } = false;
        public Thing ThingsDeteriorating { get; set; } = null;
        public bool PlantsBlighted { get; set; } = false;
        public bool AreaHasHaulables { get; set; } = false;
        public bool AreaHasFilth { get; set; } = false;

        public int? GetLastBored(Pawn pawn)
        {
            // For testing, return null (never bored)
            return null;
        }

        public void UpdateLastBored(Pawn pawn)
        {
            // For testing, do nothing
        }
    }

    /// <summary>
    /// Mock implementation of IWorkTypeStrategyProvider for testing.
    /// </summary>
    public class MockWorkTypeStrategyProvider : IWorkTypeStrategyProvider
    {
        public IWorkTypeStrategy GetStrategy(WorkTypeDef workType)
        {
            // Return a basic default strategy for testing
            return new DefaultWorkTypeStrategy();
        }
    }    /// <summary>
         /// Mock implementation of IPriorityDependencyProvider for testing.
         /// Combines all mock providers into a single dependency container.
         /// </summary>
    public class MockPriorityDependencyProvider : IPriorityDependencyProvider
    {
        public IWorldStateProvider WorldStateProvider { get; set; }
        public IMapStateProvider MapStateProvider { get; set; }
        public IWorkTypeStrategyProvider StrategyProvider { get; set; }

        public MockPriorityDependencyProvider()
        {
            WorldStateProvider = new MockWorldStateProvider();
            MapStateProvider = new MockMapStateProvider();
            StrategyProvider = new MockWorkTypeStrategyProvider();
        }

        public MockPriorityDependencyProvider(
            IWorldStateProvider worldStateProvider = null,
            IMapStateProvider mapStateProvider = null,
            IWorkTypeStrategyProvider workTypeStrategyProvider = null)
        {
            WorldStateProvider = worldStateProvider ?? new MockWorldStateProvider();
            MapStateProvider = mapStateProvider ?? new MockMapStateProvider();
            StrategyProvider = workTypeStrategyProvider ?? new MockWorkTypeStrategyProvider();
        }

        public IMapStateProvider GetMapStateProvider(Pawn pawn)
        {
            // For testing, we return the same mock regardless of pawn
            return MapStateProvider;
        }
    }
}
