using Verse;

namespace FreeWill.Tests.TestHelpers
{
    /// <summary>
    /// Helper class for creating test objects for testing.
    /// Note: Due to RimWorld's sealed classes, we'll create minimal test implementations
    /// and may require integration test approaches for complex scenarios.
    /// </summary>
    public static class MockGameObjects
    {
        /// <summary>
        /// Creates commonly used work type definitions for testing.
        /// Note: These return the actual defNames as strings for switch statements.
        /// </summary>
        public static class WorkTypes
        {
            public const string Firefighter = "Firefighter";
            public const string Patient = "Patient";
            public const string PatientBedRest = "PatientBedRest";
            public const string Doctor = "Doctor";
            public const string Cooking = "Cooking";
            public const string Hunting = "Hunting";
            public const string Construction = "Construction";
            public const string Growing = "Growing";
            public const string Hauling = "Hauling";
            public const string Cleaning = "Cleaning";
            public const string Research = "Research";
            public const string Mining = "Mining";
            public const string PlantCutting = "PlantCutting";
            public const string Smithing = "Smithing";
            public const string Tailoring = "Tailoring";
            public const string Art = "Art";
            public const string Crafting = "Crafting";
            public const string HaulingUrgent = "HaulingUrgent";
            public const string Childcare = "Childcare";
            public const string Warden = "Warden";
            public const string Handling = "Handling";
            public const string BasicWorker = "BasicWorker";
        }

        /// <summary>
        /// Creates a basic WorkTypeDef for testing purposes.
        /// Note: This creates a minimal instance with the defName set.
        /// </summary>
        public static WorkTypeDef CreateTestWorkTypeDef(string defName)
        {
            WorkTypeDef workTypeDef = new WorkTypeDef
            {
                defName = defName,
                label = defName.ToLower(),
                description = $"Test work type: {defName}"
            };
            return workTypeDef;
        }
    }
}
