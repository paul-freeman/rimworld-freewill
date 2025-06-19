using System.Collections.Generic;
using System.Linq;
using Verse;

namespace FreeWill
{
    /// <summary>
    /// Registry for managing work type priority calculation strategies.
    /// Provides centralized access to all strategy implementations.
    /// </summary>
    public static class WorkTypeStrategyRegistry
    {
        private static readonly Dictionary<string, IWorkTypeStrategy> strategies = new Dictionary<string, IWorkTypeStrategy>();
        private static bool initialized = false;

        /// <summary>
        /// Initializes the registry with all available strategies.
        /// This is called automatically when first accessed.
        /// </summary>
        private static void Initialize()
        {
            if (initialized)
            {
                return;
            }

            // Register all strategy implementations
            RegisterStrategy(new FirefighterStrategy());
            RegisterStrategy(new PatientStrategy());
            RegisterStrategy(new DoctorStrategy());
            RegisterStrategy(new PatientBedRestStrategy());
            RegisterStrategy(new ChildcareStrategy());
            RegisterStrategy(new BasicWorkerStrategy());
            RegisterStrategy(new WardenStrategy());
            RegisterStrategy(new HandlingStrategy());
            RegisterStrategy(new CookingStrategy());
            RegisterStrategy(new HuntingStrategy());
            RegisterStrategy(new ConstructionStrategy());
            RegisterStrategy(new GrowingStrategy());
            RegisterStrategy(new MiningStrategy());
            RegisterStrategy(new PlantCuttingStrategy());
            RegisterStrategy(new SmithingStrategy());
            RegisterStrategy(new TailoringStrategy());
            RegisterStrategy(new ArtStrategy());
            RegisterStrategy(new CraftingStrategy());
            RegisterStrategy(new HaulingStrategy());
            RegisterStrategy(new CleaningStrategy());
            RegisterStrategy(new ResearchingStrategy());
            RegisterStrategy(new HaulingUrgentStrategy());
            RegisterStrategy(new DefaultWorkTypeStrategy());

            initialized = true;
        }

        /// <summary>
        /// Registers a strategy for a specific work type.
        /// </summary>
        /// <param name="strategy">The strategy to register.</param>
        private static void RegisterStrategy(IWorkTypeStrategy strategy)
        {
            strategies[strategy.WorkType.defName] = strategy;
        }

        /// <summary>
        /// Gets the strategy for the specified work type.
        /// If no specific strategy exists, returns the default strategy.
        /// </summary>
        /// <param name="workTypeDef">The work type to get a strategy for.</param>
        /// <returns>The appropriate strategy instance.</returns>
        public static IWorkTypeStrategy GetStrategy(WorkTypeDef workTypeDef)
        {
            Initialize();

            if (strategies.TryGetValue(workTypeDef.defName, out IWorkTypeStrategy strategy))
            {
                return strategy;
            }

            // Fallback to default strategy
            return strategies.Values.OfType<DefaultWorkTypeStrategy>().First();
        }

        /// <summary>
        /// Gets all registered strategies.
        /// </summary>
        /// <returns>Collection of all registered strategies.</returns>
        public static IEnumerable<IWorkTypeStrategy> GetAllStrategies()
        {
            Initialize();
            return strategies.Values;
        }
    }
}
