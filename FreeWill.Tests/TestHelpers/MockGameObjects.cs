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
            public const string Doctor = "Doctor";
            public const string Cooking = "Cooking";
            public const string Hauling = "Hauling";
            public const string Cleaning = "Cleaning";
            public const string Research = "Research";
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
