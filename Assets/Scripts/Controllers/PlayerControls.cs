using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference moveActionRef;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    [Header("Movement")]
    [SerializeField] private float speed = 50f;
    [SerializeField] private float smoothTime = 0.1f;

    private Vector3 currentVelocity;

    private void OnEnable()
    {
        moveActionRef.action.Enable();
    }

    private void OnDisable()
    {
        moveActionRef.action.Disable();
    }

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        Vector2 input = moveActionRef.action.ReadValue<Vector2>();
        if (input.sqrMagnitude < 0.01f) return;

        // Camera directions (flattened on Y)
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        // Camera-relative movement
        Vector3 targetDirection = camRight * input.x + camForward * input.y;

        Vector3 targetVelocity = targetDirection * speed;

        // Smooth movement
        Vector3 smoothMove = Vector3.SmoothDamp(
            Vector3.zero,
            targetVelocity,
            ref currentVelocity,
            smoothTime
        );

        transform.position += smoothMove * Time.deltaTime;

        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                10f * Time.deltaTime
            );
        }
    }
}
