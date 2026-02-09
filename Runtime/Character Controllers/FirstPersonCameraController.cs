using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonCameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 1.6f, 0f);

    [Header("Look")]
    [SerializeField] private float lookSensitivity = 120f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    private Vector2 lookInput;
    private float yaw;
    private float pitch;

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
        }

        yaw += lookInput.x * lookSensitivity * Time.deltaTime;
        pitch -= lookInput.y * lookSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.position = target.position + rotation * localOffset;
        transform.rotation = rotation;
    }

    public void SetLookInput(Vector2 input)
    {
        lookInput = input;
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

        Vector3 forward = transform.forward;
        yaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        pitch = Mathf.Asin(forward.y) * Mathf.Rad2Deg;
        transform.position = target.position + transform.rotation * localOffset;
    }
}
