using UnityEngine;

public class Fly : Prey
{
    [Header("Fly Movement")]
    [SerializeField] private float moveRadius = 2f;
    [SerializeField] private float moveSpeed = 2f;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    private void Start()
    {
        startPosition = transform.position;
        PickNewTarget();
    }

    protected override void Move()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            PickNewTarget();
        }
    }

    private void PickNewTarget()
    {
        targetPosition = startPosition + Random.insideUnitSphere * moveRadius;
    }
}