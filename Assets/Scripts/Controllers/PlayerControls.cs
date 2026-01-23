using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerControls : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference MoveActionRef;
    [SerializeField] private InputActionReference NextCameraActionRef;
    [SerializeField] private InputActionReference PrevCameraActionRef;
    [SerializeField] private InputActionReference CrouchActionRef;
    [SerializeField] private InputActionReference JumpActionRef;
    [SerializeField] private InputActionReference SprintActionRef;
    [SerializeField] private InputActionReference InteractActionRef;
    [SerializeField] private InputActionReference MenuActionRef;


    [Header("Cameras")]
    public List<Camera> Cameras;

    [Header("Movement")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float speedMultiplier = 1.0f;
    [SerializeField] float rotationSpeed = 10f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;

    private float verticalVelocity;
    private Animator animator;

    private int currentCameraIndex = 0;
    private CharacterController controller;
    private GameManager gm;
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        gm = GameManager.Instance;
    }


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
        MenuActionRef.action.Enable();

        NextCameraActionRef.action.performed += OnNextCamera;
        PrevCameraActionRef.action.performed += OnPrevCamera;
        JumpActionRef.action.performed += OnJump;
        CrouchActionRef.action.performed += OnCrouch;
        CrouchActionRef.action.canceled += OnUncrouch;
        SprintActionRef.action.performed += OnSprint;
        SprintActionRef.action.canceled += OnStopSprint;
        InteractActionRef.action.performed += OnInteract;
        MenuActionRef.action.performed += OnPause;
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
        MenuActionRef.action.performed -= OnPause;

        MoveActionRef.action.Disable();
        NextCameraActionRef.action.Disable();
        PrevCameraActionRef.action.Disable();
        CrouchActionRef.action.Disable();
        JumpActionRef.action.Disable();
        SprintActionRef.action.Disable();
        InteractActionRef.action.Disable();
        MenuActionRef.action.Disable();
    }


    #endregion

    private void Update()
    {
        HandleMovement();

        if(IsMoving())
            animator.SetBool("iswalking", true);
        else
            animator.SetBool("iswalking", false);

        bool isGrounded = controller.isGrounded;
        animator.SetBool("isgrounded", isGrounded);
    }

    #region Movement

    void HandleMovement()
    {
        if (Cameras == null || Cameras.Count == 0)
            return;

        Camera activeCam = Cameras[currentCameraIndex];
        Vector2 input = MoveActionRef.action.ReadValue<Vector2>();

        Vector3 camForward = activeCam.transform.forward;
        Vector3 camRight = activeCam.transform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 move =
            camRight * input.x +
            camForward * input.y;

        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }



        float currentSpeed = speed * speedMultiplier;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 finalMove = move * currentSpeed;
        finalMove.y = verticalVelocity;

        controller.Move(finalMove * Time.deltaTime);
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
        if (!controller.isGrounded)
            return;

        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        animator.SetTrigger("jump");
    }


    void OnCrouch(InputAction.CallbackContext ctx)
    {
        speedMultiplier = 0.2f;
        Debug.Log("Crouch");
        animator.SetBool("iscrouching", true);
        animator.SetBool("isrunning", false);
    }

    void OnUncrouch(InputAction.CallbackContext ctx)
    {
        speedMultiplier = 1f;
        Debug.Log("Uncrouch");
        animator.SetBool("iscrouching", false);
    }

    void OnSprint(InputAction.CallbackContext ctx)
    {
        if (animator.GetBool("iscrouching"))
            return;
        speedMultiplier = 2f;
        Debug.Log("Sprint");
        animator.SetBool("isrunning", true);
    }

    void OnStopSprint(InputAction.CallbackContext ctx)
    {
        if (animator.GetBool("iscrouching"))
            return;
        speedMultiplier = 1f;
        Debug.Log("Stop Sprint");
        animator.SetBool("isrunning", false);
    }

    void OnInteract(InputAction.CallbackContext ctx)
    {
        if (gm.currentdesk == null) return;

        gm.currentdesk.StartInteraction(this);
        animator.SetTrigger("interact");
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

    public bool IsMoving()
    {
        if (Cameras == null || Cameras.Count == 0)
            return false;

        Vector2 input = MoveActionRef.action.ReadValue<Vector2>();
        return input.sqrMagnitude > 0.01f;
    }

    private void OnPause(InputAction.CallbackContext ctx)
    {
        bool isPaused = UIManager.Instance.endGameUI.activeSelf;

        if (isPaused)
            gm.ResumeGame();
        else
            gm.PauseGame();
    }
}
