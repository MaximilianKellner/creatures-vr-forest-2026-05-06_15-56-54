using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GroundPrey : Prey
{
    [Header("Movement")]
    [SerializeField] private float wanderRadius = 5f;
    [SerializeField] private float wanderInterval = 3f;
    [SerializeField] private float fleeDistance = 5f;
    [SerializeField] private float fleeRadius = 4f;

    [Header("References")]
    [SerializeField] private Transform playerTarget;

    private NavMeshAgent agent;
    private float timer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
                Debug.Log($"{name}: Player gefunden.");
            }
            else
            {
                Debug.LogError($"{name}: Kein Objekt mit Tag 'Player' gefunden!");
            }
        }
    }

    private void Start()
    {
        if (!agent.isOnNavMesh)
        {
            Debug.LogError($"{name}: Ist NICHT auf dem NavMesh!");
            enabled = false;
            return;
        }

        PickWanderTarget();
    }

    protected override void Move()
    {
        if (!agent.isOnNavMesh)
            return;

        if (playerTarget != null)
        {
            float distanceToPlayer = Vector3.Distance(
                transform.position,
                playerTarget.position
            );

            if (distanceToPlayer <= fleeDistance)
            {
                FleeFromPlayer();
                return;
            }
        }

        Wander();
    }

    private void Wander()
    {
        timer += Time.deltaTime;

        if (timer >= wanderInterval || !agent.hasPath || agent.remainingDistance <= 0.3f)
        {
            timer = 0f;
            PickWanderTarget();
        }
    }

    private void PickWanderTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(
            randomDirection,
            out NavMeshHit hit,
            wanderRadius,
            NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void FleeFromPlayer()
    {
        if (playerTarget == null)
            return;

        Vector3 fleeDirection = transform.position - playerTarget.position;
        fleeDirection.y = 0f;
        fleeDirection.Normalize();

        Vector3 fleeTarget = transform.position + fleeDirection * fleeRadius;

        if (NavMesh.SamplePosition(
            fleeTarget,
            out NavMeshHit hit,
            fleeRadius,
            NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}