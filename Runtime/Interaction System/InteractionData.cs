using UnityEngine;

namespace Shizounu.Library.Interaction
{
    /// <summary>
    /// Data structure containing information about an interaction event
    /// </summary>
    public struct InteractionData
    {
        public IInteractable Interactable;
        public GameObject Interactor;
        public Vector3 InteractionPoint;
        public float Distance;
        public float Timestamp;

        public InteractionData(IInteractable interactable, GameObject interactor, Vector3 point, float distance)
        {
            Interactable = interactable;
            Interactor = interactor;
            InteractionPoint = point;
            Distance = distance;
            Timestamp = Time.time;
        }
    }

    /// <summary>
    /// Settings for interaction detection
    /// </summary>
    [System.Serializable]
    public class InteractionSettings
    {
        [Tooltip("Maximum distance for interactions")]
        public float MaxInteractionDistance = 3f;

        [Tooltip("Angle from forward direction to detect interactions (in degrees)")]
        [Range(0f, 180f)]
        public float InteractionAngle = 45f;

        [Tooltip("Layer mask for interactable objects")]
        public LayerMask InteractableLayerMask = -1;

        [Tooltip("How often to check for interactables (in seconds)")]
        [Range(0.01f, 1f)]
        public float CheckInterval = 0.1f;

        [Tooltip("Use sphere cast instead of raycast for detection")]
        public bool UseSphereCast = false;

        [Tooltip("Radius for sphere cast (if enabled)")]
        [Range(0.1f, 2f)]
        public float SphereCastRadius = 0.5f;
    }
}
