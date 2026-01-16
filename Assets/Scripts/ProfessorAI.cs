using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ProfessorAI : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public GameObject player;

    [Header("Vision")]
    public float viewDistance = 10f;
    public float viewAngle = 120f;

    [Header("Patrol")]
    public Vector3[] patrolPoints;
    private int currentIndex = 0;

    [Header("Suspicion")]
    public float maxSuspicion = 100f;
    public float suspicionIncreaseRate = 40f;
    public float suspicionDecreaseRate = 20f;
    public float rotationSpeed = 5f;
    public float stopDuration = 0.5f;

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
        float dt = GameManager.Instance.DeltaTime;

        if (seesPlayer)
        {
            suspicion += suspicionIncreaseRate * dt;
            stopTimer = stopDuration;
        }
        else
        {
            suspicion -= suspicionDecreaseRate * dt;
        }

        suspicion = Mathf.Clamp(suspicion, 0f, maxSuspicion);

        if (suspicion >= maxSuspicion)
        {
            GameManager.Instance.PlayerSpotted();
        }
    }

    void HandleMovement(bool seesPlayer)
    {
        if (seesPlayer || stopTimer > 0f)
        {
            agent.isStopped = true;
            stopTimer -= GameManager.Instance.DeltaTime;
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
            rotationSpeed * GameManager.Instance.DeltaTime
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
