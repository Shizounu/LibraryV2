using System;
using System.Collections.Generic;
using System.Linq;

namespace Shizounu.Library.RandomSystem.Tests
{
    /// <summary>
    /// Unit tests for the RNG system.
    /// Demonstrates testing patterns and validates system correctness.
    /// </summary>
    public static class RngSystemTests
    {
        /// <summary>
        /// Test that the same seed produces the same sequence.
        /// </summary>
        public static bool TestDeterministicGeneration()
        {
            var rng1 = new XorshiftRng(12345);
            var rng2 = new XorshiftRng(12345);

            for (int i = 0; i < 100; i++)
            {
                if (rng1.Next() != rng2.Next())
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Test that different seeds produce different sequences.
        /// </summary>
        public static bool TestDifferentSeedsProduceDifferentSequences()
        {
            var rng1 = new XorshiftRng(12345);
            var rng2 = new XorshiftRng(54321);

            var values1 = new List<uint>();
            var values2 = new List<uint>();

            for (int i = 0; i < 10; i++)
            {
                values1.Add(rng1.Next());
                values2.Add(rng2.Next());
            }

            // Should have at least some differences
            return !values1.SequenceEqual(values2);
        }

        /// <summary>
        /// Test that multiple users can generate from their own sources.
        /// </summary>
        public static bool TestMultipleUsers()
        {
            var context = new RngContext();

            var user1 = context.GetOrCreateUser("User1", seed: 111);
            var user2 = context.GetOrCreateUser("User2", seed: 222);

            // Get values from both users
            for (int i = 0; i < 10; i++)
            {
                user1.Next();
                user2.Next();
            }

            // Should have different generation counts if they didn't interfere
            return user1.GenerationCount == user2.GenerationCount;
        }

        /// <summary>
        /// Test snapshot and restore functionality.
        /// </summary>
        public static bool TestSnapshotAndRestore()
        {
            var context = new RngContext();
            var user = context.GetOrCreateUser("Test", seed: 999);

            // Generate some values
            var values1 = new List<int>();
            for (int i = 0; i < 5; i++)
                values1.Add(user.NextInt(1, 100));

            // Create snapshot
            var snapshot = context.CreateSnapshot("test_point");

            // Generate more values
            var values2 = new List<int>();
            for (int i = 0; i < 5; i++)
                values2.Add(user.NextInt(1, 100));

            // Restore snapshot
            context.RestoreFromSnapshot(snapshot);

            // Generate values again - should match values2 exactly
            var values2Replay = new List<int>();
            for (int i = 0; i < 5; i++)
                values2Replay.Add(user.NextInt(1, 100));

            return values2.SequenceEqual(values2Replay);
        }

        /// <summary>
        /// Test history recording.
        /// </summary>
        public static bool TestHistoryRecording()
        {
            var context = new RngContext();
            var user = context.GetOrCreateUser("HistoryTest", seed: 444);

            int initialCount = context.History.EntryCount;

            user.Next("test1");
            user.Next("test2");
            user.Next("test3");

            // Should have 3 new entries
            if (context.History.EntryCount != initialCount + 3)
                return false;

            // Verify labels
            var entries = context.History.GetAllEntries();
            if (entries[initialCount].Label != "test1")
                return false;
            if (entries[initialCount + 1].Label != "test2")
                return false;
            if (entries[initialCount + 2].Label != "test3")
                return false;

            return true;
        }

        /// <summary>
        /// Test history navigation.
        /// </summary>
        public static bool TestHistoryNavigation()
        {
            var context = new RngContext();
            var user = context.GetOrCreateUser("NavTest", seed: 555);

            // Generate values
            for (int i = 0; i < 5; i++)
                user.Next();

            // Should be at step 5
            if (context.History.CurrentStep != 5)
                return false;

            // Step backward
            if (!context.History.StepBackward())
                return false;
            if (context.History.CurrentStep != 4)
                return false;

            // Step forward
            if (!context.History.StepForward())
                return false;
            if (context.History.CurrentStep != 5)
                return false;

            // Can't step backward past 0
            for (int i = 0; i < 10; i++)
                context.History.StepBackward();

            if (context.History.CurrentStep != 0)
                return false;

            // Can't step backward anymore
            if (context.History.StepBackward())
                return false;

            return true;
        }

        /// <summary>
        /// Test permutation comparison.
        /// </summary>
        public static bool TestPermutationComparison()
        {
            var context = new RngContext();
            var user = context.GetOrCreateUser("PermTest", seed: 666);

            // Create variations
            var vars = new List<RngSnapshot>();
            for (int i = 0; i < 3; i++)
            {
                user.NextInt(1, 100);
                vars.Add(context.CreateSnapshot($"var_{i}"));
            }

            // Compare
            var comparison = context.ComparePermutations(vars, snap =>
            {
                var u = context.Users.Values.First();
                u.NextInt(1, 100);
            });

            // Should have 3 branches
            if (comparison.BranchCount != 3)
                return false;

            // Average variance should be between 0 and 1
            float variance = comparison.GetAverageVariance();
            if (variance < 0 || variance > 1)
                return false;

            return true;
        }

        /// <summary>
        /// Test disabling history recording for performance.
        /// </summary>
        public static bool TestHistoryDisable()
        {
            var context = new RngContext();
            var user = context.GetOrCreateUser("DisableTest", seed: 777);

            int beforeCount = context.History.EntryCount;

            // Disable history
            context.SetRecordHistory(false);
            user.Next();
            user.Next();

            // Should still be at before count
            if (context.History.EntryCount != beforeCount)
                return false;

            // Re-enable
            context.SetRecordHistory(true);
            user.Next();

            // Should have one new entry
            if (context.History.EntryCount != beforeCount + 1)
                return false;

            return true;
        }

        /// <summary>
        /// Test cloning RNG sources.
        /// </summary>
        public static bool TestSourceCloning()
        {
            var rng1 = new XorshiftRng(9999);
            var rng1Clone = rng1.Clone();

            // Get values from both
            var vals1 = new List<uint>();
            var valsClone = new List<uint>();

            for (int i = 0; i < 10; i++)
            {
                vals1.Add(rng1.Next());
                valsClone.Add(rng1Clone.Next());
            }

            // Should be identical
            return vals1.SequenceEqual(valsClone);
        }

        /// <summary>
        /// Runs all tests and returns results.
        /// </summary>
        public static void RunAllTests()
        {
            var tests = new Dictionary<string, Func<bool>>
            {
                { "Deterministic Generation", TestDeterministicGeneration },
                { "Different Seeds Different Sequences", TestDifferentSeedsProduceDifferentSequences },
                { "Multiple Users", TestMultipleUsers },
                { "Snapshot and Restore", TestSnapshotAndRestore },
                { "History Recording", TestHistoryRecording },
                { "History Navigation", TestHistoryNavigation },
                { "Permutation Comparison", TestPermutationComparison },
                { "History Disable", TestHistoryDisable },
                { "Source Cloning", TestSourceCloning }
            };

            int passed = 0;
            int failed = 0;

            UnityEngine.Debug.Log("=== RNG SYSTEM TESTS ===\n");

            foreach (var test in tests)
            {
                try
                {
                    bool result = test.Value();
                    if (result)
                    {
                        UnityEngine.Debug.Log($"✓ PASS: {test.Key}");
                        passed++;
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"✗ FAIL: {test.Key}");
                        failed++;
                    }
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.Log($"✗ ERROR: {test.Key} - {ex.Message}");
                    failed++;
                }
            }

            UnityEngine.Debug.Log($"\n=== RESULTS ===");
            UnityEngine.Debug.Log($"Passed: {passed}");
            UnityEngine.Debug.Log($"Failed: {failed}");
            UnityEngine.Debug.Log($"Total: {passed + failed}");
        }
    }
}
