using UnityEngine;

namespace Shizounu.Library.GameAI.CoverSystem
{
    /// <summary>
    /// Represents a cover point in the world with metadata about its protection value.
    /// </summary>
    public class CoverPoint
    {
        public Vector3 Position { get; set; }
        public float CoverHeight { get; set; }
        public Collider CoverCollider { get; set; }
        public float BaseProtectionValue { get; set; }
        private float cachedQuality;
        private bool qualityDirty = true;

        public CoverPoint(Vector3 position, float height = 1.8f, Collider collider = null, float protection = 1f)
        {
            Position = position;
            CoverHeight = height;
            CoverCollider = collider;
            BaseProtectionValue = protection;
        }

        public float GetCachedQuality()
        {
            return cachedQuality;
        }

        public void SetQuality(float quality)
        {
            cachedQuality = quality;
            qualityDirty = false;
        }

        public void MarkDirty()
        {
            qualityDirty = true;
        }

        public bool IsQualityDirty()
        {
            return qualityDirty;
        }
    }
}
