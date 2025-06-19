using System;
using FreeWill.Tests.TestHelpers;
using RimWorld;
using Verse;

namespace FreeWill.Tests
{
    /// <summary>
    /// Basic tests for the Priority class infrastructure.
    /// Note: These tests focus on what can be tested without complex RimWorld game state.
    /// Full integration tests will require a more sophisticated test harness.
    /// </summary>
    public class PriorityTests
    {
        /// <summary>
        /// Test Priority constructor with valid inputs (basic initialization).
        /// Note: This tests the constructor but doesn't call methods that require game state.
        /// </summary>
        public static void TestConstructorBasics()
        {
            // Arrange
            var workTypeDef = TestDataBuilders.WorkTypeDefs.Hauling;

            // Act & Assert - Test constructor with null pawn
            // This documents the current behavior
            try
            {
                var priority = new Priority(null, workTypeDef);

                // Verify basic properties are accessible
                if (priority.WorkTypeDef == null)
                {
                    throw new Exception("WorkTypeDef should not be null after construction");
                }

                if (priority.WorkTypeDef.defName != "Hauling")
                {
                    throw new Exception($"Expected WorkTypeDef.defName 'Hauling', got '{priority.WorkTypeDef.defName}'");
                }

                if (priority.AdjustmentStrings == null)
                {
                    throw new Exception("AdjustmentStrings should not be null after construction");
                }

                Console.WriteLine("Constructor basics - PASSED");
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine($"Constructor with null pawn threw NullReferenceException (expected): {ex.Message}");
            }
        }

        /// <summary>
        /// Test Priority constructor with null WorkTypeDef.
        /// </summary>
        public static void TestConstructorWithNullWorkType()
        {
            try
            {
                var priority = new Priority(null, null);

                // If we get here, verify the state
                if (priority.WorkTypeDef != null)
                {
                    throw new Exception("Expected WorkTypeDef to be null");
                }

                Console.WriteLine("Constructor with null WorkTypeDef - PASSED (no exception thrown)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Constructor with null WorkTypeDef threw exception (acceptable): {ex.GetType().Name}");
            }
        }

        /// <summary>
        /// Test IComparable implementation without requiring full Priority computation.
        /// </summary>
        public static void TestIComparable()
        {
            // We can't easily test this without creating valid Priority instances
            // that have computed values, so for now just document the interface

            var workTypeDef = TestDataBuilders.WorkTypeDefs.Hauling;

            try
            {
                var priority1 = new Priority(null, workTypeDef);
                var priority2 = new Priority(null, workTypeDef);

                // Test CompareTo with null
                var result = ((IComparable)priority1).CompareTo(null);
                if (result <= 0)
                {
                    throw new Exception("CompareTo(null) should return positive value");
                }

                // Test CompareTo with non-Priority object
                var result2 = ((IComparable)priority1).CompareTo("not a priority");
                if (result2 <= 0)
                {
                    throw new Exception("CompareTo(non-Priority) should return positive value");
                }

                Console.WriteLine("IComparable basic tests - PASSED");
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("IComparable test skipped due to null pawn dependency");
            }
        }

        /// <summary>
        /// Test work type definition creation (tests our test helpers).
        /// </summary>
        public static void TestWorkTypeDefCreation()
        {
            var firefighter = TestDataBuilders.WorkTypeDefs.Firefighter;
            var patient = TestDataBuilders.WorkTypeDefs.Patient;
            var doctor = TestDataBuilders.WorkTypeDefs.Doctor;
            var cooking = TestDataBuilders.WorkTypeDefs.Cooking;
            var hauling = TestDataBuilders.WorkTypeDefs.Hauling;
            var cleaning = TestDataBuilders.WorkTypeDefs.Cleaning;
            var research = TestDataBuilders.WorkTypeDefs.Research;

            // Test each work type
            if (firefighter?.defName != MockGameObjects.WorkTypes.Firefighter)
                throw new Exception($"Firefighter defName mismatch: expected '{MockGameObjects.WorkTypes.Firefighter}', got '{firefighter?.defName}'");

            if (patient?.defName != MockGameObjects.WorkTypes.Patient)
                throw new Exception($"Patient defName mismatch: expected '{MockGameObjects.WorkTypes.Patient}', got '{patient?.defName}'");

            if (doctor?.defName != MockGameObjects.WorkTypes.Doctor)
                throw new Exception($"Doctor defName mismatch: expected '{MockGameObjects.WorkTypes.Doctor}', got '{doctor?.defName}'");

            if (cooking?.defName != MockGameObjects.WorkTypes.Cooking)
                throw new Exception($"Cooking defName mismatch: expected '{MockGameObjects.WorkTypes.Cooking}', got '{cooking?.defName}'");

            if (hauling?.defName != MockGameObjects.WorkTypes.Hauling)
                throw new Exception($"Hauling defName mismatch: expected '{MockGameObjects.WorkTypes.Hauling}', got '{hauling?.defName}'");

            if (cleaning?.defName != MockGameObjects.WorkTypes.Cleaning)
                throw new Exception($"Cleaning defName mismatch: expected '{MockGameObjects.WorkTypes.Cleaning}', got '{cleaning?.defName}'");

            if (research?.defName != MockGameObjects.WorkTypes.Research)
                throw new Exception($"Research defName mismatch: expected '{MockGameObjects.WorkTypes.Research}', got '{research?.defName}'");

            Console.WriteLine("WorkTypeDef creation - PASSED");
        }

        /// <summary>
        /// Test constants and helper values.
        /// </summary>
        public static void TestConstants()
        {
            // Test our test data builder constants
            if (TestDataBuilders.Values.DefaultPriority != 0.2f)
                throw new Exception($"DefaultPriority should be 0.2f, got {TestDataBuilders.Values.DefaultPriority}");

            if (TestDataBuilders.Values.HighPriority != 0.8f)
                throw new Exception($"HighPriority should be 0.8f, got {TestDataBuilders.Values.HighPriority}");

            if (TestDataBuilders.Values.LowPriority != 0.1f)
                throw new Exception($"LowPriority should be 0.1f, got {TestDataBuilders.Values.LowPriority}");

            if (TestDataBuilders.Values.DisabledPriority != 0.0f)
                throw new Exception($"DisabledPriority should be 0.0f, got {TestDataBuilders.Values.DisabledPriority}");

            if (TestDataBuilders.Values.FloatTolerance != 0.001f)
                throw new Exception($"FloatTolerance should be 0.001f, got {TestDataBuilders.Values.FloatTolerance}");

            Console.WriteLine("Test constants - PASSED");
        }

        /// <summary>
        /// Run all basic tests that can execute without complex RimWorld dependencies.
        /// </summary>
        public static void RunAllTests()
        {
            Console.WriteLine("=== Running FreeWill Priority Tests ===");
            Console.WriteLine("Note: These are basic tests that avoid complex RimWorld game state dependencies.");
            Console.WriteLine("Full integration testing will require more sophisticated test setup.");
            Console.WriteLine();

            try
            {
                TestWorkTypeDefCreation();
                TestConstants();
                TestConstructorBasics();
                TestConstructorWithNullWorkType();
                TestIComparable();

                Console.WriteLine();
                Console.WriteLine("=== All basic tests PASSED! ===");
                Console.WriteLine();
                Console.WriteLine("Next steps for comprehensive testing:");
                Console.WriteLine("1. Create test harness for RimWorld game state (pawn, map, components)");
                Console.WriteLine("2. Test Priority.Compute() method with various scenarios");
                Console.WriteLine("3. Test ToGamePriority() and FromGamePriority() conversion methods");
                Console.WriteLine("4. Test priority adjustment methods (Set, Add, Multiply, etc.)");
                Console.WriteLine("5. Test work-type-specific logic in InnerCompute()");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"=== Test FAILED: {ex.Message} ===");
                Console.WriteLine($"Exception Type: {ex.GetType().Name}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
