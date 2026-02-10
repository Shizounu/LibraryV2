using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Shizounu.Library.RandomSystem.Examples
{
    /// <summary>
    /// Demonstrates the performance difference between regular and lightweight snapshots
    /// in performance-critical algorithms like Mini-Max and BSP.
    /// </summary>
    public class RngPerformanceExample : MonoBehaviour
    {
        private void Start()
        {
            UnityEngine.Debug.Log("=== RNG PERFORMANCE COMPARISON ===\n");

            // Comparison of snapshot performance
            CompareSnapshotPerformance();

            UnityEngine.Debug.Log("\n=== MINI-MAX ALGORITHM EXAMPLE ===\n");
            MiniMaxExample();

            UnityEngine.Debug.Log("\n=== BSP ALGORITHM EXAMPLE ===\n");
            BspExample();

            UnityEngine.Debug.Log("\n=== MONTE CARLO SIMULATION EXAMPLE ===\n");
            MonteCarloExample();
        }

        /// <summary>
        /// Directly compares performance between snapshot types.
        /// </summary>
        private void CompareSnapshotPerformance()
        {
            var context = new RngContext();
            var user1 = context.GetOrCreateUser("User1", seed: 123);
            var user2 = context.GetOrCreateUser("User2", seed: 456);
            var user3 = context.GetOrCreateUser("User3", seed: 789);

            const int iterations = 10000;

            // Disable history recording for fair comparison
            context.SetRecordHistory(false);

            // Test regular snapshots
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var snapshot = context.CreateSnapshot();
                user1.Next();
                user2.Next();
                user3.Next();
                context.RestoreFromSnapshot(snapshot);
            }
            sw.Stop();
            long regularMs = sw.ElapsedMilliseconds;
            long regularMemory = GC.GetTotalMemory(false);

            // Clear snapshots
            context.ClearHistory();
            GC.Collect();

            // Test lightweight snapshots
            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                var snapshot = context.CreateLightweightSnapshot();
                user1.Next();
                user2.Next();
                user3.Next();
                context.RestoreFromLightweightSnapshot(snapshot);
            }
            sw.Stop();
            long lightweightMs = sw.ElapsedMilliseconds;
            long lightweightMemory = GC.GetTotalMemory(false);

            UnityEngine.Debug.Log($"Regular Snapshots: {regularMs}ms");
            UnityEngine.Debug.Log($"Lightweight Snapshots: {lightweightMs}ms");
            UnityEngine.Debug.Log($"Speedup: {(float)regularMs / lightweightMs:F1}x faster");
            UnityEngine.Debug.Log($"Memory saved: ~{(regularMemory - lightweightMemory) / 1024}KB");
        }

        /// <summary>
        /// Example of using lightweight snapshots in a Mini-Max game tree search.
        /// </summary>
        private void MiniMaxExample()
        {
            var context = new RngContext();
            var evalRng = context.GetOrCreateUser("Evaluation", seed: 42);

            // Disable history for max performance
            context.SetRecordHistory(false);

            // Use snapshot stack for automatic state management
            using var snapshotStack = context.CreateSnapshotStack();

            int nodesEvaluated = 0;
            var sw = Stopwatch.StartNew();

            // Simulate Mini-Max tree search (depth 5)
            int bestScore = MiniMax(evalRng, snapshotStack, depth: 5, isMaximizing: true, ref nodesEvaluated);

            sw.Stop();

            UnityEngine.Debug.Log($"Mini-Max completed:");
            UnityEngine.Debug.Log($"  Best score: {bestScore}");
            UnityEngine.Debug.Log($"  Nodes evaluated: {nodesEvaluated}");
            UnityEngine.Debug.Log($"  Time: {sw.ElapsedMilliseconds}ms");
            UnityEngine.Debug.Log($"  Avg per node: {(float)sw.ElapsedMilliseconds / nodesEvaluated:F4}ms");
        }

        private int MiniMax(RngUser rng, RngSnapshotStack stack, int depth, bool isMaximizing, ref int nodesEvaluated)
        {
            nodesEvaluated++;

            // Base case: evaluate position
            if (depth == 0)
            {
                return rng.NextInt(0, 100); // Simulated position evaluation
            }

            int bestValue = isMaximizing ? int.MinValue : int.MaxValue;

            // Try multiple moves (branches)
            const int movesPerPosition = 3;
            for (int move = 0; move < movesPerPosition; move++)
            {
                // Push current state before trying move
                stack.Push();

                // Simulate making a move (generates some random values)
                rng.Next();

                // Recurse
                int value = MiniMax(rng, stack, depth - 1, !isMaximizing, ref nodesEvaluated);

                // Restore state after exploring this branch
                stack.Pop();

                // Update best value
                if (isMaximizing)
                    bestValue = Math.Max(bestValue, value);
                else
                    bestValue = Math.Min(bestValue, value);
            }

            return bestValue;
        }

        /// <summary>
        /// Example of using lightweight snapshots in BSP tree generation.
        /// </summary>
        private void BspExample()
        {
            var context = new RngContext();
            var bspRng = context.GetOrCreateUser("BSP", seed: 999);

            context.SetRecordHistory(false);

            var sw = Stopwatch.StartNew();
            int nodesCreated = 0;

            // Generate BSP tree
            var root = GenerateBspTree(context, bspRng, new Rect(0, 0, 100, 100), depth: 6, ref nodesCreated);

            sw.Stop();

            UnityEngine.Debug.Log($"BSP generation completed:");
            UnityEngine.Debug.Log($"  Nodes created: {nodesCreated}");
            UnityEngine.Debug.Log($"  Time: {sw.ElapsedMilliseconds}ms");
            UnityEngine.Debug.Log($"  Avg per node: {(float)sw.ElapsedMilliseconds / nodesCreated:F4}ms");
        }

        private class BspNode
        {
            public Rect Area;
            public BspNode Left;
            public BspNode Right;
        }

        private BspNode GenerateBspTree(RngContext context, RngUser rng, Rect area, int depth, ref int nodesCreated)
        {
            nodesCreated++;

            var node = new BspNode { Area = area };

            if (depth <= 0 || area.width < 10 || area.height < 10)
                return node; // Leaf node

            // Try different split positions using lightweight snapshots
            var bestSnapshot = context.CreateLightweightSnapshot();
            float bestScore = float.MaxValue;

            // Test several split options
            const int splitsToTest = 3;
            for (int i = 0; i < splitsToTest; i++)
            {
                var snapshot = context.CreateLightweightSnapshot();

                bool splitHorizontal = rng.NextFloat() > 0.5f;
                float splitRatio = rng.NextFloat() * 0.4f + 0.3f; // 0.3 to 0.7

                // Evaluate this split
                float score = EvaluateSplit(rng, splitHorizontal, splitRatio);

                if (score < bestScore)
                {
                    bestScore = score;
                    bestSnapshot = snapshot;
                }
            }

            // Use the best split
            context.RestoreFromLightweightSnapshot(bestSnapshot);
            bool useSplitH = rng.NextFloat() > 0.5f;
            float useRatio = rng.NextFloat() * 0.4f + 0.3f;

            // Create child nodes (simplified - not actually splitting the rect properly)
            if (useSplitH)
            {
                float splitY = area.y + area.height * useRatio;
                // Would create left/right children here
            }

            return node;
        }

        private float EvaluateSplit(RngUser rng, bool horizontal, float ratio)
        {
            // Simulate split evaluation
            return rng.NextFloat() * 100;
        }

        /// <summary>
        /// Example of Monte Carlo simulation with lightweight snapshots.
        /// </summary>
        private void MonteCarloExample()
        {
            var context = new RngContext();
            var simRng = context.GetOrCreateUser("Simulation", seed: 777);

            context.SetRecordHistory(false);

            const int simulations = 10000;
            var sw = Stopwatch.StartNew();

            // Save initial state
            var initialState = context.CreateLightweightSnapshot();

            int successCount = 0;

            for (int i = 0; i < simulations; i++)
            {
                // Restore to initial state for each simulation
                context.RestoreFromLightweightSnapshot(initialState);

                // Run simulation
                bool success = RunSimulation(simRng);
                if (success)
                    successCount++;
            }

            sw.Stop();

            float successRate = (float)successCount / simulations;

            UnityEngine.Debug.Log($"Monte Carlo simulation completed:");
            UnityEngine.Debug.Log($"  Simulations: {simulations}");
            UnityEngine.Debug.Log($"  Success rate: {successRate:P}");
            UnityEngine.Debug.Log($"  Time: {sw.ElapsedMilliseconds}ms");
            UnityEngine.Debug.Log($"  Avg per simulation: {(float)sw.ElapsedMilliseconds / simulations:F4}ms");
        }

        private bool RunSimulation(RngUser rng)
        {
            // Simulate some process with random elements
            int score = 0;
            for (int step = 0; step < 10; step++)
            {
                score += rng.NextInt(0, 10);
            }
            return score > 45; // Success if score above threshold
        }
    }

    /// <summary>
    /// Example of using snapshot pooling for extreme performance (millions of snapshots).
    /// </summary>
    public class RngPoolingExample : MonoBehaviour
    {
        private void Start()
        {
            UnityEngine.Debug.Log("=== RNG POOLING EXAMPLE ===\n");

            var context = new RngContext();
            var user = context.GetOrCreateUser("PoolTest", seed: 12345);
            context.SetRecordHistory(false);

            const int iterations = 100000;

            // Without pooling
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var snapshot = context.CreateLightweightSnapshot();
                user.Next();
                context.RestoreFromLightweightSnapshot(snapshot);
            }
            sw.Stop();
            long nopoolMs = sw.ElapsedMilliseconds;
            GC.Collect();

            // With pooling
            var pool = new RngSnapshotPool(maxUserCount: 4, maxPoolSize: 100);

            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                var snapshot = pool.CreatePooledSnapshot(context);
                user.Next();
                snapshot.Restore(context);
                pool.ReturnSnapshot(ref snapshot);
            }
            sw.Stop();
            long pooledMs = sw.ElapsedMilliseconds;

            UnityEngine.Debug.Log($"Without pooling: {nopoolMs}ms");
            UnityEngine.Debug.Log($"With pooling: {pooledMs}ms");
            UnityEngine.Debug.Log($"Speedup: {(float)nopoolMs / pooledMs:F1}x faster");
            UnityEngine.Debug.Log($"Pool size: {pool.PooledCount} arrays cached");
        }
    }
}
