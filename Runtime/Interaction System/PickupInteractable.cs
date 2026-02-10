using UnityEngine;

namespace Shizounu.Library.Interaction
{
    /// <summary>
    /// Interactable that can be picked up and held by the player
    /// </summary>
    public class PickupInteractable : InteractableBase
    {
        [Header("Pickup Settings")]
        [SerializeField] private bool canDrop = true;
        [SerializeField] private Transform holdPosition;
        [SerializeField] private Vector3 holdOffset = Vector3.forward;
        [SerializeField] private bool disablePhysicsWhenHeld = true;

        private Rigidbody rb;
        private Collider col;
        private Transform originalParent;
        private bool isBeingHeld = false;
        private GameObject currentHolder;

        public bool IsBeingHeld => isBeingHeld;
        public GameObject CurrentHolder => currentHolder;

        public override bool CanInteract => base.CanInteract && !isBeingHeld;

        protected override void Awake()
        {
            base.Awake();
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
            originalParent = transform.parent;
        }

        protected override void OnInteract(GameObject interactor)
        {
            PickUp(interactor);
        }

        /// <summary>
        /// Pick up this object
        /// </summary>
        public void PickUp(GameObject holder)
        {
            if (isBeingHeld)
                return;

            currentHolder = holder;
            isBeingHeld = true;

            // Set position
            if (holdPosition != null)
            {
                transform.SetParent(holdPosition);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
            else
            {
                transform.SetParent(holder.transform);
                transform.localPosition = holdOffset;
                transform.localRotation = Quaternion.identity;
            }

            // Disable physics
            if (disablePhysicsWhenHeld)
            {
                if (rb != null)
                {
                    rb.isKinematic = true;
                }
                if (col != null)
                {
                    col.enabled = false;
                }
            }
        }

        /// <summary>
        /// Drop this object
        /// </summary>
        public void Drop()
        {
            if (!isBeingHeld || !canDrop)
                return;

            isBeingHeld = false;
            currentHolder = null;

            transform.SetParent(originalParent);

            // Re-enable physics
            if (disablePhysicsWhenHeld)
            {
                if (rb != null)
                {
                    rb.isKinematic = false;
                }
                if (col != null)
                {
                    col.enabled = true;
                }
            }
        }

        /// <summary>
        /// Drop with force
        /// </summary>
        public void Drop(Vector3 force)
        {
            Drop();
            if (rb != null)
            {
                rb.AddForce(force, ForceMode.Impulse);
            }
        }
    }
}
