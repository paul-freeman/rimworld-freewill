using System;
using FreeWill.Tests.IntegrationTests;
using FreeWill.Tests.StrategyTests;
using FreeWill.Tests.TestHelpers;

namespace FreeWill.Tests
{
    /// <summary>
    /// Test runner program for FreeWill Priority tests.
    /// Runs the reorganized test suite from multiple test classes.
    /// </summary>
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("=== FreeWill Test Suite ===");
            Console.WriteLine("Running reorganized tests from multiple test classes");
            Console.WriteLine();

            try
            {
                // Test basic constants (smoke test)
                if (TestDataBuilders.Values.DefaultPriority == 0.2f)
                {
                    Console.WriteLine("✓ Test constants working");
                }
                else
                {
                    Console.WriteLine("✗ Test constants failed");
                    return;
                }

                // Test mock work types (smoke test)
                if (MockGameObjects.WorkTypes.Hauling == "Hauling")
                {
                    Console.WriteLine("✓ Mock work types working");
                }
                else
                {
                    Console.WriteLine("✗ Mock work types failed");
                    return;
                }

                Console.WriteLine();
                Console.WriteLine("=== Running Reorganized Test Suite ===");
                Console.WriteLine("Note: Some tests may be skipped due to RimWorld dependencies");
                Console.WriteLine();

                // Run organized test suites
                Console.WriteLine("1. Running Basic Priority Tests...");
                BasicPriorityTests.RunAllTests();

                Console.WriteLine();
                Console.WriteLine("2. Running Consider Method Tests...");
                ConsiderMethodTests.RunAllTests();
                Console.WriteLine();
                Console.WriteLine("3. Running Strategy Tests...");
                StrategyTests.FirefighterStrategyTests.RunAllTests();

                Console.WriteLine();
                StrategyTests.DoctorStrategyTests.RunAllTests();

                Console.WriteLine();
                StrategyTests.PatientStrategyTests.RunAllTests();

                Console.WriteLine();
                StrategyTests.PatientBedRestStrategyTests.RunAllTests();

                Console.WriteLine();
                StrategyTests.CookingStrategyTests.RunAllTests();

                Console.WriteLine();
                StrategyTests.HuntingStrategyTests.RunAllTests();

                Console.WriteLine();
                StrategyTests.ConstructionStrategyTests.RunAllTests();

                Console.WriteLine();
                StrategyTests.GrowingStrategyTests.RunAllTests();

                Console.WriteLine();
                StrategyTests.MiningStrategyTests.RunAllTests();

                Console.WriteLine();
                StrategyTests.PlantCuttingStrategyTests.RunAllTests();

                Console.WriteLine();
                StrategyTests.SmithingStrategyTests.RunAllTests();

                Console.WriteLine();
                StrategyTests.TailoringStrategyTests.RunAllTests();

                Console.WriteLine();
                StrategyTests.ArtStrategyTests.RunAllTests();

                Console.WriteLine();
                StrategyTests.CraftingStrategyTests.RunAllTests();

                Console.WriteLine();
                StrategyTests.HaulingStrategyTests.RunAllTests();

                Console.WriteLine();
                StrategyTests.CleaningStrategyTests.RunAllTests();

                Console.WriteLine();
                StrategyTests.ResearchingStrategyTests.RunAllTests();

                Console.WriteLine();
                StrategyTests.HaulingUrgentStrategyTests.RunAllTests();

                Console.WriteLine();
                Console.WriteLine("4. Running Specialized Strategy Tests...");
                StrategyTests.ChildcareStrategyTests.RunAllTests();

                Console.WriteLine();
                StrategyTests.WardenStrategyTests.RunAllTests();

                Console.WriteLine();
                StrategyTests.HandlingStrategyTests.RunAllTests();

                Console.WriteLine();
                StrategyTests.BasicWorkerStrategyTests.RunAllTests();

                Console.WriteLine();
                StrategyTests.DefaultWorkTypeStrategyTests.RunAllTests();

                Console.WriteLine();
                Console.WriteLine("5. Running Integration Tests...");
                PriorityCalculationIntegrationTests.RunAllTests();

                Console.WriteLine();
                Console.WriteLine("=== LEGACY TESTS (for comparison) ===");
                Console.WriteLine("Running original PriorityTests to verify reorganization...");
                PriorityTests.RunAllTests();

                Console.WriteLine();
                Console.WriteLine("=== SMOKE TEST PASSED ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Smoke test failed: {ex.Message}");
                Environment.Exit(1);
            }
        }
    }
}
