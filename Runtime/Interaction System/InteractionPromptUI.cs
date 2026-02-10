using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Shizounu.Library.Interaction
{
    /// <summary>
    /// UI component that displays interaction prompts and feedback
    /// </summary>
    public class InteractionPromptUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InteractionController interactionController;
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Text Components")]
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private TextMeshProUGUI keyText;
        [SerializeField] private Image progressBar;

        [Header("Settings")]
        [SerializeField] private string defaultKeyText = "[E]";
        [SerializeField] private float fadeSpeed = 5f;
        [SerializeField] private bool showDistance = false;
        [SerializeField] private string distanceFormat = " ({0:F1}m)";

        [Header("Progress Bar")]
        [SerializeField] private bool showProgressForHoldInteractables = true;

        private HoldInteractable currentHoldInteractable;
        private bool isVisible = false;

        private void Start()
        {
            if (interactionController == null)
            {
                interactionController = FindFirstObjectByType<InteractionController>();
            }

            if (interactionController != null)
            {
                interactionController.OnInteractableFocused.AddListener(OnInteractableFocused);
                interactionController.OnInteractableUnfocused.AddListener(OnInteractableUnfocused);
            }

            if (canvasGroup == null && canvas != null)
            {
                canvasGroup = canvas.GetComponent<CanvasGroup>();
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            if (progressBar != null)
            {
                progressBar.fillAmount = 0f;
                progressBar.gameObject.SetActive(false);
            }

            Hide();
        }

        private void Update()
        {
            // Update visibility
            if (canvasGroup != null)
            {
                float targetAlpha = isVisible ? 1f : 0f;
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
            }

            // Update progress bar for hold interactables
            if (currentHoldInteractable != null && showProgressForHoldInteractables)
            {
                if (progressBar != null)
                {
                    progressBar.fillAmount = currentHoldInteractable.HoldProgress;
                }
            }

            // Update distance if enabled
            if (isVisible && showDistance && interactionController != null)
            {
                UpdateDistance();
            }
        }

        private void OnInteractableFocused(IInteractable interactable)
        {
            Show(interactable);
            
            currentHoldInteractable = interactable as HoldInteractable;
            if (currentHoldInteractable != null && progressBar != null && showProgressForHoldInteractables)
            {
                progressBar.gameObject.SetActive(true);
            }
        }

        private void OnInteractableUnfocused(IInteractable interactable)
        {
            Hide();
            currentHoldInteractable = null;
            
            if (progressBar != null)
            {
                progressBar.gameObject.SetActive(false);
                progressBar.fillAmount = 0f;
            }
        }

        private void Show(IInteractable interactable)
        {
            isVisible = true;

            if (canvas != null)
            {
                canvas.gameObject.SetActive(true);
            }

            if (promptText != null)
            {
                promptText.text = interactable.InteractionPrompt;
            }

            if (keyText != null)
            {
                keyText.text = defaultKeyText;
            }
        }

        private void Hide()
        {
            isVisible = false;

            if (canvas != null && canvasGroup != null && canvasGroup.alpha <= 0.01f)
            {
                canvas.gameObject.SetActive(false);
            }
        }

        private void UpdateDistance()
        {
            if (interactionController.CurrentFocusedInteractable != null && promptText != null)
            {
                float distance = Vector3.Distance(
                    interactionController.transform.position,
                    interactionController.CurrentFocusedInteractable.Transform.position
                );

                string basePrompt = interactionController.CurrentFocusedInteractable.InteractionPrompt;
                promptText.text = basePrompt + string.Format(distanceFormat, distance);
            }
        }

        /// <summary>
        /// Set the key text to display
        /// </summary>
        public void SetKeyText(string key)
        {
            defaultKeyText = key;
            if (keyText != null)
            {
                keyText.text = key;
            }
        }

        /// <summary>
        /// Manually show a prompt
        /// </summary>
        public void ShowPrompt(string prompt)
        {
            isVisible = true;
            if (canvas != null)
            {
                canvas.gameObject.SetActive(true);
            }
            if (promptText != null)
            {
                promptText.text = prompt;
            }
        }

        /// <summary>
        /// Manually hide the prompt
        /// </summary>
        public void HidePrompt()
        {
            Hide();
        }

        private void OnDestroy()
        {
            if (interactionController != null)
            {
                interactionController.OnInteractableFocused.RemoveListener(OnInteractableFocused);
                interactionController.OnInteractableUnfocused.RemoveListener(OnInteractableUnfocused);
            }
        }
    }
}
