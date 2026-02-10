using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

namespace Shizounu.Library.Interaction
{
    /// <summary>
    /// Controls interaction detection and execution for a player or agent
    /// </summary>
    public class InteractionController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private InteractionSettings settings = new InteractionSettings();

        [Header("Detection")]
        [SerializeField] private Transform detectionOrigin;
        [SerializeField] private bool autoDetect = true;

        [Header("Input")]
        [SerializeField] private KeyCode interactionKey = KeyCode.E;
        [SerializeField] private bool useInputSystem = false;

        [Header("Events")]
        public UnityEvent<InteractionData> OnInteractionStarted;
        public UnityEvent<InteractionData> OnInteractionCompleted;
        public UnityEvent<IInteractable> OnInteractableFocused;
        public UnityEvent<IInteractable> OnInteractableUnfocused;

        private IInteractable currentFocusedInteractable;
        private float lastCheckTime;
        private List<IInteractable> detectedInteractables = new List<IInteractable>();

        public IInteractable CurrentFocusedInteractable => currentFocusedInteractable;
        public InteractionSettings Settings => settings;
        public bool HasInteractableInRange => currentFocusedInteractable != null;

        private void Start()
        {
            if (detectionOrigin == null)
                detectionOrigin = transform;
        }

        private void Update()
        {
            if (autoDetect)
            {
                if (Time.time - lastCheckTime >= settings.CheckInterval)
                {
                    DetectInteractables();
                    lastCheckTime = Time.time;
                }
            }

            // Handle input
            if (Input.GetKeyDown(interactionKey) && !useInputSystem)
            {
                TryInteract();
            }
        }

        /// <summary>
        /// Detects interactables in range and updates the focused interactable
        /// </summary>
        public void DetectInteractables()
        {
            detectedInteractables.Clear();
            RaycastHit[] hits;

            if (settings.UseSphereCast)
            {
                hits = Physics.SphereCastAll(
                    detectionOrigin.position,
                    settings.SphereCastRadius,
                    detectionOrigin.forward,
                    settings.MaxInteractionDistance,
                    settings.InteractableLayerMask
                );
            }
            else
            {
                hits = Physics.RaycastAll(
                    detectionOrigin.position,
                    detectionOrigin.forward,
                    settings.MaxInteractionDistance,
                    settings.InteractableLayerMask
                );
            }

            // Collect valid interactables
            foreach (var hit in hits)
            {
                var interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null && interactable.CanInteract)
                {
                    // Check angle constraint
                    Vector3 directionToInteractable = (hit.point - detectionOrigin.position).normalized;
                    float angle = Vector3.Angle(detectionOrigin.forward, directionToInteractable);

                    if (angle <= settings.InteractionAngle)
                    {
                        detectedInteractables.Add(interactable);
                    }
                }
            }

            // Sort by priority and distance, then select the best one
            IInteractable bestInteractable = null;
            if (detectedInteractables.Count > 0)
            {
                bestInteractable = detectedInteractables
                    .OrderByDescending(i => i.InteractionPriority)
                    .ThenBy(i => Vector3.Distance(detectionOrigin.position, i.Transform.position))
                    .First();
            }

            // Update focused interactable
            if (bestInteractable != currentFocusedInteractable)
            {
                if (currentFocusedInteractable != null)
                {
                    currentFocusedInteractable.OnUnfocused();
                    OnInteractableUnfocused?.Invoke(currentFocusedInteractable);
                }

                currentFocusedInteractable = bestInteractable;

                if (currentFocusedInteractable != null)
                {
                    currentFocusedInteractable.OnFocused();
                    OnInteractableFocused?.Invoke(currentFocusedInteractable);
                }
            }
        }

        /// <summary>
        /// Attempts to interact with the currently focused interactable
        /// </summary>
        /// <returns>True if interaction was successful</returns>
        public bool TryInteract()
        {
            if (currentFocusedInteractable != null && currentFocusedInteractable.CanInteract)
            {
                Vector3 interactionPoint = currentFocusedInteractable.Transform.position;
                float distance = Vector3.Distance(detectionOrigin.position, interactionPoint);

                var interactionData = new InteractionData(
                    currentFocusedInteractable,
                    gameObject,
                    interactionPoint,
                    distance
                );

                OnInteractionStarted?.Invoke(interactionData);
                currentFocusedInteractable.Interact(gameObject);
                OnInteractionCompleted?.Invoke(interactionData);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Manually interact with a specific interactable, bypassing detection
        /// </summary>
        public bool InteractWith(IInteractable interactable)
        {
            if (interactable != null && interactable.CanInteract)
            {
                float distance = Vector3.Distance(detectionOrigin.position, interactable.Transform.position);
                var interactionData = new InteractionData(
                    interactable,
                    gameObject,
                    interactable.Transform.position,
                    distance
                );

                OnInteractionStarted?.Invoke(interactionData);
                interactable.Interact(gameObject);
                OnInteractionCompleted?.Invoke(interactionData);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the detection origin transform
        /// </summary>
        public void SetDetectionOrigin(Transform origin)
        {
            detectionOrigin = origin;
        }

        private void OnDrawGizmosSelected()
        {
            if (detectionOrigin == null)
                detectionOrigin = transform;

            // Draw interaction range
            Gizmos.color = Color.yellow;
            Vector3 direction = detectionOrigin.forward * settings.MaxInteractionDistance;
            Gizmos.DrawRay(detectionOrigin.position, direction);

            // Draw sphere at end if using sphere cast
            if (settings.UseSphereCast)
            {
                Gizmos.color = Color.yellow * 0.5f;
                Gizmos.DrawWireSphere(detectionOrigin.position + direction, settings.SphereCastRadius);
            }

            // Draw focused interactable
            if (Application.isPlaying && currentFocusedInteractable != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(currentFocusedInteractable.Transform.position, 0.5f);
                Gizmos.DrawLine(detectionOrigin.position, currentFocusedInteractable.Transform.position);
            }
        }
    }
}
