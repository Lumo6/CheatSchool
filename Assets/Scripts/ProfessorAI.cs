using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ProfessorAI : MonoBehaviour
{
    [Header("Navigation")]
    public NavMeshAgent agent;
    public List<Transform> patrolPoints = new();
    int currentPatrolIndex;

    [Header("Vision")]
    public float viewDistance = 10f;
    public float viewAngle = 90f;
    public LayerMask obstacleMask;
    public LayerMask playerMask;

    [Header("Suspicion")]
    public float timeToCatch = 2.5f;
    public float suspicionDecreaseSpeed = 1.5f;
    float suspicionTimer = 0f;

    [Header("Reaction")]
    public float pauseWhenHidden = 1.2f;
    bool isPaused;

    [Header("UI")]
    public SuspicionMeterUI suspicionUI;


    Transform player;
    GameManager gm;

    enum State { Patrol, Suspicious }
    State currentState = State.Patrol;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        gm = GameManager.Instance;

        GoToNextPatrolPoint();
    }

    void Update()
    {
        if (isPaused) return;

        Patrol();
        DetectPlayer();
    }

    #region PATROL
    void Patrol()
    {
        if (patrolPoints.Count == 0) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            GoToNextPatrolPoint();
    }

    void GoToNextPatrolPoint()
    {
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
    }
    #endregion

    #region DETECTION
    void DetectPlayer()
    {
        Vector3 dir = player.position - transform.position;
        float distance = dir.magnitude;

        if (distance > viewDistance)
        {
            ReduceSuspicion();
            return;
        }

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > viewAngle * 0.5f)
        {
            ReduceSuspicion();
            return;
        }

        Ray ray = new Ray(transform.position + Vector3.up * 1.6f, dir.normalized);

        if (Physics.Raycast(ray, out RaycastHit hit, viewDistance, obstacleMask | playerMask))
        {
            if (hit.collider.CompareTag("Player"))
            {
                IncreaseSuspicion();
            }
            else
            {
                // Player was hidden by desk
                ReduceSuspicion();
                PauseBriefly();
            }
        }
    }

    void IncreaseSuspicion()
    {
        // Only suspicious if player is copying
        if (!gm.IsCopying())
        {
            ReduceSuspicion();
            return;
        }

        suspicionTimer += Time.deltaTime;
        currentState = State.Suspicious;

        if (suspicionTimer >= timeToCatch)
        {
            CatchPlayer();
        }
    }

    void ReduceSuspicion()
    {
        suspicionTimer -= Time.deltaTime * suspicionDecreaseSpeed;
        suspicionTimer = Mathf.Max(0, suspicionTimer);

        if (suspicionTimer == 0)
            currentState = State.Patrol;
    }

    void PauseBriefly()
    {
        if (!isPaused)
            StartCoroutine(PauseCoroutine());
    }

    System.Collections.IEnumerator PauseCoroutine()
    {
        isPaused = true;
        agent.isStopped = true;
        yield return new WaitForSeconds(pauseWhenHidden);
        agent.isStopped = false;
        isPaused = false;
    }
    #endregion

    void CatchPlayer()
    {
        Debug.Log("Player caught cheating!");
        Time.timeScale = 0f;
    }

    #region PUBLIC API
    public void SetPatrolPoints(List<Transform> points)
    {
        patrolPoints = points;
        currentPatrolIndex = 0;
        GoToNextPatrolPoint();
    }

    public void SetDifficulty(LevelGenerator.Difficulty difficulty)
    {
        switch (difficulty)
        {
            case LevelGenerator.Difficulty.Easy:
                agent.speed = 2.0f;
                timeToCatch = 3.5f;
                break;

            case LevelGenerator.Difficulty.Medium:
                agent.speed = 3.5f;
                timeToCatch = 2.5f;
                break;

            case LevelGenerator.Difficulty.Hard:
                agent.speed = 5.0f;
                timeToCatch = 1.5f;
                break;
        }
    }
    #endregion

    #region DEBUG
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 left = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, left * viewDistance);
        Gizmos.DrawRay(transform.position, right * viewDistance);
    }
    #endregion
}
