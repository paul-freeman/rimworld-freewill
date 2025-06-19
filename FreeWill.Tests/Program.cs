using System;
using FreeWill.Tests.TestHelpers;

namespace FreeWill.Tests
{
    /// <summary>
    /// Test runner program for FreeWill Priority tests.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Simple FreeWill Smoke Test ===");

            try
            {
                // Test basic constants
                if (TestDataBuilders.Values.DefaultPriority == 0.2f)
                {
                    Console.WriteLine("✓ Test constants working");
                }
                else
                {
                    Console.WriteLine("✗ Test constants failed");
                    return;
                }

                // Test mock work types
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
                Console.WriteLine("=== Running Full Priority Tests ===");
                Console.WriteLine("Note: Some tests may fail or be skipped due to RimWorld dependencies");
                Console.WriteLine();

                // Run the full test suite
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
