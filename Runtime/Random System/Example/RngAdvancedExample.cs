using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shizounu.Library.RandomSystem.Examples
{
    /// <summary>
    /// Advanced examples demonstrating complex RNG system usage.
    /// Shows:
    /// - Complex game simulation scenarios
    /// - AI decision making with branching
    /// - Detailed permutation analysis
    /// - Custom RNG source implementations
    /// </summary>
    public class RngAdvancedExample : MonoBehaviour
    {
        private RngContext _rngContext;

        private void Start()
        {
            // Example 1: Combat simulation with multiple attempts
            CombatSimulationExample();

            // Example 2: AI decision tree with different outcomes
            AiDecisionExample();

            // Example 3: Detailed permutation analysis
            DetailedPermutationAnalysis();
        }

        /// <summary>
        /// Simulates combat between two combatants with multiple random factors.
        /// </summary>
        private void CombatSimulationExample()
        {
            Debug.Log("=== COMBAT SIMULATION EXAMPLE ===");

            _rngContext = new RngContext();

            var playerAttack = _rngContext.GetOrCreateUser("PlayerAttack", seed: 42);
            var playerDamage = _rngContext.GetOrCreateUser("PlayerDamage", seed: 43);
            var enemyAttack = _rngContext.GetOrCreateUser("EnemyAttack", seed: 44);
            var enemyDamage = _rngContext.GetOrCreateUser("EnemyDamage", seed: 45);

            // Simulate multiple combat rounds
            for (int round = 1; round <= 3; round++)
            {
                Debug.Log($"\n--- Round {round} ---");

                // Player attacks
                int playerHit = playerAttack.NextInt(1, 21, "player_attack_roll");
                if (playerHit >= 12)  // Hit threshold
                {
                    int damage = playerDamage.NextInt(8, 15, "player_damage");
                    Debug.Log($"Player hits! (rolled {playerHit}) for {damage} damage");
                }
                else
                {
                    Debug.Log($"Player misses (rolled {playerHit})");
                }

                // Enemy attacks
                int enemyHit = enemyAttack.NextInt(1, 21, "enemy_attack_roll");
                if (enemyHit >= 10)
                {
                    int damage = enemyDamage.NextInt(6, 12, "enemy_damage");
                    Debug.Log($"Enemy hits! (rolled {enemyHit}) for {damage} damage");
                }
                else
                {
                    Debug.Log($"Enemy misses (rolled {enemyHit})");
                }
            }

            Debug.Log($"\nTotal generations: {_rngContext.History.EntryCount}");
            Debug.Log(_rngContext.History.GetUserEntries(playerAttack.Id).Count + " player attack rolls");
            Debug.Log(_rngContext.History.GetUserEntries(enemyAttack.Id).Count + " enemy attack rolls");
        }

        /// <summary>
        /// Demonstrates AI making different decisions based on RNG outcomes.
        /// </summary>
        private void AiDecisionExample()
        {
            Debug.Log("\n=== AI DECISION MAKING EXAMPLE ===");

            _rngContext = new RngContext();

            var aiDecision = _rngContext.GetOrCreateUser("AiDecision", seed: 100);

            // Create a snapshot before the AI makes its decision
            var beforeDecision = _rngContext.CreateSnapshot("ai_decision_point");

            Debug.Log("--- AI Scenario 1: Attack ---");
            SimulateAiBehavior(aiDecision);

            // Rewind to the same point
            _rngContext.RestoreFromSnapshot(beforeDecision);

            Debug.Log("\n--- AI Scenario 2: Defend (same RNG point) ---");
            SimulateAiBehavior(aiDecision);
        }

        private void SimulateAiBehavior(RngUser aiDecision)
        {
            int behavior = aiDecision.NextInt(0, 3, "ai_behavior_choice");

            switch (behavior)
            {
                case 0:
                    Debug.Log("AI: Aggressive behavior");
                    Debug.Log($"  Attack roll: {aiDecision.NextInt(1, 20, "ai_attack")}");
                    break;
                case 1:
                    Debug.Log("AI: Defensive behavior");
                    Debug.Log($"  Defense boost: {aiDecision.NextInt(5, 15, "ai_defense")}");
                    break;
                case 2:
                    Debug.Log("AI: Evasive behavior");
                    Debug.Log($"  Dodge chance: {aiDecision.NextFloat():P}");
                    break;
            }
        }

        /// <summary>
        /// Detailed analysis of different random permutations.
        /// </summary>
        private void DetailedPermutationAnalysis()
        {
            Debug.Log("\n=== DETAILED PERMUTATION ANALYSIS ===");

            _rngContext = new RngContext();

            var damageCalculation = _rngContext.GetOrCreateUser("DamageCalc", seed: 200);

            // Simulate a base state
            damageCalculation.NextInt(1, 100);

            // Create multiple variation points
            var variations = new List<RngSnapshot>();
            for (int i = 0; i < 4; i++)
            {
                variations.Add(_rngContext.CreateSnapshot($"variation_{i}"));
                damageCalculation.NextInt(1, 20);
            }

            Debug.Log($"Created {variations.Count} variation snapshots");

            // Simulate outcomes from each variation
            var comparison = _rngContext.ComparePermutations(variations, snapshot =>
            {
                var roll = _rngContext.Users.Values.First();
                for (int i = 0; i < 3; i++)
                {
                    roll.NextInt(1, 20, $"outcome_roll_{i}");
                }
            });

            Debug.Log("\n--- Permutation Comparison Summary ---");
            Debug.Log(comparison.GetComparisonSummary());

            // Analyze consistency
            bool allConsistent = comparison.AreAllResultsConsistent();
            float avgVariance = comparison.GetAverageVariance();

            Debug.Log($"All results consistent: {allConsistent}");
            Debug.Log($"Average variance: {avgVariance:P}");

            // Per-user analysis
            foreach (var userId in comparison.Branches[0].Results.Keys)
            {
                var uniqueValues = comparison.GetUniqueResultsForUser(userId);
                var userVariance = comparison.GetVarianceForUser(userId);
                Debug.Log($"User {userId}: {uniqueValues.Count} unique values, variance {userVariance:P}");
            }
        }

        /// <summary>
        /// Example of implementing a custom RNG source for specific use cases.
        /// </summary>
        public class WeightedRng : IRngSource
        {
            /// <summary>
            /// An RNG source that biases toward specific values.
            /// Useful for testing difficulty levels or weighted random tables.
            /// </summary>
            private XorshiftRng _baseRng;
            private float _bias;  // 0 = normal, 0.5 = strongly biased toward 0.5

            public uint Seed => _baseRng.Seed;

            public WeightedRng(uint seed, float bias = 0)
            {
                _baseRng = new XorshiftRng(seed);
                _bias = Mathf.Clamp01(bias);
            }

            public void SetSeed(uint seed)
            {
                _baseRng.SetSeed(seed);
            }

            public uint Next()
            {
                return _baseRng.Next();
            }

            public float NextFloat()
            {
                float val1 = _baseRng.NextFloat();
                float val2 = _baseRng.NextFloat();

                // Blend toward 0.5 based on bias
                return Mathf.Lerp(val1, 0.5f, _bias) +
                       Mathf.Lerp(val2, 0.5f, _bias);
            }

            public double NextDouble()
            {
                return NextFloat();
            }

            public int NextInt(int maxValue)
            {
                return (int)(NextFloat() * maxValue);
            }

            public int NextInt(int minValue, int maxValue)
            {
                return minValue + NextInt(maxValue - minValue);
            }

            public IRngSource Clone()
            {
                return new WeightedRng(Seed, _bias);
            }
        }
    }
}
