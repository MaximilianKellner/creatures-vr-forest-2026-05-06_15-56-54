using UnityEngine;

public enum PreyType
{
    Normal,
    Poison
}

public abstract class Prey : MonoBehaviour
{
    [Header("Prey Settings")]
    [SerializeField] private PreyType preyType = PreyType.Normal;
    [SerializeField] private int damage;
    [SerializeField] private int healAmount;

    [Header("Upgrade Settings")]
    [SerializeField] private PreyGivesUpgrade givesUpgrade = PreyGivesUpgrade.None;
    [SerializeField] private int upgradeLevel = 1;

    [Header("Eat Settings")]
    [SerializeField] private float eatDistance = 1.2f;

    private bool isCaptured;
    private bool canBeEaten;
    private Transform followTarget;
    private Transform player;

    public bool IsCaptured => isCaptured;
    public PreyType Type => preyType;
    public int Damage => damage;
    public int HealAmount => healAmount;

    private void OnValidate()
    {
        if (preyType == PreyType.Normal)
        {
            damage = 0;
        }
        else if (preyType == PreyType.Poison)
        {
            healAmount = 0;
        }

        if (upgradeLevel < 1)
            upgradeLevel = 1;

        if (eatDistance < 0.1f)
            eatDistance = 0.1f;
    }

    protected virtual void Update()
    {
        if (isCaptured)
        {
            FollowTongue();

            if (canBeEaten)
                CheckAutoEat();

            return;
        }

        Move();
    }

    protected abstract void Move();

    public void AttachToTongue(Transform tongueTarget)
    {
        if (isCaptured) return;

        isCaptured = true;
        canBeEaten = false;
        followTarget = tongueTarget;
        player = tongueTarget.root;

        if (TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (TryGetComponent(out Collider col))
        {
            col.enabled = true;
            col.isTrigger = true;
        }
    }

    public void AllowEat()
    {
        canBeEaten = true;
    }

    private void FollowTongue()
    {
        if (followTarget == null) return;

        transform.position = Vector3.Lerp(
            transform.position,
            followTarget.position,
            Time.deltaTime * 15f
        );
    }

    private void CheckAutoEat()
    {
        if (player == null) return;

        Vector3 closestPoint = transform.position;

        CharacterController controller =
            player.GetComponentInParent<CharacterController>() ??
            player.GetComponentInChildren<CharacterController>();

        if (controller != null)
        {
            closestPoint = controller.ClosestPoint(transform.position);
        }

        float distance = Vector3.Distance(transform.position, closestPoint);

        if (distance <= eatDistance)
        {
            Eat();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isCaptured || !canBeEaten)
            return;

        if (other.CompareTag("Player") || other.GetComponentInParent<PlayerHealth>() != null)
        {
            Eat();
        }
    }

    private void Eat()
    {
        if (player == null) return;

        PlayerHealth health =
            player.GetComponentInParent<PlayerHealth>() ??
            player.GetComponentInChildren<PlayerHealth>();

        if (health != null)
        {
            if (preyType == PreyType.Poison)
                health.TakeDamage(damage);
            else
                health.Heal(healAmount);
        }

        UpgradeSystem upgradeSystem =
            player.GetComponentInParent<UpgradeSystem>() ??
            player.GetComponentInChildren<UpgradeSystem>();

        if (upgradeSystem != null)
        {
            upgradeSystem.UnlockUpgrade(givesUpgrade, upgradeLevel);
        }
        Destroy(gameObject);
    }
}