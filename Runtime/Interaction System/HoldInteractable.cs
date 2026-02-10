using UnityEngine;

namespace Shizounu.Library.Interaction
{
    /// <summary>
    /// Interactable that can be held for a duration before triggering
    /// </summary>
    public class HoldInteractable : InteractableBase
    {
        [Header("Hold Settings")]
        [SerializeField] private float holdDuration = 2f;
        [SerializeField] private bool canCancelHold = true;
        [SerializeField] private bool resetOnCancel = true;

        private float currentHoldTime = 0f;
        private bool isHolding = false;
        private GameObject currentInteractor;

        public float HoldProgress => Mathf.Clamp01(currentHoldTime / holdDuration);
        public bool IsHolding => isHolding;

        protected override void OnInteract(GameObject interactor)
        {
            if (!isHolding)
            {
                StartHold(interactor);
            }
        }

        private void Update()
        {
            if (isHolding)
            {
                currentHoldTime += Time.deltaTime;

                if (currentHoldTime >= holdDuration)
                {
                    CompleteHold();
                }
            }
        }

        private void StartHold(GameObject interactor)
        {
            isHolding = true;
            currentInteractor = interactor;
            currentHoldTime = 0f;
        }

        private void CompleteHold()
        {
            isHolding = false;
            OnHoldComplete(currentInteractor);
            currentHoldTime = 0f;
            currentInteractor = null;
        }

        /// <summary>
        /// Cancel the current hold interaction
        /// </summary>
        public void CancelHold()
        {
            if (canCancelHold && isHolding)
            {
                isHolding = false;
                if (resetOnCancel)
                {
                    currentHoldTime = 0f;
                }
                currentInteractor = null;
            }
        }

        /// <summary>
        /// Override this to implement what happens when hold is completed
        /// </summary>
        protected virtual void OnHoldComplete(GameObject interactor)
        {
            // Base implementation does nothing
        }

        public override void OnUnfocused()
        {
            base.OnUnfocused();
            if (canCancelHold)
            {
                CancelHold();
            }
        }
    }
}
