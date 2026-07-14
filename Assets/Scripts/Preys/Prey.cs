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
    private CharacterController playerController;
    private PlayerHealth playerHealth;
    private UpgradeSystem playerUpgradeSystem;

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
        ResolvePlayerContext(tongueTarget);

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

        if (playerController != null)
        {
            closestPoint = playerController.ClosestPoint(transform.position);
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

        if (IsPlayerCollider(other))
        {
            Eat();
        }
    }

    private void Eat()
    {
        if (player == null) return;

        if (playerHealth == null || playerUpgradeSystem == null)
            ResolvePlayerContext(followTarget);

        if (playerHealth != null)
        {
            if (preyType == PreyType.Poison)
                playerHealth.TakeDamage(damage);
            else
                playerHealth.Heal(healAmount);
        }

        if (playerUpgradeSystem != null)
        {
            playerUpgradeSystem.UnlockUpgrade(givesUpgrade, upgradeLevel);
        }
        
        MissionTarget missionTarget = GetComponent<MissionTarget>();

        if (missionTarget != null)
        {
            MissionManager.ReportTargetCollected(missionTarget.TargetId);
        }
        
        Destroy(gameObject);
    }

    private void ResolvePlayerContext(Transform context)
    {
        player = null;
        playerController = null;
        playerHealth = null;
        playerUpgradeSystem = null;

        if (context == null)
            return;

        playerController = context.GetComponentInParent<CharacterController>();
        playerHealth = context.GetComponentInParent<PlayerHealth>();
        playerUpgradeSystem = context.GetComponentInParent<UpgradeSystem>();

        Transform root = context.root;

        if (root != null)
        {
            if (playerController == null)
                playerController = root.GetComponentInChildren<CharacterController>();

            if (playerHealth == null)
                playerHealth = root.GetComponentInChildren<PlayerHealth>();

            if (playerUpgradeSystem == null)
                playerUpgradeSystem = root.GetComponentInChildren<UpgradeSystem>();
        }

        if (playerController != null)
            player = playerController.transform;
        else if (playerHealth != null)
            player = playerHealth.transform;
        else if (playerUpgradeSystem != null)
            player = playerUpgradeSystem.transform;
        else
            player = root;
    }

    private bool IsPlayerCollider(Collider other)
    {
        return VRUIRuntimeSupport.IsPlayerCollider(other);
    }
}
