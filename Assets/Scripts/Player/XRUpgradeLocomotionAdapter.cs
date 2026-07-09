using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Jump;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

public class XRUpgradeLocomotionAdapter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UpgradeSystem upgradeSystem;
    [SerializeField] private ContinuousMoveProvider[] moveProviders;
    [SerializeField] private JumpProvider[] jumpProviders;

    [Header("Speed Upgrades")]
    [SerializeField] private float speedBonusPerLevel = 0.2f;
    [SerializeField] private float sprintMultiplier = 2f;

    [Header("Jump Upgrades")]
    [SerializeField] private float jumpBonusPerLevel = 0.2f;

    [Header("Sprint Input")]
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private bool useSprintAsPassiveBoostWhenNoInput = true;

    private float[] baseMoveSpeeds;
    private float[] baseJumpHeights;

    private void Awake()
    {
        ResolveReferences();
        CacheBaseMoveSpeeds();
        CacheBaseJumpHeights();
    }

    private void OnEnable()
    {
        if (sprintAction != null && sprintAction.action != null)
            sprintAction.action.Enable();

        ApplyUpgrades();
    }

    private void Update()
    {
        ApplyUpgrades();
    }

    private void ResolveReferences()
    {
        if (upgradeSystem == null)
            upgradeSystem = GetComponentInParent<UpgradeSystem>() ??
                            GetComponentInChildren<UpgradeSystem>();

        if (moveProviders == null || moveProviders.Length == 0)
            moveProviders = GetComponentsInChildren<ContinuousMoveProvider>(true);

        if (jumpProviders == null || jumpProviders.Length == 0)
            jumpProviders = GetComponentsInChildren<JumpProvider>(true);
    }

    private void CacheBaseMoveSpeeds()
    {
        ResolveReferences();

        if (moveProviders == null)
        {
            baseMoveSpeeds = null;
            return;
        }

        baseMoveSpeeds = new float[moveProviders.Length];

        for (int i = 0; i < moveProviders.Length; i++)
        {
            baseMoveSpeeds[i] = moveProviders[i] != null
                ? moveProviders[i].moveSpeed
                : 0f;
        }
    }

    private void CacheBaseJumpHeights()
    {
        ResolveReferences();

        if (jumpProviders == null)
        {
            baseJumpHeights = null;
            return;
        }

        baseJumpHeights = new float[jumpProviders.Length];

        for (int i = 0; i < jumpProviders.Length; i++)
        {
            baseJumpHeights[i] = jumpProviders[i] != null
                ? jumpProviders[i].jumpHeight
                : 0f;
        }
    }

    private void ApplyUpgrades()
    {
        ResolveReferences();

        if (upgradeSystem == null)
            return;

        if (moveProviders != null && moveProviders.Length > 0 &&
            (baseMoveSpeeds == null || baseMoveSpeeds.Length != moveProviders.Length))
            CacheBaseMoveSpeeds();

        if (jumpProviders != null && jumpProviders.Length > 0 &&
            (baseJumpHeights == null || baseJumpHeights.Length != jumpProviders.Length))
            CacheBaseJumpHeights();

        ApplySpeedUpgrades();
        ApplyJumpUpgrades();
    }

    private void ApplySpeedUpgrades()
    {
        if (moveProviders == null || moveProviders.Length == 0)
            return;

        float multiplier = 1f;

        int speedLevel = upgradeSystem.GetUpgradeLevel(PreyGivesUpgrade.FasterRun);
        if (speedLevel > 0)
            multiplier += speedLevel * speedBonusPerLevel;

        if (upgradeSystem.HasUpgrade(PreyGivesUpgrade.Sprint) && IsSprintActive())
            multiplier *= sprintMultiplier;

        for (int i = 0; i < moveProviders.Length; i++)
        {
            if (moveProviders[i] == null)
                continue;

            float baseSpeed = i < baseMoveSpeeds.Length ? baseMoveSpeeds[i] : moveProviders[i].moveSpeed;
            moveProviders[i].moveSpeed = baseSpeed * multiplier;
        }
    }

    private void ApplyJumpUpgrades()
    {
        if (jumpProviders == null || jumpProviders.Length == 0)
            return;

        int jumpLevel = upgradeSystem.GetUpgradeLevel(PreyGivesUpgrade.HigherJump);
        float multiplier = 1f + jumpLevel * jumpBonusPerLevel;

        for (int i = 0; i < jumpProviders.Length; i++)
        {
            if (jumpProviders[i] == null)
                continue;

            float baseHeight = i < baseJumpHeights.Length ? baseJumpHeights[i] : jumpProviders[i].jumpHeight;
            jumpProviders[i].jumpHeight = baseHeight * multiplier;
        }
    }

    private bool IsSprintActive()
    {
        if (sprintAction != null && sprintAction.action != null)
            return sprintAction.action.IsPressed();

        return useSprintAsPassiveBoostWhenNoInput;
    }
}
