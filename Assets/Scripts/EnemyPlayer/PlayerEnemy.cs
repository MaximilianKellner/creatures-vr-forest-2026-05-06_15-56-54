using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerEnemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private EnemyPatrolPath patrolPath;
    [SerializeField] private Transform tongueOrigin;
    [SerializeField] private Transform tongueTip;
    [SerializeField] private LineRenderer tongueLine;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 1.2f;
    [SerializeField] private float chaseSpeed = 2f;
    [SerializeField] private float detectionRange = 12f;
    [SerializeField] private float stopDistance = 5f;
    [SerializeField] private float waypointReachedDistance = 0.6f;

    [Header("Tongue Attack")]
    [SerializeField] private float attackRange = 7f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float tongueSpeed = 18f;
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask attackMask;

    [Header("Audio")]
    [SerializeField] private AudioSource enemyAlertSound;

    private bool isChasing;

    private NavMeshAgent agent;
    private int currentWaypointIndex;
    private bool isAttacking;
    private float nextAttackTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (tongueLine != null)
        {
            tongueLine.enabled = false;
            tongueLine.positionCount = 0;
        }
    }

    private void Start()
    {
        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0f;

        GoToCurrentWaypoint();
    }

    private void Update()
    {
        if (player == null || agent == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer(distanceToPlayer);
        }
        else
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        if (isChasing)
        {
            isChasing = false;
        }
        
        if (isAttacking)
            return;

        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0f;

        if (patrolPath == null || patrolPath.Count == 0)
            return;

        if (!agent.pathPending && agent.remainingDistance <= waypointReachedDistance)
        {
            currentWaypointIndex = patrolPath.GetNextIndex(currentWaypointIndex);
            GoToCurrentWaypoint();
        }
    }

    private void ChasePlayer(float distanceToPlayer)
    {
        if (isAttacking)
            return;

        if (!isChasing)
        {
            isChasing = true;
            enemyAlertSound.PlayOneShot(enemyAlertSound.clip);
        }

        agent.speed = chaseSpeed;
        agent.stoppingDistance = stopDistance;

        if (distanceToPlayer > stopDistance)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            agent.ResetPath();
            LookAtPlayer();
        }

        if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
        {
            StartCoroutine(TongueAttack());
        }
    }

    private void GoToCurrentWaypoint()
    {
        if (patrolPath == null || patrolPath.Count == 0)
            return;

        Transform waypoint = patrolPath.GetWaypoint(currentWaypointIndex);

        if (waypoint != null)
            agent.SetDestination(waypoint.position);
    }

    private void LookAtPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
            return;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direction),
            Time.deltaTime * 5f
        );
    }

    private IEnumerator TongueAttack()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        agent.ResetPath();
        LookAtPlayer();

        if (tongueLine == null || tongueOrigin == null || tongueTip == null)
        {
            isAttacking = false;
            yield break;
        }

        PlayerHealth targetHealth =
            player.GetComponentInParent<PlayerHealth>() ??
            player.GetComponentInChildren<PlayerHealth>();

        if (targetHealth == null)
        {
            isAttacking = false;
            yield break;
        }

        tongueLine.enabled = true;
        tongueLine.positionCount = 2;

        Vector3 startPos = tongueOrigin.position;
        Vector3 targetPos = targetHealth.transform.position + Vector3.up * 1f;

        float distance = Vector3.Distance(startPos, targetPos);

        if (distance <= 0.01f)
        {
            tongueLine.enabled = false;
            tongueLine.positionCount = 0;
            isAttacking = false;
            yield break;
        }

        bool damageApplied = false;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * tongueSpeed / distance;

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);

            tongueLine.SetPosition(0, tongueOrigin.position);
            tongueLine.SetPosition(1, currentPos);

            Vector3 rayDirection = currentPos - tongueOrigin.position;
            float rayDistance = rayDirection.magnitude;

            if (rayDistance > 0.01f)
            {
                if (Physics.Raycast(
                    tongueOrigin.position,
                    rayDirection.normalized,
                    out RaycastHit hit,
                    rayDistance,
                    attackMask))
                {
                    PlayerHealth hitHealth =
                        hit.collider.GetComponentInParent<PlayerHealth>() ??
                        hit.collider.GetComponentInChildren<PlayerHealth>();

                    if (hitHealth == targetHealth && !damageApplied)
                    {
                        hitHealth.TakeDamage(damage);
                        damageApplied = true;
                        break;
                    }
                }
            }

            yield return null;
        }

        float backT = 1f;

        while (backT > 0f)
        {
            backT -= Time.deltaTime * tongueSpeed / distance;

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, backT);

            tongueLine.SetPosition(0, tongueOrigin.position);
            tongueLine.SetPosition(1, currentPos);

            yield return null;
        }

        tongueLine.enabled = false;
        tongueLine.positionCount = 0;

        isAttacking = false;
    }
}