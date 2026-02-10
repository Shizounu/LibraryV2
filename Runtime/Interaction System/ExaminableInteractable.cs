using UnityEngine;
using UnityEngine.Events;

namespace Shizounu.Library.Interaction
{
    /// <summary>
    /// Interactable that can be examined to show information
    /// </summary>
    public class ExaminableInteractable : InteractableBase
    {
        [Header("Examination Settings")]
        [SerializeField] private string examinationTitle = "Item";
        [SerializeField] [TextArea(3, 10)] private string examinationDescription = "Description here...";
        [SerializeField] private Sprite examinationImage;
        [SerializeField] private GameObject examinationPrefab;

        [Header("Camera Settings")]
        [SerializeField] private bool useExaminationCamera = true;
        [SerializeField] private Vector3 examineCameraOffset = new Vector3(0, 0, -2);
        [SerializeField] private float examineRotationSpeed = 50f;
        [SerializeField] private bool allowRotation = true;

        [Header("Events")]
        public UnityEvent OnExamineStarted;
        public UnityEvent OnExamineEnded;

        private bool isBeingExamined = false;
        private Camera mainCamera;
        private Vector3 originalCameraPosition;
        private Quaternion originalCameraRotation;
        private GameObject examinationInstance;

        public bool IsBeingExamined => isBeingExamined;
        public string ExaminationTitle => examinationTitle;
        public string ExaminationDescription => examinationDescription;
        public Sprite ExaminationImage => examinationImage;

        protected override void OnInteract(GameObject interactor)
        {
            if (!isBeingExamined)
            {
                StartExamination(interactor);
            }
            else
            {
                EndExamination();
            }
        }

        private void StartExamination(GameObject interactor)
        {
            isBeingExamined = true;
            OnExamineStarted?.Invoke();

            if (useExaminationCamera)
            {
                mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    originalCameraPosition = mainCamera.transform.position;
                    originalCameraRotation = mainCamera.transform.rotation;

                    // Position camera for examination
                    mainCamera.transform.position = transform.position + examineCameraOffset;
                    mainCamera.transform.LookAt(transform);
                }
            }

            if (examinationPrefab != null)
            {
                examinationInstance = Instantiate(examinationPrefab);
                // Position examination UI/3D object as needed
            }

            interactionPrompt = "Stop Examining";
        }

        private void EndExamination()
        {
            isBeingExamined = false;
            OnExamineEnded?.Invoke();

            if (mainCamera != null)
            {
                mainCamera.transform.position = originalCameraPosition;
                mainCamera.transform.rotation = originalCameraRotation;
                mainCamera = null;
            }

            if (examinationInstance != null)
            {
                Destroy(examinationInstance);
                examinationInstance = null;
            }

            interactionPrompt = "Examine";
        }

        private void Update()
        {
            if (isBeingExamined && allowRotation)
            {
                // Allow rotation during examination
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");

                transform.Rotate(Vector3.up, -mouseX * examineRotationSpeed * Time.deltaTime, Space.World);
                transform.Rotate(Vector3.right, mouseY * examineRotationSpeed * Time.deltaTime, Space.World);
            }
        }

        /// <summary>
        /// Set examination information
        /// </summary>
        public void SetExaminationInfo(string title, string description, Sprite image = null)
        {
            examinationTitle = title;
            examinationDescription = description;
            if (image != null)
            {
                examinationImage = image;
            }
        }
    }
}
