using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    public InputActionReference moveActionRef;
    public InputActionReference cameraActionRef;

    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float minViewDistance = -80f;
    [SerializeField] private float maxViewDistance = 80f;
    [SerializeField] private bool invertY = false;

    


    private float xRotation = 0f;

    private void OnEnable()
    {
        moveActionRef.action.Enable();
        cameraActionRef.action.Enable();
    }

    private void OnDisable()
    {
        moveActionRef.action.Disable();
        cameraActionRef.action.Disable();
    }

    void Update()
    {
        HandleMovement();
        HandleCameraRotation();
    }

    void HandleMovement()
    {
        Vector2 input = moveActionRef.action.ReadValue<Vector2>();

        // Déplacement relatif à l’orientation du joueur
        Vector3 move =
            transform.right * input.x +
            transform.forward * input.y;

        transform.position += move * speed * Time.deltaTime;
    }

    void HandleCameraRotation()
    {
        Vector2 mouseInput = cameraActionRef.action.ReadValue<Vector2>() * mouseSensitivity * Time.deltaTime;

        // Rotation verticale (pitch)
        xRotation += (invertY ? mouseInput.y : -mouseInput.y);
        xRotation = Mathf.Clamp(xRotation, minViewDistance, maxViewDistance);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotation horizontale (yaw) -> joueur
        transform.Rotate(Vector3.up * mouseInput.x);
    }
}
