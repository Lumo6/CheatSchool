using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerControls : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference MoveActionRef;// Reference de l'action de mouvement.
    [SerializeField] private InputActionReference NextCameraActionRef;// Reference de l'action pour passer a la camera suivante.
    [SerializeField] private InputActionReference PrevCameraActionRef;// Reference de l'action pour passer a la camera precedente.
    [SerializeField] private InputActionReference CrouchActionRef;// Reference de l'action d'accroupissement.
    [SerializeField] private InputActionReference JumpActionRef;// Reference de l'action de saut.
    [SerializeField] private InputActionReference SprintActionRef;// Reference de l'action de sprint.
    [SerializeField] private InputActionReference InteractActionRef;// Reference de l'action d'interaction.
    [SerializeField] private InputActionReference MenuActionRef;// Reference de l'action de menu.


    [Header("Cameras")]
    public List<Camera> Cameras;// Liste des cameras disponibles pour le joueur.

    [Header("Movement")]
    [SerializeField] private float speed = 10f;// Vitesse de deplacement du joueur.
    [SerializeField] private float speedMultiplier = 1.0f;// Multiplicateur de vitesse pour les differentes actions (sprint, accroupissement).
    [SerializeField] float rotationSpeed = 10f;// Vitesse de rotation du joueur.

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 1.5f;// Hauteur du saut du joueur.
    [SerializeField] private float gravity = -9.81f;// Force de gravite appliquee au joueur.

    private float verticalVelocity;// Vitesse verticale actuelle du joueur.
    private Animator animator;// Reference a l'animator du joueur.

    private int currentCameraIndex = 0;// Index de la camera actuellement active.
    private CharacterController controller;// Reference au CharacterController du joueur.
    private GameManager gm;// Reference au GameManager.
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        gm = GameManager.Instance;
    }


    #region Enable / Disable

    /// <summary>
    /// Active les actions d'entree et attache les gestionnaires d'evenements.
    /// </summary>
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

    /// <summary>
    /// Desactive les actions d'entree et d�tache les gestionnaires d'evenements.
    /// </summary>
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

    /// <summary>
    /// Met a jour le mouvement du joueur et les animations associees.
    /// </summary>
    private void Update()
    {
        // Gestion du mouvement
        HandleMovement();
        // Mise a jour des animations
        if (IsMoving())
            animator.SetBool("iswalking", true);
        else
            animator.SetBool("iswalking", false);
        // Mise a jour de l'etat au sol
        bool isGrounded = controller.isGrounded;
        animator.SetBool("isgrounded", isGrounded);
    }

    #region Movement

    /// <summary>
    /// Gere le mouvement du joueur en fonction de l'entree utilisateur et de la camera active.
    /// </summary>
    void HandleMovement()
    {
        // Verifie qu'il y a des cameras disponibles
        if (Cameras == null || Cameras.Count == 0)
            return;
        // Récupère la caméra active
        Camera activeCam = Cameras[currentCameraIndex];
        Vector2 input = MoveActionRef.action.ReadValue<Vector2>();
        // Calcule la direction de mouvement en fonction de la caméra
        Vector3 camForward = activeCam.transform.forward;
        Vector3 camRight = activeCam.transform.right;
        // Ignore la composante verticale de la caméra
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();
        // Calcule le vecteur de mouvement
        Vector3 move =
            camRight * input.x +
            camForward * input.y;
        // Applique la rotation du joueur vers la direction de mouvement
        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
        // Calcule la vitesse actuelle en fonction du multiplicateur
        float currentSpeed = speed * speedMultiplier;
        // Gère la gravité et le saut
        verticalVelocity += gravity * Time.deltaTime;
        // Réinitialise la vélocité verticale si le joueur est au sol
        Vector3 finalMove = move * currentSpeed;
        finalMove.y = verticalVelocity;
        //Si le joueur est au sol et descend, on le stabilise
        controller.Move(finalMove * Time.deltaTime);
    }
    #endregion

    #region Input Functions

    /// <summary>
    /// Passe a la camera suivante dans la liste.
    /// </summary>
    /// <param name="ctx"></param>// Le contexte de l'action d'entrée.
    void OnNextCamera(InputAction.CallbackContext ctx)
    {
        currentCameraIndex = (currentCameraIndex + 1) % Cameras.Count;
        SwitchCamera(currentCameraIndex);
    }

    /// <summary>
    /// Passe a la camera precedente dans la liste.
    /// </summary>
    /// <param name="ctx"></param>// Le contexte de l'action d'entrée.
    void OnPrevCamera(InputAction.CallbackContext ctx)
    {
        currentCameraIndex--;
        if (currentCameraIndex < 0)
            currentCameraIndex = Cameras.Count - 1;

        SwitchCamera(currentCameraIndex);
    }

    /// <summary>
    /// Gere le saut du joueur.
    /// </summary>
    /// <param name="ctx"></param>// Le contexte de l'action d'entrée.
    void OnJump(InputAction.CallbackContext ctx)
    {
        if (!controller.isGrounded)
            return;

        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        animator.SetTrigger("jump");
    }

    /// <summary>
    /// Gere l'accroupissement du joueur.
    /// </summary>
    /// <param name="ctx"></param>// Le contexte de l'action d'entrée.
    void OnCrouch(InputAction.CallbackContext ctx)
    {
        speedMultiplier = 0.2f;
        Debug.Log("Crouch");
        animator.SetBool("iscrouching", true);
        animator.SetBool("isrunning", false);
    }

    /// <summary>
    /// Gere le deplacement du joueur en position debout.
    /// </summary>
    /// <param name="ctx"></param>// Le contexte de l'action d'entrée.
    void OnUncrouch(InputAction.CallbackContext ctx)
    {
        speedMultiplier = 1f;
        Debug.Log("Uncrouch");
        animator.SetBool("iscrouching", false);
    }

    /// <summary>
    /// Gere le sprint du joueur.
    /// </summary>
    /// <param name="ctx"></param>// Le contexte de l'action d'entrée.
    void OnSprint(InputAction.CallbackContext ctx)
    {
        if (animator.GetBool("iscrouching"))
            return;
        speedMultiplier = 2f;
        Debug.Log("Sprint");
        animator.SetBool("isrunning", true);
    }

    /// <summary>
    /// Gere l'arret du sprint du joueur.
    /// </summary>
    /// <param name="ctx"></param>// Le contexte de l'action d'entrée.
    void OnStopSprint(InputAction.CallbackContext ctx)
    {
        if (animator.GetBool("iscrouching"))
            return;
        speedMultiplier = 1f;
        Debug.Log("Stop Sprint");
        animator.SetBool("isrunning", false);
    }

    /// <summary>
    /// Gere l'interaction du joueur avec le bureau actuel.
    /// </summary>
    /// <param name="ctx"></param>// Le contexte de l'action d'entrée.
    void OnInteract(InputAction.CallbackContext ctx)
    {
        if (gm.currentdesk == null) return;

        gm.currentdesk.StartInteraction(this);
        animator.SetTrigger("interact");
    }

    #endregion

    #region Camera Management
    /// <summary>
    /// Active la camera a l'index specifie et desactive les autres.
    /// </summary>
    /// <param name="index"></param>// L'index de la camera a activer.
    void SwitchCamera(int index)
    {
        for (int i = 0; i < Cameras.Count; i++)
        {
            Cameras[i].gameObject.SetActive(i == index);
        }
    }
    #endregion

    /// <summary>
    /// Verifie si le joueur est en mouvement.
    /// </summary>
    /// <returns></returns>// Vrai si le joueur se deplace, sinon faux.
    public bool IsMoving()
    {
        if (Cameras == null || Cameras.Count == 0)
            return false;

        Vector2 input = MoveActionRef.action.ReadValue<Vector2>();
        return input.sqrMagnitude > 0.01f;
    }

    /// <summary>
    /// Gere la mise en pause et la reprise du jeu.
    /// </summary>
    /// <param name="ctx"></param>// Le contexte de l'action d'entrée.
    private void OnPause(InputAction.CallbackContext ctx)
    {
        bool isPaused = UIManager.Instance.endGameUI.activeSelf;

        if (isPaused)
            gm.ResumeGame();
        else
            gm.PauseGame();
    }
}
