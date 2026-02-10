using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Shizounu.Library.RandomSystem.Examples
{
    /// <summary>
    /// Example demonstrating the custom RNG system usage.
    /// Shows how to:
    /// - Create RNG users with different sources
    /// - Record history
    /// - Create snapshots
    /// - Rewind and compare different permutations
    /// </summary>
    public class RngSystemExample : MonoBehaviour
    {
        private RngContext _rngContext;

        private void Start()
        {
            // Create the RNG context
            _rngContext = new RngContext();

            // Example 1: Basic usage with multiple users
            BasicUsageExample();

            // Example 2: Snapshots and rewinding
            SnapshotExample();

            // Example 3: Comparing permutations
            PermutationComparisonExample();

            // Example 4: Custom RNG sources
            CustomRngSourceExample();
        }

        /// <summary>
        /// Demonstrates basic RNG usage with multiple independent users.
        /// </summary>
        private void BasicUsageExample()
        {
            Debug.Log("=== BASIC USAGE EXAMPLE ===");

            // Create two RNG users from the same context
            var playerAttackRoll = _rngContext.GetOrCreateUser("PlayerAttack", seed: 12345);
            var enemyAttackRoll = _rngContext.GetOrCreateUser("EnemyAttack", seed: 54321);
            var damageRoll = _rngContext.GetOrCreateUser("DamageCalculation", seed: 99999);

            Debug.Log($"Player Attack Roll: {playerAttackRoll.NextInt(1, 21, "attack_d20")}");
            Debug.Log($"Enemy Attack Roll: {enemyAttackRoll.NextInt(1, 21, "attack_d20")}");
            Debug.Log($"Damage Roll: {damageRoll.NextInt(1, 11, "damage_d10")}");

            Debug.Log(_rngContext);
            Debug.Log($"Total history entries: {_rngContext.History.EntryCount}");
        }

        /// <summary>
        /// Demonstrates creating snapshots and rewinding.
        /// </summary>
        private void SnapshotExample()
        {
            Debug.Log("\n=== SNAPSHOT & REWIND EXAMPLE ===");

            _rngContext.Reset();

            var attackRoll = _rngContext.GetOrCreateUser("CombatAttack", seed: 11111);
            var damageRoll = _rngContext.GetOrCreateUser("CombatDamage", seed: 22222);

            // Perform a turn of combat
            Debug.Log("--- Turn 1 ---");
            int attack1 = attackRoll.NextInt(1, 21, "turn1_attack");
            int damage1 = damageRoll.NextInt(5, 15, "turn1_damage");
            Debug.Log($"Attack: {attack1}, Damage: {damage1}");

            // Create a snapshot before the next turn
            var beforeTurn2 = _rngContext.CreateSnapshot("before_turn_2");
            Debug.Log($"Snapshot created: {beforeTurn2}");

            // Perform next turn
            Debug.Log("--- Turn 2 (Original) ---");
            int attack2 = attackRoll.NextInt(1, 21, "turn2_attack");
            int damage2 = damageRoll.NextInt(5, 15, "turn2_damage");
            Debug.Log($"Attack: {attack2}, Damage: {damage2}");

            // Rewind to the snapshot (before turn 2)
            Debug.Log("--- Rewinding to before Turn 2 ---");
            _rngContext.RestoreFromSnapshot(beforeTurn2);

            // Perform turn 2 again - should get same results
            Debug.Log("--- Turn 2 (Replayed) ---");
            int attack2Replay = attackRoll.NextInt(1, 21, "turn2_attack");
            int damage2Replay = damageRoll.NextInt(5, 15, "turn2_damage");
            Debug.Log($"Attack: {attack2Replay}, Damage: {damage2Replay}");
            Debug.Log($"Results match: {attack2 == attack2Replay && damage2 == damage2Replay}");
        }

        /// <summary>
        /// Demonstrates comparing different permutation branches.
        /// </summary>
        private void PermutationComparisonExample()
        {
            Debug.Log("\n=== PERMUTATION COMPARISON EXAMPLE ===");

            _rngContext.Reset();

            var roll = _rngContext.GetOrCreateUser("ComparisonRoll", seed: 77777);

            // Simulate some activity and create a reference snapshot
            roll.NextInt(1, 100);
            var referenceSnapshot = _rngContext.CreateSnapshot("reference");

            // Create multiple snapshots from the same reference point
            var snapshots = new List<RngSnapshot>();
            for (int i = 0; i < 3; i++)
            {
                roll.NextInt(1, 1000);
                snapshots.Add(_rngContext.CreateSnapshot($"variation_{i}"));
            }

            // Compare what happens when we generate values from each snapshot
            var comparison = _rngContext.ComparePermutations(snapshots, snapshot =>
            {
                var localRoll = _rngContext.Users.Values.First();
                for (int i = 0; i < 5; i++)
                {
                    localRoll.NextInt(1, 100, $"test_roll_{i}");
                }
            });

            Debug.Log(comparison.GetComparisonSummary());
        }

        /// <summary>
        /// Demonstrates using custom RNG sources.
        /// </summary>
        private void CustomRngSourceExample()
        {
            Debug.Log("\n=== CUSTOM RNG SOURCE EXAMPLE ===");

            _rngContext.Reset();

            // Use different RNG algorithms for different purposes
            var xorshiftUser = _rngContext.GetOrCreateUser(
                "FastRandom",
                new XorshiftRng(12345)
            );

            // Generate some values
            Debug.Log("Xorshift RNG:");
            for (int i = 0; i < 5; i++)
            {
                Debug.Log($"  Value {i}: {xorshiftUser.NextFloat():F4}");
            }

            // You could implement other RNG sources by inheriting IRngSource
            // Example: PCGRng, MersenneTwister, etc.
        }
    }
}
