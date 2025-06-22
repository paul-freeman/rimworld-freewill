using Verse;

namespace FreeWill.Tests.TestHelpers
{
    /// <summary>
    /// Builder classes for creating test data in a fluent, readable way.
    /// These help create complex test scenarios with better maintainability.
    /// Note: Due to RimWorld's sealed classes, this provides test constants and helpers.
    /// </summary>
    public static class TestDataBuilders
    {
        /// <summary>
        /// Helper methods for creating specific test values.
        /// </summary>
        public static class Values
        {
            public static readonly float DefaultPriority = 0.2f;
            public static readonly float HighPriority = 0.8f;
            public static readonly float LowPriority = 0.1f;
            public static readonly float DisabledPriority = 0.0f;

            /// <summary>
            /// Tolerance for floating point comparisons in tests.
            /// </summary>
            public static readonly float FloatTolerance = 0.001f;
        }

        /// <summary>
        /// Creates test WorkTypeDef instances for common scenarios.
        /// </summary>
        public static class WorkTypeDefs
        {
            public static WorkTypeDef Firefighter => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.Firefighter);
            public static WorkTypeDef Patient => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.Patient);
            public static WorkTypeDef PatientBedRest => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.PatientBedRest);
            public static WorkTypeDef Doctor => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.Doctor);
            public static WorkTypeDef Cooking => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.Cooking);
            public static WorkTypeDef Hunting => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.Hunting);
            public static WorkTypeDef Construction => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.Construction);
            public static WorkTypeDef Growing => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.Growing);
            public static WorkTypeDef Hauling => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.Hauling);
            public static WorkTypeDef Cleaning => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.Cleaning);
            public static WorkTypeDef Research => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.Research);
            public static WorkTypeDef Mining => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.Mining);
            public static WorkTypeDef PlantCutting => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.PlantCutting);
            public static WorkTypeDef Smithing => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.Smithing);
            public static WorkTypeDef Tailoring => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.Tailoring);
            public static WorkTypeDef Art => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.Art);
            public static WorkTypeDef Crafting => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.Crafting);
            public static WorkTypeDef HaulingUrgent => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.HaulingUrgent);
            public static WorkTypeDef Childcare => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.Childcare);
            public static WorkTypeDef Warden => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.Warden);
            public static WorkTypeDef Handling => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.Handling);
            public static WorkTypeDef BasicWorker => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.BasicWorker);
        }
    }
}
