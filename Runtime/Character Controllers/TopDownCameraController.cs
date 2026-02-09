using UnityEngine;
using UnityEngine.InputSystem;

public class TopDownCameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset = Vector3.zero;

    [Header("Rig")]
    [SerializeField] private float height = 12f;
    [SerializeField] private float distance = 8f;
    [SerializeField] private float rotation = 45f;
    [SerializeField] private float followSmoothTime = 0.1f;

    [Header("Zoom")]
    [SerializeField] private float minHeight = 6f;
    [SerializeField] private float maxHeight = 20f;
    [SerializeField] private float zoomSpeed = 6f;

    private float zoomInput;
    private Vector3 followVelocity;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        if (Mouse.current != null)
        {
            Vector2 scroll = Mouse.current.scroll.ReadValue();
            zoomInput = scroll.y / 120f;
        }

        height = Mathf.Clamp(height - zoomInput * zoomSpeed, minHeight, maxHeight);

        Quaternion yawRotation = Quaternion.Euler(0f, rotation, 0f);
        Vector3 offset = yawRotation * new Vector3(0f, height, -distance);
        Vector3 desiredPosition = target.position + targetOffset + offset;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref followVelocity, followSmoothTime);
        transform.rotation = Quaternion.LookRotation((target.position + targetOffset) - transform.position, Vector3.up);
    }

    public void SetZoomInput(float input)
    {
        zoomInput = input;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetRotation(float yawDegrees)
    {
        rotation = yawDegrees;
    }
}
