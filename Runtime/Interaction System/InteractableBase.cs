using UnityEngine;
using UnityEngine.Events;

namespace Shizounu.Library.Interaction
{
    /// <summary>
    /// Base class for interactable objects with common functionality
    /// </summary>
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField] protected string interactionPrompt = "Interact";
        [SerializeField] protected int interactionPriority = 0;
        [SerializeField] protected bool canInteract = true;

        [Header("Visual Feedback")]
        [SerializeField] protected bool highlightOnFocus = true;
        [SerializeField] protected Material highlightMaterial;
        [SerializeField] protected Color highlightColor = Color.yellow;

        [Header("Events")]
        public UnityEvent<GameObject> OnInteracted;
        public UnityEvent OnFocusGained;
        public UnityEvent OnFocusLost;

        protected Renderer[] renderers;
        protected Material[] originalMaterials;
        protected bool isFocused;

        public Transform Transform => transform;
        public virtual bool CanInteract => canInteract;
        public virtual string InteractionPrompt => interactionPrompt;
        public virtual int InteractionPriority => interactionPriority;

        protected virtual void Awake()
        {
            if (highlightOnFocus)
            {
                renderers = GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    originalMaterials = new Material[renderers.Length];
                    for (int i = 0; i < renderers.Length; i++)
                    {
                        originalMaterials[i] = renderers[i].material;
                    }
                }
            }
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract)
                return;

            OnInteract(interactor);
            OnInteracted?.Invoke(interactor);
        }

        public virtual void OnFocused()
        {
            isFocused = true;
            OnFocusGained?.Invoke();

            if (highlightOnFocus && renderers != null)
            {
                ApplyHighlight();
            }
        }

        public virtual void OnUnfocused()
        {
            isFocused = false;
            OnFocusLost?.Invoke();

            if (highlightOnFocus && renderers != null)
            {
                RemoveHighlight();
            }
        }

        /// <summary>
        /// Override this method to implement custom interaction logic
        /// </summary>
        protected abstract void OnInteract(GameObject interactor);

        /// <summary>
        /// Enable or disable this interactable
        /// </summary>
        public virtual void SetInteractable(bool value)
        {
            canInteract = value;
        }

        /// <summary>
        /// Change the interaction prompt text
        /// </summary>
        public virtual void SetPrompt(string prompt)
        {
            interactionPrompt = prompt;
        }

        protected virtual void ApplyHighlight()
        {
            foreach (var renderer in renderers)
            {
                if (highlightMaterial != null)
                {
                    renderer.material = highlightMaterial;
                }
                else
                {
                    // Apply color tint
                    renderer.material.color = highlightColor;
                }
            }
        }

        protected virtual void RemoveHighlight()
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material = originalMaterials[i];
            }
        }

        protected virtual void OnDestroy()
        {
            // Clean up material instances
            if (highlightOnFocus && renderers != null)
            {
                foreach (var renderer in renderers)
                {
                    if (renderer != null && renderer.material != null)
                    {
                        Destroy(renderer.material);
                    }
                }
            }
        }
    }
}
