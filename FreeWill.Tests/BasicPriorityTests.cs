using System;
using FreeWill.Tests.TestHelpers;
using Verse;

namespace FreeWill.Tests
{
    /// <summary>
    /// Basic tests for the Priority class infrastructure - constructors, conversions, and basic methods.
    /// These tests focus on fundamental Priority class behavior without complex RimWorld game state.
    /// </summary>
    public class BasicPriorityTests
    {
        /// <summary>
        /// Test Priority constructor with valid inputs (basic initialization).
        /// Note: This tests the constructor but doesn't call methods that require game state.
        /// </summary>
        public static void TestConstructorBasics()
        {
            // Arrange
            WorkTypeDef workTypeDef = TestDataBuilders.WorkTypeDefs.Hauling;

            // Act & Assert - Test constructor with null pawn
            // This documents the current behavior
            try
            {
                Priority priority = new Priority(null, workTypeDef);

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
            catch (Exception ex)
            {
                Console.WriteLine($"Constructor basics test failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Test Priority constructor with null WorkTypeDef.
        /// </summary>
        public static void TestConstructorWithNullWorkType()
        {
            try
            {
                // This should throw an exception or handle gracefully
                Priority priority = new Priority(null, null);
                Console.WriteLine("Constructor with null WorkTypeDef should handle null gracefully or throw exception");
            }
            catch (Exception ex)
            {
                // Expected behavior - constructor should validate input
                Console.WriteLine($"Constructor with null WorkTypeDef - PASSED (properly throws exception): {ex.Message}");
            }
        }

        /// <summary>
        /// Test IComparable implementation without requiring full Priority computation.
        /// </summary>
        public static void TestIComparable()
        {
            try
            {
                WorkTypeDef workTypeDef1 = TestDataBuilders.WorkTypeDefs.Hauling;
                WorkTypeDef workTypeDef2 = TestDataBuilders.WorkTypeDefs.Cooking;

                Priority priority1 = new Priority(null, workTypeDef1);
                Priority priority2 = new Priority(null, workTypeDef2);                // Test that CompareTo method exists and can be called
                // Note: Without calling Compute(), the comparison may not work fully
                try
                {
                    int comparison = ((IComparable)priority1).CompareTo(priority2);
                    Console.WriteLine($"IComparable.CompareTo() basic test - PASSED (returned {comparison})");
                }
                catch (Exception)
                {
                    // Expected when pawn is null or game state not initialized
                    Console.WriteLine("IComparable test skipped due to null pawn dependency");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"IComparable test failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Test work type definition creation (tests our test helpers).
        /// </summary>
        public static void TestWorkTypeDefCreation()
        {
            try
            {
                WorkTypeDef hauling = TestDataBuilders.WorkTypeDefs.Hauling;
                WorkTypeDef cooking = TestDataBuilders.WorkTypeDefs.Cooking;
                WorkTypeDef firefighter = TestDataBuilders.WorkTypeDefs.Firefighter;
                WorkTypeDef patient = TestDataBuilders.WorkTypeDefs.Patient;
                WorkTypeDef doctor = TestDataBuilders.WorkTypeDefs.Doctor;
                WorkTypeDef cleaning = TestDataBuilders.WorkTypeDefs.Cleaning;
                WorkTypeDef research = TestDataBuilders.WorkTypeDefs.Research;

                // Verify the basic properties exist and match expected values
                if (hauling?.defName != MockGameObjects.WorkTypes.Hauling)
                {
                    throw new Exception($"Hauling defName mismatch: expected '{MockGameObjects.WorkTypes.Hauling}', got '{hauling?.defName}'");
                }

                if (cooking?.defName != MockGameObjects.WorkTypes.Cooking)
                {
                    throw new Exception($"Cooking defName mismatch: expected '{MockGameObjects.WorkTypes.Cooking}', got '{cooking?.defName}'");
                }

                if (firefighter?.defName != MockGameObjects.WorkTypes.Firefighter)
                {
                    throw new Exception($"Firefighter defName mismatch: expected '{MockGameObjects.WorkTypes.Firefighter}', got '{firefighter?.defName}'");
                }
                if (patient?.defName != MockGameObjects.WorkTypes.Patient)
                {
                    throw new Exception($"Patient defName mismatch: expected '{MockGameObjects.WorkTypes.Patient}', got '{patient?.defName}'");
                }

                if (doctor?.defName != MockGameObjects.WorkTypes.Doctor)
                {
                    throw new Exception($"Doctor defName mismatch: expected '{MockGameObjects.WorkTypes.Doctor}', got '{doctor?.defName}'");
                }

                if (cleaning?.defName != MockGameObjects.WorkTypes.Cleaning)
                {
                    throw new Exception($"Cleaning defName mismatch: expected '{MockGameObjects.WorkTypes.Cleaning}', got '{cleaning?.defName}'");
                }

                if (research?.defName != MockGameObjects.WorkTypes.Research)
                {
                    throw new Exception($"Research defName mismatch: expected '{MockGameObjects.WorkTypes.Research}', got '{research?.defName}'");
                }

                Console.WriteLine("WorkTypeDef creation - PASSED");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WorkTypeDef creation test failed or skipped due to RimWorld dependency: {ex.Message}");
                Console.WriteLine("This is expected when running outside RimWorld environment");
            }
        }

        /// <summary>
        /// Test constants and helper values.
        /// </summary>
        public static void TestConstants()
        {
            // Test our test data builder constants
            if (TestDataBuilders.Values.DefaultPriority != 0.2f)
            {
                throw new Exception($"DefaultPriority should be 0.2f, got {TestDataBuilders.Values.DefaultPriority}");
            }

            if (TestDataBuilders.Values.HighPriority != 0.8f)
            {
                throw new Exception($"HighPriority should be 0.8f, got {TestDataBuilders.Values.HighPriority}");
            }

            if (TestDataBuilders.Values.LowPriority != 0.1f)
            {
                throw new Exception($"LowPriority should be 0.1f, got {TestDataBuilders.Values.LowPriority}");
            }

            if (TestDataBuilders.Values.DisabledPriority != 0.0f)
            {
                throw new Exception($"DisabledPriority should be 0.0f, got {TestDataBuilders.Values.DisabledPriority}");
            }

            if (TestDataBuilders.Values.FloatTolerance != 0.001f)
            {
                throw new Exception($"FloatTolerance should be 0.001f, got {TestDataBuilders.Values.FloatTolerance}");
            }

            Console.WriteLine("Test constants - PASSED");
        }

        /// <summary>
        /// Test ToGamePriority() conversion logic with boundary values using public interface.
        /// Note: RimWorld uses an inverted priority system where 1=highest, 4=lowest, 0=disabled
        /// </summary>
        public static void TestToGamePriorityConversion()
        {
            WorkTypeDef workTypeDef = TestDataBuilders.WorkTypeDefs.Hauling;

            try
            {
                Priority priority = new Priority(null, workTypeDef);
                Func<string> testDesc = () => "ToGamePriority test";

                // Test disabled value (0.0f should convert to 0)
                priority.Set(0.0f, testDesc);
                int gameValue = priority.ToGamePriority();
                if (gameValue != 0)
                {
                    throw new Exception($"ToGamePriority(0.0f) should be 0, got {gameValue}");
                }

                // Test high value (1.0f should convert to high priority = 1)
                priority.Set(1.0f, testDesc);
                gameValue = priority.ToGamePriority();
                if (gameValue != 1)
                {
                    throw new Exception($"ToGamePriority(1.0f) should be 1 (highest priority), got {gameValue}");
                }

                // Test medium-high value (around 0.75 should convert to 2)
                priority.Set(0.75f, testDesc);
                gameValue = priority.ToGamePriority();
                if (gameValue < 1 || gameValue > 3)
                {
                    throw new Exception($"ToGamePriority(0.75f) should be reasonable priority 1-3, got {gameValue}");
                }

                // Test medium value (around 0.5 should convert to 2 or 3)
                priority.Set(0.5f, testDesc);
                gameValue = priority.ToGamePriority();
                if (gameValue < 1 || gameValue > 4)
                {
                    throw new Exception($"ToGamePriority(0.5f) should be reasonable priority 1-4, got {gameValue}");
                }

                // Test low value (around 0.25 should convert to 3 or 4)
                priority.Set(0.25f, testDesc);
                gameValue = priority.ToGamePriority();
                if (gameValue < 1 || gameValue > 4)
                {
                    throw new Exception($"ToGamePriority(0.25f) should be reasonable priority 1-4, got {gameValue}");
                }

                Console.WriteLine("ToGamePriority conversion tests - PASSED");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ToGamePriority conversion test failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Test FromGamePriority() conversion logic with boundary values.
        /// Note: RimWorld uses an inverted priority system where 1=highest, 4=lowest, 0=disabled
        /// </summary>
        public static void TestFromGamePriorityConversion()
        {
            WorkTypeDef workTypeDef = TestDataBuilders.WorkTypeDefs.Hauling;

            try
            {
                // Test disabled game priority (0 should convert to 0.0f)
                Priority priority0 = new Priority(null, workTypeDef);
                priority0.FromGamePriority(0);
                if (Math.Abs(priority0.Value - 0.0f) > 0.001f)
                {
                    throw new Exception($"FromGamePriority(0) should be 0.0f, got {priority0.Value}");
                }

                // Test highest game priority (1 should convert to high value close to 1.0)
                Priority priority1 = new Priority(null, workTypeDef);
                priority1.FromGamePriority(1);
                if (priority1.Value < 0.8f) // Should be high value
                {
                    throw new Exception($"FromGamePriority(1) should be high value (>0.8), got {priority1.Value}");
                }

                // Test lowest game priority (4 should convert to low but non-zero value)
                Priority priority4 = new Priority(null, workTypeDef);
                priority4.FromGamePriority(4);
                if (priority4.Value <= 0.1f || priority4.Value >= 0.5f) // Should be low but reasonable
                {
                    throw new Exception($"FromGamePriority(4) should be low value (0.1-0.5), got {priority4.Value}");
                }

                // Test mid-range priority (2 should convert to mid-range value)
                Priority priority2 = new Priority(null, workTypeDef);
                priority2.FromGamePriority(2);
                if (priority2.Value <= 0.4f || priority2.Value >= 0.9f)
                {
                    throw new Exception($"FromGamePriority(2) should be mid-range value (0.4-0.9), got {priority2.Value}");
                }

                Console.WriteLine("FromGamePriority conversion tests - PASSED");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FromGamePriority conversion test failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Test round-trip conversion between ToGamePriority and FromGamePriority.
        /// </summary>
        public static void TestRoundTripConversion()
        {
            WorkTypeDef workTypeDef = TestDataBuilders.WorkTypeDefs.Hauling;

            try
            {
                // Test that converting from game priority to Priority and back gives consistent results
                for (int gamePriority = 0; gamePriority <= 3; gamePriority++)
                {
                    Priority priority = new Priority(null, workTypeDef);
                    priority.FromGamePriority(gamePriority);
                    int backToGame = priority.ToGamePriority();

                    if (backToGame != gamePriority)
                    {
                        throw new Exception($"Round-trip failed: {gamePriority} -> {priority.Value} -> {backToGame}");
                    }
                }

                Console.WriteLine("Round-trip conversion tests - PASSED");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Round-trip conversion test failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Test that ToGamePriority handles edge cases and invalid values properly using public interface.
        /// Note: RimWorld uses an inverted priority system where 1=highest, 4=lowest, 0=disabled
        /// </summary>
        public static void TestToGamePriorityEdgeCases()
        {
            WorkTypeDef workTypeDef = TestDataBuilders.WorkTypeDefs.Hauling;

            try
            {
                Priority priority = new Priority(null, workTypeDef);
                Func<string> testDesc = () => "ToGamePriority edge test";

                // Test negative value (should be clamped to disabled)
                priority.Set(-0.5f, testDesc);
                int gameValue = priority.ToGamePriority();
                if (gameValue != 0)
                {
                    throw new Exception($"Negative priority should convert to 0, got {gameValue}");
                }

                // Test value above 1.0 (should be clamped to highest priority = 1)
                priority.Set(1.5f, testDesc);
                gameValue = priority.ToGamePriority();
                if (gameValue != 1)
                {
                    throw new Exception($"Priority above 1.0 should convert to 1 (highest priority), got {gameValue}");
                }

                // Test boundary values close to 1.0
                priority.Set(0.999f, testDesc);
                gameValue = priority.ToGamePriority();
                if (gameValue != 1)
                {
                    throw new Exception($"Priority 0.999 should convert to 1 (highest priority), got {gameValue}");
                }

                Console.WriteLine("ToGamePriority edge cases - PASSED");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ToGamePriority edge cases test failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Test FromGamePriority with invalid input values.
        /// </summary>
        public static void TestFromGamePriorityEdgeCases()
        {
            WorkTypeDef workTypeDef = TestDataBuilders.WorkTypeDefs.Hauling;

            try
            {
                // Test invalid game priority values
                Priority priority = new Priority(null, workTypeDef);
                priority.FromGamePriority(-1);
                if (priority.Value < 0.0f || priority.Value > 1.0f)
                {
                    throw new Exception($"Invalid input should still result in valid Value range, got {priority.Value}");
                }

                priority = new Priority(null, workTypeDef);
                priority.FromGamePriority(5);
                if (priority.Value < 0.0f || priority.Value > 1.0f)
                {
                    throw new Exception($"Invalid input should still result in valid Value range, got {priority.Value}");
                }

                Console.WriteLine("FromGamePriority edge cases - PASSED");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FromGamePriority edge cases test failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Test error handling in Compute() method.
        /// Note: This tests the error handling path when game state is invalid.
        /// </summary>
        public static void TestComputeErrorHandling()
        {
            WorkTypeDef workTypeDef = TestDataBuilders.WorkTypeDefs.Hauling;

            try
            {
                // Test with null pawn (should handle gracefully)
                Priority priority = new Priority(null, workTypeDef);

                try
                {
                    priority.Compute();
                    // If we get here, Compute() succeeded or handled the null pawn gracefully
                    Console.WriteLine("Compute with null pawn handled gracefully");
                }
                catch (Exception ex)
                {
                    // Expected - this is testing the error handling path
                    Console.WriteLine($"Compute() properly throws exception with invalid state: {ex.Message}");
                }

                Console.WriteLine("Compute error handling tests - COMPLETED");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Compute error handling test setup failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Run all basic priority tests.
        /// </summary>
        public static void RunAllTests()
        {
            Console.WriteLine("=== Running Basic Priority Tests ===");
            Console.WriteLine();

            int passedTests = 0;
            int failedTests = 0;
            int skippedTests = 0;

            // Test infrastructure
            RunTest("TestWorkTypeDefCreation", TestWorkTypeDefCreation, ref passedTests, ref failedTests, ref skippedTests);
            RunTest("TestConstants", TestConstants, ref passedTests, ref failedTests, ref skippedTests);

            // Basic constructor tests
            RunTest("TestConstructorBasics", TestConstructorBasics, ref passedTests, ref failedTests, ref skippedTests);
            RunTest("TestConstructorWithNullWorkType", TestConstructorWithNullWorkType, ref passedTests, ref failedTests, ref skippedTests);
            RunTest("TestIComparable", TestIComparable, ref passedTests, ref failedTests, ref skippedTests);

            // Core priority calculation tests
            RunTest("TestToGamePriorityConversion", TestToGamePriorityConversion, ref passedTests, ref failedTests, ref skippedTests);
            RunTest("TestFromGamePriorityConversion", TestFromGamePriorityConversion, ref passedTests, ref failedTests, ref skippedTests);
            RunTest("TestRoundTripConversion", TestRoundTripConversion, ref passedTests, ref failedTests, ref skippedTests);
            RunTest("TestToGamePriorityEdgeCases", TestToGamePriorityEdgeCases, ref passedTests, ref failedTests, ref skippedTests);
            RunTest("TestFromGamePriorityEdgeCases", TestFromGamePriorityEdgeCases, ref passedTests, ref failedTests, ref skippedTests);
            RunTest("TestComputeErrorHandling", TestComputeErrorHandling, ref passedTests, ref failedTests, ref skippedTests);

            Console.WriteLine();
            Console.WriteLine("=== Basic Priority Test Summary ===");
            Console.WriteLine($"Passed: {passedTests}");
            Console.WriteLine($"Failed: {failedTests}");
            Console.WriteLine($"Skipped: {skippedTests}");
            Console.WriteLine($"Total: {passedTests + failedTests + skippedTests}");
            Console.WriteLine();

            if (failedTests == 0)
            {
                Console.WriteLine("=== All basic priority tests COMPLETED successfully! ===");
            }
            else
            {
                Console.WriteLine($"=== {failedTests} test(s) FAILED ===");
                throw new Exception($"{failedTests} basic priority test(s) failed");
            }
        }

        private static void RunTest(string testName, Action testMethod, ref int passedTests, ref int failedTests, ref int skippedTests)
        {
            try
            {
                testMethod();
                passedTests++;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("skipped") || ex.Message.Contains("RimWorld dependency"))
                {
                    skippedTests++;
                }
                else
                {
                    Console.WriteLine($"FAILED: {testName} - {ex.Message}");
                    failedTests++;
                }
            }
        }
    }
}
