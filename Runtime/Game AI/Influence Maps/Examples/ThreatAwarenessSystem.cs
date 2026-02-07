using UnityEngine;

namespace Shizounu.Library.GameAI.InfluenceMaps.Examples
{
    /// <summary>
    /// Example of a dynamic threat awareness system.
    /// </summary>
    public class ThreatAwarenessSystem : MonoBehaviour
    {
        [SerializeField] private InfluenceMapManager mapManager;
        [SerializeField] private Transform[] enemies;
        [SerializeField] private float enemyInfluence = -10f;
        [SerializeField] private float enemyRadius = 5f;

        private void Update()
        {
            if (mapManager == null)
                return;

            // Clear previous frame's enemy positions
            mapManager.Map.Clear();

            // Add influence for each enemy
            foreach (var enemy in enemies)
            {
                if (enemy != null)
                {
                    mapManager.Map.AddInfluence(enemy.position, enemyInfluence, enemyRadius);
                }
            }

            // AI can now query the map to find safe positions
            Vector3 safestPosition = mapManager.Map.FindMaxInfluence();
            
            // Draw for visualization
            Debug.DrawLine(transform.position, safestPosition, Color.green);
        }

        /// <summary>
        /// Find a safe position for the AI to move to.
        /// </summary>
        public Vector3 FindSafePosition()
        {
            return mapManager.Map.FindMaxInfluence();
        }

        /// <summary>
        /// Check if a position is safe.
        /// </summary>
        public bool IsPositionSafe(Vector3 position, float threshold = -1f)
        {
            float influence = mapManager.Map.GetInfluence(position);
            return influence > threshold;
        }
    }
}
