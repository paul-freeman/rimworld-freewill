using System;
using System.Collections.Generic;
using RimWorld;
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
            public static WorkTypeDef Doctor => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.Doctor);
            public static WorkTypeDef Cooking => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.Cooking);
            public static WorkTypeDef Hauling => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.Hauling);
            public static WorkTypeDef Cleaning => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.Cleaning);
            public static WorkTypeDef Research => MockGameObjects.CreateTestWorkTypeDef(MockGameObjects.WorkTypes.Research);
        }
    }
}
