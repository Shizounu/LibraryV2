using UnityEngine;

namespace Shizounu.Library.Interaction
{
    /// <summary>
    /// Simple button-like interactable that triggers an action when interacted with
    /// </summary>
    public class SimpleInteractable : InteractableBase
    {
        [Header("Simple Interactable Settings")]
        [SerializeField] protected bool oneTimeUse = false;
        [SerializeField] protected float cooldownTime = 0f;

        private bool hasBeenUsed = false;
        private float lastInteractionTime = -Mathf.Infinity;

        public override bool CanInteract
        {
            get
            {
                if (!base.CanInteract)
                    return false;

                if (oneTimeUse && hasBeenUsed)
                    return false;

                if (Time.time - lastInteractionTime < cooldownTime)
                    return false;

                return true;
            }
        }

        protected override void OnInteract(GameObject interactor)
        {
            lastInteractionTime = Time.time;
            hasBeenUsed = true;
        }

        /// <summary>
        /// Reset the interactable to be usable again (for one-time use)
        /// </summary>
        public void Reset()
        {
            hasBeenUsed = false;
            lastInteractionTime = -Mathf.Infinity;
        }
    }
}
