using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Orbit")]
    [SerializeField] private float lookSensitivity = 120f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private float minDistance = 1.5f;
    [SerializeField] private float maxDistance = 6f;
    [SerializeField] private float zoomSpeed = 3f;
    [SerializeField] private float followSmoothTime = 0.08f;

    private Vector2 lookInput;
    private float zoomInput;
    private float yaw;
    private float pitch = 20f;
    private float distance = 4f;
    private Vector3 followVelocity;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            lookInput = mouseDelta;
            
            Vector2 scroll = Mouse.current.scroll.ReadValue();
            zoomInput = scroll.y / 120f;
        }

        yaw += lookInput.x * lookSensitivity * Time.deltaTime;
        pitch -= lookInput.y * lookSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        distance = Mathf.Clamp(distance - zoomInput * zoomSpeed, minDistance, maxDistance);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = target.position + targetOffset - (rotation * Vector3.forward * distance);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref followVelocity, followSmoothTime);
        transform.rotation = rotation;
    }

    public void SetLookInput(Vector2 input)
    {
        lookInput = input;
    }

    public void SetZoomInput(float input)
    {
        zoomInput = input;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SnapToTarget()
    {
        if (target == null)
        {
            return;
        }

        Vector3 toTarget = (target.position + targetOffset) - transform.position;
        if (toTarget.sqrMagnitude > 0.001f)
        {
            Quaternion rotation = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            yaw = rotation.eulerAngles.y;
            pitch = rotation.eulerAngles.x;
        }
    }
}
