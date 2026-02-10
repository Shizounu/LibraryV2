using UnityEngine;
using System.Collections.Generic;

namespace Shizounu.Library.Interaction
{
    /// <summary>
    /// Manages a sequence of interactables that must be activated in order
    /// </summary>
    public class InteractionSequence : MonoBehaviour
    {
        [System.Serializable]
        public class SequenceStep
        {
            public InteractableBase interactable;
            public bool required = true;
            [Tooltip("Delay before this step becomes available")]
            public float delay = 0f;
        }

        [Header("Sequence Settings")]
        [SerializeField] private List<SequenceStep> steps = new List<SequenceStep>();
        [SerializeField] private bool loopSequence = false;
        [SerializeField] private bool resetOnFailure = false;

        [Header("Events")]
        public UnityEngine.Events.UnityEvent OnSequenceStarted;
        public UnityEngine.Events.UnityEvent OnSequenceCompleted;
        public UnityEngine.Events.UnityEvent OnSequenceFailed;

        private int currentStepIndex = 0;
        private bool isComplete = false;
        private float stepStartTime = 0f;

        public int CurrentStep => currentStepIndex;
        public bool IsComplete => isComplete;
        public float Progress => steps.Count > 0 ? (float)currentStepIndex / steps.Count : 0f;

        private void Start()
        {
            InitializeSequence();
        }

        private void InitializeSequence()
        {
            // Subscribe to all interactables
            foreach (var step in steps)
            {
                if (step.interactable != null)
                {
                    step.interactable.OnInteracted.AddListener(OnInteractableUsed);
                    step.interactable.SetInteractable(false); // Disable all initially
                }
            }

            // Enable first step
            if (steps.Count > 0)
            {
                EnableStep(0);
                OnSequenceStarted?.Invoke();
            }
        }

        private void OnInteractableUsed(GameObject interactor)
        {
            if (isComplete)
                return;

            var currentStep = steps[currentStepIndex];

            // Check if the correct interactable was used
            if (currentStep.interactable != null)
            {
                AdvanceSequence();
            }
            else if (resetOnFailure)
            {
                FailSequence();
            }
        }

        private void Update()
        {
            // Check if enough time has passed for the current step
            if (!isComplete && currentStepIndex < steps.Count)
            {
                var currentStep = steps[currentStepIndex];
                if (currentStep.delay > 0 && Time.time - stepStartTime < currentStep.delay)
                {
                    if (currentStep.interactable != null)
                    {
                        currentStep.interactable.SetInteractable(false);
                    }
                }
                else
                {
                    if (currentStep.interactable != null && !currentStep.interactable.CanInteract)
                    {
                        currentStep.interactable.SetInteractable(true);
                    }
                }
            }
        }

        private void EnableStep(int index)
        {
            if (index >= 0 && index < steps.Count)
            {
                currentStepIndex = index;
                stepStartTime = Time.time;
                
                var step = steps[index];
                if (step.interactable != null && step.delay <= 0)
                {
                    step.interactable.SetInteractable(true);
                }
            }
        }

        private void AdvanceSequence()
        {
            currentStepIndex++;

            if (currentStepIndex >= steps.Count)
            {
                CompleteSequence();
            }
            else
            {
                EnableStep(currentStepIndex);
            }
        }

        private void CompleteSequence()
        {
            isComplete = true;
            OnSequenceCompleted?.Invoke();

            if (loopSequence)
            {
                ResetSequence();
            }
        }

        private void FailSequence()
        {
            OnSequenceFailed?.Invoke();
            ResetSequence();
        }

        /// <summary>
        /// Reset the sequence to the beginning
        /// </summary>
        public void ResetSequence()
        {
            // Disable all steps
            foreach (var step in steps)
            {
                if (step.interactable != null)
                {
                    step.interactable.SetInteractable(false);
                }
            }

            currentStepIndex = 0;
            isComplete = false;
            EnableStep(0);
        }

        /// <summary>
        /// Skip to a specific step
        /// </summary>
        public void SkipToStep(int index)
        {
            if (index >= 0 && index < steps.Count)
            {
                EnableStep(index);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from all interactables
            foreach (var step in steps)
            {
                if (step.interactable != null)
                {
                    step.interactable.OnInteracted.RemoveListener(OnInteractableUsed);
                }
            }
        }
    }
}
