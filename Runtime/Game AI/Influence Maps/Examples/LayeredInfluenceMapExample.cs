using UnityEngine;
using System.Collections.Generic;

namespace Shizounu.Library.GameAI.InfluenceMaps.Examples
{
    /// <summary>
    /// Example showing multi-layer influence maps.
    /// </summary>
    public class LayeredInfluenceMapExample : MonoBehaviour
    {
        private InfluenceMap map;

        private void Start()
        {
            map = new InfluenceMap(50, 50, 1f, Vector3.zero);

            // Create separate layers for different types of influence
            map.CreateLayer("enemies");
            map.CreateLayer("allies");
            map.CreateLayer("resources");

            // Add influence to each layer
            map.AddInfluenceToLayer("enemies", new Vector3(10, 0, 10), -5f, 4f);
            map.AddInfluenceToLayer("allies", new Vector3(20, 0, 20), 3f, 3f);
            map.AddInfluenceToLayer("resources", new Vector3(30, 0, 30), 2f, 2f);

            // Combine layers with different weights
            var weights = new Dictionary<string, float>
            {
                { "enemies", 1.5f },   // Enemies matter more
                { "allies", 1.0f },    // Allies matter
                { "resources", 0.5f }  // Resources matter less
            };

            map.CombineLayers(weights);

            // Now the main map has combined influence
            Vector3 safestPosition = map.FindMaxInfluence();
            Debug.Log($"Safest position considering all factors: {safestPosition}");
        }
    }
}
