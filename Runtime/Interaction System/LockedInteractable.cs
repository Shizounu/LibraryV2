using UnityEngine;

namespace Shizounu.Library.Interaction
{
    /// <summary>
    /// Interactable that requires a specific item or key to interact with
    /// </summary>
    public class LockedInteractable : InteractableBase
    {
        [Header("Lock Settings")]
        [SerializeField] private string requiredKey = "Key";
        [SerializeField] private string lockedPrompt = "Locked";
        [SerializeField] private string unlockedPrompt = "Open";
        [SerializeField] private bool consumeKey = true;
        [SerializeField] private bool relockable = false;

        private bool isUnlocked = false;

        public bool IsUnlocked => isUnlocked;
        public string RequiredKey => requiredKey;

        public override bool CanInteract => base.CanInteract && (isUnlocked || CanUnlock());
        public override string InteractionPrompt => isUnlocked ? unlockedPrompt : lockedPrompt;

        protected override void OnInteract(GameObject interactor)
        {
            if (isUnlocked)
            {
                OnUnlockedInteract(interactor);
            }
            else if (TryUnlock(interactor))
            {
                OnUnlocked(interactor);
            }
        }

        /// <summary>
        /// Check if the interactor has the required key
        /// </summary>
        private bool CanUnlock()
        {
            // This is a simple implementation. In a real project, you'd check an inventory system
            return false; // Override this or use HasKey method
        }

        /// <summary>
        /// Attempt to unlock with the interactor
        /// </summary>
        private bool TryUnlock(GameObject interactor)
        {
            // Check if interactor has the key (implement your inventory check here)
            bool hasKey = HasKey(interactor);

            if (hasKey)
            {
                isUnlocked = true;

                if (consumeKey)
                {
                    RemoveKey(interactor);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Override this to implement key checking logic
        /// </summary>
        protected virtual bool HasKey(GameObject interactor)
        {
            // Default implementation - always returns false
            // Override this in derived classes to implement proper inventory checking
            return false;
        }

        /// <summary>
        /// Override this to implement key removal logic
        /// </summary>
        protected virtual void RemoveKey(GameObject interactor)
        {
            // Default implementation - does nothing
            // Override this in derived classes to remove the key from inventory
        }

        /// <summary>
        /// Called when successfully unlocked
        /// </summary>
        protected virtual void OnUnlocked(GameObject interactor)
        {
            // Override in derived classes for custom behavior
        }

        /// <summary>
        /// Called when interacting with an already unlocked object
        /// </summary>
        protected virtual void OnUnlockedInteract(GameObject interactor)
        {
            // Override in derived classes for custom behavior
        }

        /// <summary>
        /// Manually unlock this interactable
        /// </summary>
        public void Unlock()
        {
            isUnlocked = true;
        }

        /// <summary>
        /// Lock this interactable again
        /// </summary>
        public void Lock()
        {
            if (relockable)
            {
                isUnlocked = false;
            }
        }

        /// <summary>
        /// Change the required key
        /// </summary>
        public void SetRequiredKey(string key)
        {
            requiredKey = key;
        }
    }
}
