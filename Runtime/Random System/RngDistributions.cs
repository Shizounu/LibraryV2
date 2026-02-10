using System;
using System.Collections.Generic;

namespace Shizounu.Library.RandomSystem
{
    /// <summary>
    /// Utility class for generating common distributions.
    /// </summary>
    public static class RngDistributions
    {
        /// <summary>
        /// Generates a value from a normal distribution (bell curve) using Box-Muller transform.
        /// </summary>
        public static float NextGaussian(RngUser rng, float mean = 0, float stdDev = 1,
                                         string label = "gaussian")
        {
            float u1 = rng.NextFloat($"{label}_u1");
            float u2 = rng.NextFloat($"{label}_u2");

            // Box-Muller transform
            float z0 = MathF.Sqrt(-2 * MathF.Log(u1)) * MathF.Cos(2 * MathF.PI * u2);
            return mean + stdDev * z0;
        }

        /// <summary>
        /// Generates a value from an exponential distribution.
        /// </summary>
        public static float NextExponential(RngUser rng, float lambda = 1,
                                           string label = "exponential")
        {
            float u = rng.NextFloat(label);
            return -MathF.Log(u) / lambda;
        }

        /// <summary>
        /// Generates a boolean with the specified probability of being true.
        /// </summary>
        public static bool NextBool(RngUser rng, float probability = 0.5f,
                                    string label = "bool")
        {
            return rng.NextFloat(label) < probability;
        }

        /// <summary>
        /// Performs a percentage roll (0-100).
        /// </summary>
        public static float PercentageRoll(RngUser rng, string label = "percentage")
        {
            return rng.NextFloat(label) * 100f;
        }

        /// <summary>
        /// Simulates rolling N dice with specified sides.
        /// </summary>
        public static int DiceRoll(RngUser rng, int count, int sides,
                                   string label = "dice_roll")
        {
            int total = 0;
            for (int i = 0; i < count; i++)
            {
                total += rng.NextInt(1, sides + 1, $"{label}_d{sides}_{i}");
            }
            return total;
        }

        /// <summary>
        /// Selects a random element from a collection.
        /// </summary>
        public static T PickRandom<T>(RngUser rng, IList<T> collection,
                                      string label = "pick_random")
        {
            if (collection == null || collection.Count == 0)
                throw new ArgumentException("Collection must not be empty");

            int index = rng.NextInt(collection.Count, label);
            return collection[index];
        }

        /// <summary>
        /// Shuffles a list in-place using Fisher-Yates shuffle.
        /// </summary>
        public static void Shuffle<T>(RngUser rng, IList<T> list, string label = "shuffle")
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int randomIndex = rng.NextInt(i + 1, $"{label}_{i}");

                // Swap
                T temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

        /// <summary>
        /// Generates random points on a sphere surface (unit sphere).
        /// </summary>
        public static (float x, float y, float z) RandomPointOnSphere(RngUser rng,
                                                                      string label = "sphere_point")
        {
            float theta = rng.NextFloat($"{label}_theta") * 2 * MathF.PI;
            float phi = MathF.Acos(2 * rng.NextFloat($"{label}_phi") - 1);

            float x = MathF.Sin(phi) * MathF.Cos(theta);
            float y = MathF.Sin(phi) * MathF.Sin(theta);
            float z = MathF.Cos(phi);

            return (x, y, z);
        }

        /// <summary>
        /// Generates a random point inside a circle (unit circle).
        /// </summary>
        public static (float x, float y) RandomPointInCircle(RngUser rng,
                                                            string label = "circle_point")
        {
            float angle = rng.NextFloat($"{label}_angle") * 2 * MathF.PI;
            float radius = MathF.Sqrt(rng.NextFloat($"{label}_radius"));

            float x = radius * MathF.Cos(angle);
            float y = radius * MathF.Sin(angle);

            return (x, y);
        }
    }
}
