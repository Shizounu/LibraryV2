using UnityEngine;

namespace Shizounu.Library.Interaction
{
    /// <summary>
    /// Interface for objects that can be interacted with
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// The transform of the interactable object
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// Whether this interactable can currently be interacted with
        /// </summary>
        bool CanInteract { get; }

        /// <summary>
        /// Text to display when the player can interact with this object
        /// </summary>
        string InteractionPrompt { get; }

        /// <summary>
        /// Priority for this interaction (higher values take precedence when multiple are available)
        /// </summary>
        int InteractionPriority { get; }

        /// <summary>
        /// Called when the player interacts with this object
        /// </summary>
        /// <param name="interactor">The GameObject that initiated the interaction</param>
        void Interact(GameObject interactor);

        /// <summary>
        /// Called when the interactable becomes the focused/highlighted target
        /// </summary>
        void OnFocused();

        /// <summary>
        /// Called when the interactable is no longer focused
        /// </summary>
        void OnUnfocused();
    }
}
