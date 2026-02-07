using UnityEngine;

namespace Shizounu.Library.GameAI.InfluenceMaps.Examples
{
    /// <summary>
    /// Example showing basic influence map usage.
    /// </summary>
    public class BasicInfluenceMapExample : MonoBehaviour
    {
        private InfluenceMap map;

        private void Start()
        {
            // Create a 50x50 grid with 1 unit cells starting at world origin
            map = new InfluenceMap(50, 50, 1f, Vector3.zero);

            // Add some influence at specific positions
            map.AddInfluence(new Vector3(10, 0, 10), 5f, 3f);  // Positive influence with radius
            map.AddInfluence(new Vector3(30, 0, 30), -3f, 2f); // Negative influence

            // Find best and worst positions
            Vector3 bestPosition = map.FindMaxInfluence();
            Vector3 worstPosition = map.FindMinInfluence();

            Debug.Log($"Best position: {bestPosition}");
            Debug.Log($"Worst position: {worstPosition}");
        }

        private void Update()
        {
            // Apply decay over time
            map.ApplyDecay(0.1f, Time.deltaTime);

            // Optionally propagate influence to neighbors
            if (Input.GetKey(KeyCode.Space))
            {
                map.Propagate(0.1f, true);
            }
        }
    }
}
