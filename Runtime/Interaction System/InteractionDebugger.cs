using UnityEngine;
using System.Collections.Generic;

namespace Shizounu.Library.Interaction
{
    /// <summary>
    /// Utility for debugging and visualizing interactions in the scene
    /// </summary>
    public class InteractionDebugger : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool showDetectionRange = true;
        [SerializeField] private bool showFocusedInteractable = true;
        [SerializeField] private bool showAllInteractables = false;
        [SerializeField] private bool logInteractions = true;
        [SerializeField] private Color rangeColor = Color.yellow;
        [SerializeField] private Color focusedColor = Color.green;
        [SerializeField] private Color interactableColor = Color.cyan;

        private InteractionController controller;
        private IInteractable[] allInteractables;

        private void Start()
        {
            controller = GetComponent<InteractionController>();
            
            if (controller != null)
            {
                controller.OnInteractionStarted.AddListener(OnInteractionStarted);
                controller.OnInteractionCompleted.AddListener(OnInteractionCompleted);
                controller.OnInteractableFocused.AddListener(OnInteractableFocused);
                controller.OnInteractableUnfocused.AddListener(OnInteractableUnfocused);
            }

            RefreshInteractablesList();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                RefreshInteractablesList();
                Debug.Log($"Found {allInteractables.Length} interactables in scene");
            }
        }

        private void OnInteractionStarted(InteractionData data)
        {
            if (logInteractions)
            {
                Debug.Log($"[Interaction] Started: {data.Interactable.InteractionPrompt} at distance {data.Distance:F2}m");
            }
        }

        private void OnInteractionCompleted(InteractionData data)
        {
            if (logInteractions)
            {
                Debug.Log($"[Interaction] Completed: {data.Interactable.InteractionPrompt}");
            }
        }

        private void OnInteractableFocused(IInteractable interactable)
        {
            if (logInteractions)
            {
                Debug.Log($"[Interaction] Focused: {interactable.InteractionPrompt}");
            }
        }

        private void OnInteractableUnfocused(IInteractable interactable)
        {
            if (logInteractions)
            {
                Debug.Log($"[Interaction] Unfocused: {interactable.InteractionPrompt}");
            }
        }

        private void RefreshInteractablesList()
        {
            var interactableBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            List<IInteractable> list = new List<IInteractable>();

            foreach (var behaviour in interactableBehaviours)
            {
                if (behaviour is IInteractable interactable)
                {
                    list.Add(interactable);
                }
            }

            allInteractables = list.ToArray();
        }

        private void OnDrawGizmos()
        {
            if (controller == null)
                return;

            // Draw detection range
            if (showDetectionRange)
            {
                Gizmos.color = rangeColor;
                Transform origin = controller.transform;
                Vector3 direction = origin.forward * controller.Settings.MaxInteractionDistance;
                Gizmos.DrawRay(origin.position, direction);

                if (controller.Settings.UseSphereCast)
                {
                    Gizmos.DrawWireSphere(origin.position + direction, controller.Settings.SphereCastRadius);
                }
            }

            // Draw focused interactable
            if (showFocusedInteractable && controller.CurrentFocusedInteractable != null)
            {
                Gizmos.color = focusedColor;
                Vector3 pos = controller.CurrentFocusedInteractable.Transform.position;
                Gizmos.DrawWireSphere(pos, 0.3f);
                Gizmos.DrawLine(controller.transform.position, pos);
            }

            // Draw all interactables
            if (showAllInteractables && allInteractables != null)
            {
                Gizmos.color = interactableColor;
                foreach (var interactable in allInteractables)
                {
                    if (interactable != null && interactable.Transform != null)
                    {
                        Gizmos.DrawWireCube(interactable.Transform.position, Vector3.one * 0.2f);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (controller != null)
            {
                controller.OnInteractionStarted.RemoveListener(OnInteractionStarted);
                controller.OnInteractionCompleted.RemoveListener(OnInteractionCompleted);
                controller.OnInteractableFocused.RemoveListener(OnInteractableFocused);
                controller.OnInteractableUnfocused.RemoveListener(OnInteractableUnfocused);
            }
        }

        /// <summary>
        /// Log all interactables in range
        /// </summary>
        [ContextMenu("Log Interactables In Range")]
        public void LogInteractablesInRange()
        {
            if (controller == null)
                return;

            Debug.Log("=== Interactables in Range ===");
            
            // Force a detection
            controller.DetectInteractables();
            
            if (controller.CurrentFocusedInteractable != null)
            {
                Debug.Log($"Focused: {controller.CurrentFocusedInteractable.InteractionPrompt} " +
                         $"(Priority: {controller.CurrentFocusedInteractable.InteractionPriority})");
            }
            else
            {
                Debug.Log("No interactables in range");
            }
        }

        /// <summary>
        /// Log all interactables in scene
        /// </summary>
        [ContextMenu("Log All Interactables")]
        public void LogAllInteractables()
        {
            RefreshInteractablesList();
            Debug.Log($"=== All Interactables ({allInteractables.Length}) ===");
            
            foreach (var interactable in allInteractables)
            {
                string canInteract = interactable.CanInteract ? "✓" : "✗";
                Debug.Log($"{canInteract} {interactable.InteractionPrompt} " +
                         $"(Priority: {interactable.InteractionPriority}) - {interactable.Transform.name}");
            }
        }
    }
}
