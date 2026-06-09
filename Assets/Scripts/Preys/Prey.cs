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

    private bool isCaptured;
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
    }

    protected virtual void Update()
    {
        if (isCaptured)
        {
            FollowTongue();
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
        followTarget = tongueTarget;
        player = tongueTarget.root;

        if (TryGetComponent(out Rigidbody rb))
            rb.isKinematic = true;

        if (TryGetComponent(out Collider col))
            col.enabled = false;
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

        if (Vector3.Distance(transform.position, player.position) < 0.5f)
        {
            PlayerHealth health =
                player.GetComponentInParent<PlayerHealth>() ??
                player.GetComponentInChildren<PlayerHealth>();

            if (health != null)
            {
                if (preyType == PreyType.Poison)
                {
                    health.TakeDamage(damage);
                }
                else if (preyType == PreyType.Normal)
                {
                    health.Heal(healAmount);
                }
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
}