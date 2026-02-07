using UnityEngine;
using System.Collections.Generic;
using Shizounu.Library.GameAI.InfluenceMaps;

namespace Shizounu.Library.GameAI.CoverSystem {
    /// <summary>
    /// Manages cover points and evaluates their suitability for protection.
    /// Uses influence maps to determine threat levels and cover quality.
    /// </summary>
    public class CoverEvaluator
    {
        private LayerMask obstacleLayer;
        private float maxRaycastDistance = 100f;

        public CoverEvaluator(LayerMask obstacleLayer, float maxRaycastDistance = 100f)
        {
            this.obstacleLayer = obstacleLayer;
            this.maxRaycastDistance = maxRaycastDistance;
        }

        /// <summary>
        /// Check if a cover point provides line of sight protection from a threat.
        /// </summary>
        public bool IsProtectedFromThreat(Vector3 coverPosition, Vector3 threatPosition, float coverHeight = 1.8f)
        {
            Vector3 coverEyePos = coverPosition + Vector3.up * (coverHeight * 0.5f);
            Vector3 threatEyePos = threatPosition + Vector3.up * (coverHeight * 0.5f);

            Vector3 dirToThreat = (threatEyePos - coverEyePos).normalized;
            float distToThreat = Vector3.Distance(coverEyePos, threatEyePos);

            // Check if there's an obstacle between cover and threat
            return Physics.Raycast(coverEyePos, dirToThreat, distToThreat, obstacleLayer);
        }

        /// <summary>
        /// Evaluate cover quality based on multiple threats.
        /// </summary>
        public float EvaluateCoverQuality(Vector3 coverPosition, List<Vector3> threatPositions, float coverHeight = 1.8f)
        {
            if (threatPositions.Count == 0)
                return 0f;

            float protectionScore = 0f;

            foreach (var threatPos in threatPositions)
            {
                if (IsProtectedFromThreat(coverPosition, threatPos, coverHeight))
                {
                    protectionScore += 1f;
                }
            }

            // Score is percentage of threats protected from
            return protectionScore / threatPositions.Count;
        }

        /// <summary>
        /// Evaluate cover quality using an influence map (threats contribute negative influence).
        /// </summary>
        public float EvaluateCoverWithInfluenceMap(Vector3 coverPosition, InfluenceMap influenceMap)
        {
            float influence = influenceMap.GetInfluence(coverPosition);
            // Higher (less negative) influence = better cover
            return -influence; // Invert so positive is better
        }
    }
}
