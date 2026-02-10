using UnityEngine;

namespace Shizounu.Library.Interaction
{
    /// <summary>
    /// Helper component that enables interaction when the player enters a trigger zone
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class InteractionZone : MonoBehaviour
    {
        [Header("Zone Settings")]
        [SerializeField] private InteractableBase targetInteractable;
        [SerializeField] private bool autoInteract = false;
        [SerializeField] private float autoInteractDelay = 0f;
        [SerializeField] private bool oneTimeUse = false;
        [SerializeField] private string requiredTag = "Player";

        [Header("Events")]
        public UnityEngine.Events.UnityEvent<GameObject> OnPlayerEntered;
        public UnityEngine.Events.UnityEvent<GameObject> OnPlayerExited;

        private Collider triggerCollider;
        private bool hasBeenUsed = false;
        private GameObject currentPlayer;
        private float enterTime;

        private void Awake()
        {
            triggerCollider = GetComponent<Collider>();
            if (!triggerCollider.isTrigger)
            {
                Debug.LogWarning($"InteractionZone on {gameObject.name} should have a trigger collider!");
                triggerCollider.isTrigger = true;
            }
        }

        private void Update()
        {
            if (autoInteract && currentPlayer != null && !hasBeenUsed)
            {
                if (Time.time - enterTime >= autoInteractDelay)
                {
                    PerformInteraction();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasBeenUsed && oneTimeUse)
                return;

            if (string.IsNullOrEmpty(requiredTag) || other.CompareTag(requiredTag))
            {
                currentPlayer = other.gameObject;
                enterTime = Time.time;
                OnPlayerEntered?.Invoke(other.gameObject);

                if (targetInteractable != null)
                {
                    targetInteractable.SetInteractable(true);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (string.IsNullOrEmpty(requiredTag) || other.CompareTag(requiredTag))
            {
                if (currentPlayer == other.gameObject)
                {
                    currentPlayer = null;
                }

                OnPlayerExited?.Invoke(other.gameObject);

                if (targetInteractable != null && !autoInteract)
                {
                    targetInteractable.SetInteractable(false);
                }
            }
        }

        private void PerformInteraction()
        {
            if (targetInteractable != null && targetInteractable.CanInteract)
            {
                targetInteractable.Interact(currentPlayer);
                hasBeenUsed = true;
            }
        }

        /// <summary>
        /// Reset the zone for reuse
        /// </summary>
        public void Reset()
        {
            hasBeenUsed = false;
            currentPlayer = null;
        }
    }
}
