using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Shizounu.Library.Interaction
{
    /// <summary>
    /// Extension methods and utilities for the interaction system
    /// </summary>
    public static class InteractionExtensions
    {
        /// <summary>
        /// Get all interactables on a GameObject and its children
        /// </summary>
        public static IInteractable[] GetInteractables(this GameObject gameObject, bool includeInactive = false)
        {
            var components = gameObject.GetComponentsInChildren<MonoBehaviour>(includeInactive);
            return components.OfType<IInteractable>().ToArray();
        }

        /// <summary>
        /// Get the first interactable on a GameObject
        /// </summary>
        public static IInteractable GetInteractable(this GameObject gameObject)
        {
            var components = gameObject.GetComponents<MonoBehaviour>();
            return components.OfType<IInteractable>().FirstOrDefault();
        }

        /// <summary>
        /// Check if a GameObject has any interactables
        /// </summary>
        public static bool HasInteractable(this GameObject gameObject)
        {
            return gameObject.GetInteractable() != null;
        }

        /// <summary>
        /// Enable all interactables on a GameObject
        /// </summary>
        public static void EnableInteractables(this GameObject gameObject)
        {
            var interactables = gameObject.GetInteractables();
            foreach (var interactable in interactables)
            {
                if (interactable is InteractableBase interactableBase)
                {
                    interactableBase.SetInteractable(true);
                }
            }
        }

        /// <summary>
        /// Disable all interactables on a GameObject
        /// </summary>
        public static void DisableInteractables(this GameObject gameObject)
        {
            var interactables = gameObject.GetInteractables();
            foreach (var interactable in interactables)
            {
                if (interactable is InteractableBase interactableBase)
                {
                    interactableBase.SetInteractable(false);
                }
            }
        }

        /// <summary>
        /// Find all interactables within a radius
        /// </summary>
        public static IInteractable[] FindInteractablesInRadius(Vector3 center, float radius, LayerMask layerMask)
        {
            var colliders = Physics.OverlapSphere(center, radius, layerMask);
            List<IInteractable> interactables = new List<IInteractable>();

            foreach (var collider in colliders)
            {
                var interactable = collider.GetComponent<IInteractable>();
                if (interactable != null && interactable.CanInteract)
                {
                    interactables.Add(interactable);
                }
            }

            return interactables.ToArray();
        }

        /// <summary>
        /// Get the closest interactable to a point
        /// </summary>
        public static IInteractable GetClosestInteractable(Vector3 point, IInteractable[] interactables)
        {
            if (interactables == null || interactables.Length == 0)
                return null;

            return interactables
                .Where(i => i != null && i.CanInteract)
                .OrderBy(i => Vector3.Distance(point, i.Transform.position))
                .FirstOrDefault();
        }

        /// <summary>
        /// Get interactables sorted by priority and distance
        /// </summary>
        public static IInteractable[] SortByPriorityAndDistance(Vector3 point, IInteractable[] interactables)
        {
            if (interactables == null || interactables.Length == 0)
                return new IInteractable[0];

            return interactables
                .Where(i => i != null && i.CanInteract)
                .OrderByDescending(i => i.InteractionPriority)
                .ThenBy(i => Vector3.Distance(point, i.Transform.position))
                .ToArray();
        }
    }

    /// <summary>
    /// Global interaction event manager for cross-component communication
    /// </summary>
    public class InteractionEventManager : MonoBehaviour
    {
        private static InteractionEventManager instance;
        public static InteractionEventManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("InteractionEventManager");
                    instance = go.AddComponent<InteractionEventManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        public delegate void InteractionDelegate(InteractionData data);
        public static event InteractionDelegate OnAnyInteractionStarted;
        public static event InteractionDelegate OnAnyInteractionCompleted;

        /// <summary>
        /// Broadcast that an interaction has started
        /// </summary>
        public static void BroadcastInteractionStarted(InteractionData data)
        {
            OnAnyInteractionStarted?.Invoke(data);
        }

        /// <summary>
        /// Broadcast that an interaction has completed
        /// </summary>
        public static void BroadcastInteractionCompleted(InteractionData data)
        {
            OnAnyInteractionCompleted?.Invoke(data);
        }
    }

    /// <summary>
    /// Utility for filtering interactables based on conditions
    /// </summary>
    public static class InteractionFilters
    {
        /// <summary>
        /// Filter by tag
        /// </summary>
        public static IInteractable[] FilterByTag(IInteractable[] interactables, string tag)
        {
            return interactables.Where(i => i != null && i.Transform.CompareTag(tag)).ToArray();
        }

        /// <summary>
        /// Filter by layer
        /// </summary>
        public static IInteractable[] FilterByLayer(IInteractable[] interactables, LayerMask layerMask)
        {
            return interactables.Where(i => i != null && ((1 << i.Transform.gameObject.layer) & layerMask) != 0).ToArray();
        }

        /// <summary>
        /// Filter by name pattern
        /// </summary>
        public static IInteractable[] FilterByName(IInteractable[] interactables, string namePattern)
        {
            return interactables.Where(i => i != null && i.Transform.name.Contains(namePattern)).ToArray();
        }

        /// <summary>
        /// Filter by distance
        /// </summary>
        public static IInteractable[] FilterByDistance(IInteractable[] interactables, Vector3 point, float maxDistance)
        {
            return interactables.Where(i => i != null && 
                Vector3.Distance(point, i.Transform.position) <= maxDistance).ToArray();
        }

        /// <summary>
        /// Filter by type
        /// </summary>
        public static T[] FilterByType<T>(IInteractable[] interactables) where T : class, IInteractable
        {
            return interactables.OfType<T>().ToArray();
        }

        /// <summary>
        /// Filter by priority threshold
        /// </summary>
        public static IInteractable[] FilterByPriority(IInteractable[] interactables, int minPriority)
        {
            return interactables.Where(i => i != null && i.InteractionPriority >= minPriority).ToArray();
        }
    }
}
