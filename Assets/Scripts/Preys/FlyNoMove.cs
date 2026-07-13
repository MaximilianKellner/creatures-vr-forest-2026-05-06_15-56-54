using UnityEngine;

public class FlyNoMove : Prey
{
    [Header("Fly Movement")]
    [SerializeField] private float moveRadius = 2f;
    [SerializeField] private float moveSpeed = 2f;

    protected override void Move()
    {}
}