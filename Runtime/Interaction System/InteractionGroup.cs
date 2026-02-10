using UnityEngine;
using System.Collections.Generic;

namespace Shizounu.Library.Interaction
{
    /// <summary>
    /// Manages a group of related interactables
    /// </summary>
    public class InteractionGroup : MonoBehaviour
    {
        [System.Serializable]
        public enum GroupBehavior
        {
            Independent,        // All can be interacted with independently
            Sequential,         // Must be interacted with in order
            SingleUse,         // Only one can be used, then all become disabled
            AllOrNothing,      // All must be interacted with before any effect occurs
            ExclusiveToggle    // Only one can be active at a time
        }

        [Header("Group Settings")]
        [SerializeField] private List<InteractableBase> interactables = new List<InteractableBase>();
        [SerializeField] private GroupBehavior behavior = GroupBehavior.Independent;
        [SerializeField] private bool resetOnComplete = false;

        [Header("Events")]
        public UnityEngine.Events.UnityEvent OnGroupCompleted;
        public UnityEngine.Events.UnityEvent OnGroupReset;

        private HashSet<InteractableBase> activatedInteractables = new HashSet<InteractableBase>();
        private int currentSequentialIndex = 0;
        private bool isComplete = false;

        public int ActivatedCount => activatedInteractables.Count;
        public int TotalCount => interactables.Count;
        public bool IsComplete => isComplete;
        public float Progress => TotalCount > 0 ? (float)ActivatedCount / TotalCount : 0f;

        private void Start()
        {
            InitializeGroup();
        }

        private void InitializeGroup()
        {
            foreach (var interactable in interactables)
            {
                if (interactable != null)
                {
                    interactable.OnInteracted.AddListener(OnInteractableUsed);
                }
            }

            ApplyBehavior();
        }

        private void ApplyBehavior()
        {
            switch (behavior)
            {
                case GroupBehavior.Sequential:
                    // Disable all except first
                    for (int i = 0; i < interactables.Count; i++)
                    {
                        if (interactables[i] != null)
                        {
                            interactables[i].SetInteractable(i == 0);
                        }
                    }
                    break;

                case GroupBehavior.ExclusiveToggle:
                    // All enabled, but we'll manage their states
                    foreach (var interactable in interactables)
                    {
                        if (interactable != null)
                        {
                            interactable.SetInteractable(true);
                        }
                    }
                    break;

                default:
                    // Enable all by default
                    foreach (var interactable in interactables)
                    {
                        if (interactable != null)
                        {
                            interactable.SetInteractable(true);
                        }
                    }
                    break;
            }
        }

        private void OnInteractableUsed(GameObject interactor)
        {
            // Find which interactable was used
            InteractableBase used = null;
            foreach (var interactable in interactables)
            {
                if (interactable != null && interactable.gameObject == interactor)
                {
                    used = interactable;
                    break;
                }
            }

            if (used != null)
            {
                HandleInteraction(used);
            }
        }

        private void HandleInteraction(InteractableBase interactable)
        {
            if (isComplete)
                return;

            activatedInteractables.Add(interactable);

            switch (behavior)
            {
                case GroupBehavior.Sequential:
                    HandleSequential(interactable);
                    break;

                case GroupBehavior.SingleUse:
                    HandleSingleUse(interactable);
                    break;

                case GroupBehavior.AllOrNothing:
                    HandleAllOrNothing();
                    break;

                case GroupBehavior.ExclusiveToggle:
                    HandleExclusiveToggle(interactable);
                    break;

                case GroupBehavior.Independent:
                default:
                    HandleIndependent();
                    break;
            }
        }

        private void HandleSequential(InteractableBase interactable)
        {
            int index = interactables.IndexOf(interactable);
            if (index == currentSequentialIndex)
            {
                currentSequentialIndex++;

                // Enable next
                if (currentSequentialIndex < interactables.Count)
                {
                    interactables[currentSequentialIndex].SetInteractable(true);
                }
                else
                {
                    CompleteGroup();
                }
            }
        }

        private void HandleSingleUse(InteractableBase interactable)
        {
            // Disable all others
            foreach (var i in interactables)
            {
                if (i != null && i != interactable)
                {
                    i.SetInteractable(false);
                }
            }
            CompleteGroup();
        }

        private void HandleAllOrNothing()
        {
            if (activatedInteractables.Count >= interactables.Count)
            {
                CompleteGroup();
            }
        }

        private void HandleExclusiveToggle(InteractableBase interactable)
        {
            // This is handled per-interactable basis
            // Could disable others if desired
        }

        private void HandleIndependent()
        {
            if (activatedInteractables.Count >= interactables.Count)
            {
                CompleteGroup();
            }
        }

        private void CompleteGroup()
        {
            isComplete = true;
            OnGroupCompleted?.Invoke();

            if (resetOnComplete)
            {
                ResetGroup();
            }
        }

        /// <summary>
        /// Reset the group to initial state
        /// </summary>
        public void ResetGroup()
        {
            activatedInteractables.Clear();
            currentSequentialIndex = 0;
            isComplete = false;

            ApplyBehavior();
            OnGroupReset?.Invoke();
        }

        /// <summary>
        /// Add an interactable to the group
        /// </summary>
        public void AddInteractable(InteractableBase interactable)
        {
            if (interactable != null && !interactables.Contains(interactable))
            {
                interactables.Add(interactable);
                interactable.OnInteracted.AddListener(OnInteractableUsed);
            }
        }

        /// <summary>
        /// Remove an interactable from the group
        /// </summary>
        public void RemoveInteractable(InteractableBase interactable)
        {
            if (interactables.Contains(interactable))
            {
                interactables.Remove(interactable);
                if (interactable != null)
                {
                    interactable.OnInteracted.RemoveListener(OnInteractableUsed);
                }
            }
        }

        /// <summary>
        /// Enable all interactables in the group
        /// </summary>
        public void EnableAll()
        {
            foreach (var interactable in interactables)
            {
                if (interactable != null)
                {
                    interactable.SetInteractable(true);
                }
            }
        }

        /// <summary>
        /// Disable all interactables in the group
        /// </summary>
        public void DisableAll()
        {
            foreach (var interactable in interactables)
            {
                if (interactable != null)
                {
                    interactable.SetInteractable(false);
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var interactable in interactables)
            {
                if (interactable != null)
                {
                    interactable.OnInteracted.RemoveListener(OnInteractableUsed);
                }
            }
        }
    }
}
