using UnityEngine;

namespace Shizounu.Library.Interaction
{
    /// <summary>
    /// Interactable that toggles between states (e.g., switches, doors)
    /// </summary>
    public class ToggleInteractable : InteractableBase
    {
        [Header("Toggle Settings")]
        [SerializeField] private bool isOn = false;
        [SerializeField] private string onPrompt = "Turn Off";
        [SerializeField] private string offPrompt = "Turn On";
        [SerializeField] private bool canToggleOff = true;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string toggleParameter = "IsOn";

        public bool IsOn => isOn;

        public override string InteractionPrompt => isOn ? onPrompt : offPrompt;

        protected override void OnInteract(GameObject interactor)
        {
            if (isOn && !canToggleOff)
                return;

            isOn = !isOn;
            UpdateState();
        }

        private void UpdateState()
        {
            if (animator != null)
            {
                animator.SetBool(toggleParameter, isOn);
            }

            interactionPrompt = isOn ? onPrompt : offPrompt;
        }

        /// <summary>
        /// Set the toggle state without triggering interaction
        /// </summary>
        public void SetState(bool state)
        {
            isOn = state;
            UpdateState();
        }

        /// <summary>
        /// Force the toggle to on state
        /// </summary>
        public void TurnOn()
        {
            SetState(true);
        }

        /// <summary>
        /// Force the toggle to off state
        /// </summary>
        public void TurnOff()
        {
            SetState(false);
        }
    }
}
