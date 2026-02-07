using UnityEngine;
using Shizounu.Library.GameAI.CoverSystem;

namespace Shizounu.Library.GameAI.CoverSystem
{
    /// <summary>
    /// Component for marking objects in the world as cover points.
    /// Automatically registers with the CoverManager.
    /// </summary>
    public class CoverPointComponent : MonoBehaviour
    {
        [SerializeField] private float coverHeight = 1.8f;
        [SerializeField] private float protectionValue = 1f;
        [SerializeField] private bool blocksInfluence = true;
        [SerializeField] private CoverManager coverManager;

        public float CoverHeight => coverHeight;
        public float ProtectionValue => protectionValue;
        public bool BlocksInfluence => blocksInfluence;

        private void Start()
        {
            if (coverManager == null)
            {
                coverManager = FindFirstObjectByType<CoverManager>();
            }

            if (coverManager != null)
            {
                coverManager.RegisterCoverPoint(this);
            }
            else
            {
                Debug.LogWarning($"No CoverManager found for {gameObject.name}", gameObject);
            }
        }

        /// <summary>
        /// Get the cover position (center of the collider or transform position).
        /// </summary>
        public Vector3 GetCoverPosition()
        {
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                return collider.bounds.center;
            }
            return transform.position;
        }

        /// <summary>
        /// Get cover bounds for influence blocking.
        /// </summary>
        public Bounds GetCoverBounds()
        {
            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                return collider.bounds;
            }

            return new Bounds(transform.position, Vector3.one * 0.5f);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(GetCoverPosition(), Vector3.one * 0.5f);
            Gizmos.DrawLine(GetCoverPosition(), GetCoverPosition() + Vector3.up * coverHeight);
        }
    }
}
