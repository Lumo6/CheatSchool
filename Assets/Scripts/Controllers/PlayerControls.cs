using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerControls : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionReference MoveActionRef;
    public InputActionReference NextCameraActionRef;
    public InputActionReference PrevCameraActionRef;
    public InputActionReference CrouchActionRef;
    public InputActionReference JumpActionRef;
    public InputActionReference SprintActionRef;
    public InputActionReference InteractActionRef;


    [Header("Cameras")]
    public List<Camera> Cameras;

    [Header("Movement")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float speedMultiplier = 1.0f;

    private int currentCameraIndex = 0;
    private bool isSprinting = false;
    private bool isCrouching = false;
    private CopyTargetDesk currentDesk;

    #region Enable / Disable

    private void OnEnable()
    {
        MoveActionRef.action.Enable();
        NextCameraActionRef.action.Enable();
        PrevCameraActionRef.action.Enable();
        CrouchActionRef.action.Enable();
        JumpActionRef.action.Enable();
        SprintActionRef.action.Enable();
        InteractActionRef.action.Enable();

        NextCameraActionRef.action.performed += OnNextCamera;
        PrevCameraActionRef.action.performed += OnPrevCamera;
        JumpActionRef.action.performed += OnJump;
        CrouchActionRef.action.performed += OnCrouch;
        CrouchActionRef.action.canceled += OnUncrouch;
        SprintActionRef.action.performed += OnSprint;
        SprintActionRef.action.canceled += OnStopSprint;
        InteractActionRef.action.performed += OnInteract;
    }

    private void OnDisable()
    {
        NextCameraActionRef.action.performed -= OnNextCamera;
        PrevCameraActionRef.action.performed -= OnPrevCamera;
        JumpActionRef.action.performed -= OnJump;
        CrouchActionRef.action.performed -= OnCrouch;
        CrouchActionRef.action.canceled -= OnUncrouch;
        SprintActionRef.action.performed -= OnSprint;
        SprintActionRef.action.canceled -= OnStopSprint;
        InteractActionRef.action.performed -= OnInteract;

        MoveActionRef.action.Disable();
        NextCameraActionRef.action.Disable();
        PrevCameraActionRef.action.Disable();
        CrouchActionRef.action.Disable();
        JumpActionRef.action.Disable();
        SprintActionRef.action.Disable();
        InteractActionRef.action.Disable();
    }


    #endregion

    private void Update()
    {
        HandleMovement();
    }

    #region Movement

    void HandleMovement()
    {
        Vector2 input = MoveActionRef.action.ReadValue<Vector2>();

        Vector3 move =
            transform.right * input.x +
            transform.forward * input.y;

        float currentSpeed = isSprinting ? speed * speedMultiplier : speed;
        transform.position += move * currentSpeed * Time.deltaTime;
    }

    #endregion

    #region Input Functions

    void OnNextCamera(InputAction.CallbackContext ctx)
    {
        currentCameraIndex = (currentCameraIndex + 1) % Cameras.Count;
        SwitchCamera(currentCameraIndex);
    }

    void OnPrevCamera(InputAction.CallbackContext ctx)
    {
        currentCameraIndex--;
        if (currentCameraIndex < 0)
            currentCameraIndex = Cameras.Count - 1;

        SwitchCamera(currentCameraIndex);
    }

    void OnJump(InputAction.CallbackContext ctx)
    {
        Debug.Log("Jump");
        // Add jump logic (Rigidbody, CharacterController, etc.)
    }

    void OnCrouch(InputAction.CallbackContext ctx)
    {
        isCrouching = true;
        speedMultiplier = 0.2f;
        Debug.Log("Crouch");
    }

    void OnUncrouch(InputAction.CallbackContext ctx)
    {
        isCrouching = false;
        speedMultiplier = 1f;
        Debug.Log("Uncrouch");
    }

    void OnSprint(InputAction.CallbackContext ctx)
    {
        isSprinting = true;
        speedMultiplier = 2f;
        Debug.Log("Sprint");
    }

    void OnStopSprint(InputAction.CallbackContext ctx)
    {
        isSprinting = false;
        speedMultiplier = 1f;
        Debug.Log("Stop Sprint");
    }

    void OnInteract(InputAction.CallbackContext ctx)
    {
        if (currentDesk == null) return;

        Debug.Log("Interact pressed");
        currentDesk.StartInteraction();
    }

    #endregion

    #region Camera Management
    void SwitchCamera(int index)
    {
        for (int i = 0; i < Cameras.Count; i++)
        {
            Cameras[i].gameObject.SetActive(i == index);
        }
    }
    #endregion

    #region Setters / Getters
    public void SetCurrentDesk(CopyTargetDesk desk)
    {
        currentDesk = desk;
    }

    public void ClearCurrentDesk(CopyTargetDesk desk)
    {
        if (currentDesk == desk)
        {
            desk.StopInteraction();
            currentDesk = null;
        }
    }

    #endregion
}
