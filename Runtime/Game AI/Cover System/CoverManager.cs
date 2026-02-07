using UnityEngine;
using Shizounu.Library.GameAI.InfluenceMaps;
using System.Collections.Generic;
using System.Linq;

namespace Shizounu.Library.GameAI.CoverSystem
{
    public class CoverManager : MonoBehaviour
    {
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private float maxRaycastDistance = 100f;
        [SerializeField] private float coverHeight = 1.8f;

        private List<CoverPoint> coverPoints = new List<CoverPoint>();
        private CoverEvaluator evaluator;
        private InfluenceMapManager influenceMapManager;

        private void Awake()
        {
            evaluator = new CoverEvaluator(obstacleLayer, maxRaycastDistance);
            influenceMapManager = GetComponent<InfluenceMapManager>();
        }

        /// <summary>
        /// Register a cover point in the system.
        /// </summary>
        public void RegisterCoverPoint(Vector3 position, float height = 1.8f, Collider collider = null, float protectionValue = 1f)
        {
            var point = new CoverPoint(position, height, collider, protectionValue);
            coverPoints.Add(point);
        }

        /// <summary>
        /// Register a cover point component.
        /// </summary>
        public void RegisterCoverPoint(CoverPointComponent component)
        {
            RegisterCoverPoint(component.transform.position, component.CoverHeight, component.GetComponent<Collider>(), component.ProtectionValue);
            ApplyCoverBlocking(component);
        }

        private void ApplyCoverBlocking(CoverPointComponent component)
        {
            if (influenceMapManager == null || !component.BlocksInfluence)
                return;

            influenceMapManager.Map.SetBlockedBounds(component.GetCoverBounds(), true);
        }

        /// <summary>
        /// Find the best cover point for protection from threats.
        /// </summary>
        public CoverPoint FindBestCover(Vector3 agentPosition, List<Vector3> threatPositions)
        {
            if (coverPoints.Count == 0)
                return null;

            CoverPoint bestCover = null;
            float bestScore = -1f;

            foreach (var cover in coverPoints)
            {
                float distanceToAgent = Vector3.Distance(agentPosition, cover.Position);
                float quality = evaluator.EvaluateCoverQuality(cover.Position, threatPositions, cover.CoverHeight);
                
                // Combine quality with distance (prefer closer cover)
                float score = quality - (distanceToAgent * 0.01f);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestCover = cover;
                }
            }

            return bestCover;
        }

        /// <summary>
        /// Find best cover using influence map (threat positions).
        /// </summary>
        public CoverPoint FindBestCoverUsingInfluence(Vector3 agentPosition)
        {
            if (coverPoints.Count == 0 || influenceMapManager == null)
                return null;

            CoverPoint bestCover = null;
            float bestScore = -1f;

            foreach (var cover in coverPoints)
            {
                float distanceToAgent = Vector3.Distance(agentPosition, cover.Position);
                float quality = evaluator.EvaluateCoverWithInfluenceMap(cover.Position, influenceMapManager.Map);
                
                // Combine quality with distance
                float score = quality - (distanceToAgent * 0.01f);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestCover = cover;
                }
            }

            return bestCover;
        }

        /// <summary>
        /// Find all cover points that protect from a threat.
        /// </summary>
        public List<CoverPoint> FindProtectedCover(Vector3 agentPosition, Vector3 threatPosition)
        {
            var protectedCovers = new List<CoverPoint>();

            foreach (var cover in coverPoints)
            {
                if (evaluator.IsProtectedFromThreat(cover.Position, threatPosition, cover.CoverHeight))
                {
                    protectedCovers.Add(cover);
                }
            }

            return protectedCovers.OrderBy(c => Vector3.Distance(agentPosition, c.Position)).ToList();
        }

        /// <summary>
        /// Get all registered cover points.
        /// </summary>
        public List<CoverPoint> GetAllCoverPoints()
        {
            return new List<CoverPoint>(coverPoints);
        }

        /// <summary>
        /// Clear all cover points.
        /// </summary>
        public void ClearCoverPoints()
        {
            coverPoints.Clear();
        }

        /// <summary>
        /// Remove a cover point.
        /// </summary>
        public void RemoveCoverPoint(CoverPoint point)
        {
            coverPoints.Remove(point);
        }
    }
}
