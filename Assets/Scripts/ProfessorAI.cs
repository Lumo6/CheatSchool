using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Classe ProfessorAI
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class ProfessorAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;// Reference to the NavMeshAgent component
    public GameObject player;// Reference to the player GameObject

    [Header("Vision")]
    [SerializeField] private float viewDistance = 10f;// Distance the professor can see
    [SerializeField] private float viewAngle = 120f;// Angle of the professor's field of view

    [Header("Patrol")]
    public Vector3[] patrolPoints;// Points the professor will patrol between
    private int currentIndex = 0;// Current index in the patrol points array

    [Header("Suspicion")]
    [SerializeField] private float maxSuspicion = 100f;// Maximum suspicion level
    [SerializeField] private float suspicionIncreaseRate = 20f;// Rate at which suspicion increases when the player is seen
    [SerializeField] private float suspicionDecreaseRate = 10f;// Rate at which suspicion decreases when the player is not seen
    [SerializeField] private float rotationSpeed = 5f;// Speed at which the professor rotates to face the player
    [SerializeField] private float stopDuration = 0.5f;// Duration the professor stops when seeing the player

    [SerializeField] private AudioClip catchSoundClip;// Sound played when the player is caught

    private float suspicion = 0f;// Current suspicion level
    private float stopTimer = 0f;// Timer for how long the professor stops


    void Awake()
    {
        // Initialize references and settings
        agent = GetComponent<NavMeshAgent>();
        DifficultySettings ds = GameManager.Instance.currentDifficultySettings;
        suspicionIncreaseRate = ds.suspicionIncreaseRate;
        suspicionDecreaseRate = ds.suspicionDecreaseRate;
        stopDuration = ds.stopDuration;
        viewAngle = ds.viewAngle;
        viewDistance = ds.viewDistance;
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState == GameManager.GameState.Spotted_GameOver)
            return;

        bool seesPlayer = DetectPlayer();

        UpdateSuspicion(seesPlayer);
        HandleMovement(seesPlayer);
    }

    /// <summary>
    /// Patrol between the defined patrol points.
    /// </summary>
    void Patrol()
    {
        if (patrolPoints.Length == 0 || !agent.isOnNavMesh)
            return;

        if (agent.pathPending)
            return;

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            // Choose a new random index, different from the current one
            int newIndex;
            do
            {
                newIndex = Random.Range(0, patrolPoints.Length);
            } while (newIndex == currentIndex);

            currentIndex = newIndex;
            agent.SetDestination(patrolPoints[currentIndex]);
        }
    }

    /// <summary>
    /// Detect if the player is within the professor's field of view.
    /// </summary>
    /// <returns></returns>// True if the player is detected, false otherwise.
    bool DetectPlayer()
    {
        if (player == null)
            return false;
        // Calculate direction and distance to player
        Vector3 dirToPlayer = player.transform.position - transform.position;
        float distance = dirToPlayer.magnitude;
        // Check if player is within view distance
        if (distance > viewDistance)
            return false;
        // Check if player is within the view angle
        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > viewAngle / 2f)
            return false;
        // Raycast to check for obstacles between professor and player
        if (Physics.Raycast(
            transform.position + new Vector3(0,0.5f,0),
            dirToPlayer.normalized,
            out RaycastHit hit,
            viewDistance))
        {
            return hit.transform == player.transform;
        }

        return false;
    }

    /// <summary>
    /// Update the suspicion level based on whether the player is seen.
    /// </summary>
    /// <param name="seesPlayer"></param>// True if the player is seen, false otherwise.
    void UpdateSuspicion(bool seesPlayer)
    {
        // Increase or decrease suspicion based on player visibility
        if (seesPlayer)
        {
            suspicion += suspicionIncreaseRate * Time.deltaTime;
            stopTimer = stopDuration;
        }
        else
        {
            suspicion -= suspicionDecreaseRate * Time.deltaTime;
        }

        suspicion = Mathf.Clamp(suspicion, 0f, maxSuspicion);
        // Check if suspicion has reached the maximum level
        if (suspicion >= maxSuspicion)
        {
            SoundFXManager.Instance.PlaySound(catchSoundClip, this.transform);
            GameManager.Instance.PlayerSpotted();
        }
        UIManager.Instance.updateSuspicionProgressUI(suspicion / maxSuspicion);
    }

    /// <summary>
    /// Handle the movement of the professor based on player visibility.
    /// </summary>
    /// <param name="seesPlayer"></param>// True if the player is seen, false otherwise.
    void HandleMovement(bool seesPlayer)
    {
        if (seesPlayer || stopTimer > 0f)
        {
            agent.isStopped = true;
            stopTimer -= Time.deltaTime;
            RotateTowardsPlayer();
        }
        else
        {
            agent.isStopped = false;
            Patrol();
        }
    }

    /// <summary>
    /// Rotate the professor to face the player.
    /// </summary>
    void RotateTowardsPlayer()
    {
        if (player == null)
            return;
        // Calculate direction to player
        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0f;
        // Rotate smoothly towards the player
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
    /// <summary>
    /// Get the current suspicion level.
    /// </summary>
    /// <returns></returns>// The current suspicion level.
    public float GetSuspicion()
    {
        return suspicion;
    }
    /// <summary>
    /// Get the maximum suspicion level.
    /// </summary>
    /// <returns></returns>// The maximum suspicion level.
    public float GetMaxSuspicion()
    {
        return maxSuspicion;
    }
}
