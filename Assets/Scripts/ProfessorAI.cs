using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ProfessorAI : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent agent;
    [Header("Vision")]
    public float viewDistance = 10f;
    public float viewAngle = 60f;
    public Vector3[] patrolPoints;
    private int currentIndex;
    public LayerMask obstacleMask;
    public GameObject player;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        Patrol();
        DetectPlayer();
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0)
            return;

        if (!agent.isOnNavMesh)
            return;

        if (agent.pathPending)
            return;

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            currentIndex = (currentIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentIndex]);
        }
    }

    void DetectPlayer()
    {
        if (player == null)
            return;

        Vector3 dirToPlayer = player.transform.position - transform.position;
        float distance = dirToPlayer.magnitude;

        if (distance > viewDistance)
            return;

        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        if (angle > viewAngle / 2f)
            return;

        if (Physics.Raycast(
            transform.position + Vector3.up,
            dirToPlayer.normalized,
            out RaycastHit hit,
            viewDistance,
            ~obstacleMask))
        {
            if (hit.transform == player)
            {
                Debug.Log("Joueur détecté !");
                GameOver();
            }
        }
    }

    void GameOver()
    {
        Time.timeScale = 0f;
        Debug.Log("Partie perdue !");
    }
}
