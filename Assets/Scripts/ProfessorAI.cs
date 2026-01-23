using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ProfessorAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    public GameObject player;

    [Header("Vision")]
    [SerializeField] private float viewDistance = 10f;
    [SerializeField] private float viewAngle = 120f;

    [Header("Patrol")]
    public Vector3[] patrolPoints;
    private int currentIndex = 0;

    [Header("Suspicion")]
    [SerializeField] private float maxSuspicion = 100f;
    [SerializeField] private float suspicionIncreaseRate = 40f;
    [SerializeField] private float suspicionDecreaseRate = 20f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float stopDuration = 0.5f;

    [SerializeField] private AudioClip catchSoundClip;

    private float suspicion = 0f;
    private float stopTimer = 0f;


    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState == GameManager.GameState.Spotted_GameOver)
            return;

        bool seesPlayer = DetectPlayer();

        UpdateSuspicion(seesPlayer);
        HandleMovement(seesPlayer);
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0 || !agent.isOnNavMesh)
            return;

        if (agent.pathPending)
            return;

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.SetDestination(patrolPoints[currentIndex]);
            currentIndex = (currentIndex + 1) % patrolPoints.Length;
        }
    }

    bool DetectPlayer()
    {
        if (player == null)
            return false;

        Vector3 dirToPlayer = player.transform.position - transform.position;
        float distance = dirToPlayer.magnitude;

        if (distance > viewDistance)
            return false;

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > viewAngle / 2f)
            return false;

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

    void UpdateSuspicion(bool seesPlayer)
    {

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

        if (suspicion >= maxSuspicion)
        {
            SoundFXManager.Instance.PlaySound(catchSoundClip, this.transform);
            GameManager.Instance.PlayerSpotted();
        }
        UIManager.Instance.updateSuspicionProgressUI(suspicion / maxSuspicion);
    }

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

    void RotateTowardsPlayer()
    {
        if (player == null)
            return;

        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0f;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    public float GetSuspicion()
    {
        return suspicion;
    }
    public float GetMaxSuspicion()
    {
        return maxSuspicion;
    }
}
