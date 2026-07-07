using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class VRAbilityProvider : MonoBehaviour
{
    enum AbilityKind
    {
        None,
        NightVision,
        Sonar,
        Scent,
        Hearing
    }

    [Serializable]
    class TouchpadAbilitySlot
    {
        [SerializeField]
        AbilityKind ability;

        public AbilityKind Ability => ability;
    }

    [Header("Input")]
    [SerializeField]
    InputActionReference rightTouchpadAxis;

    [SerializeField]
    InputActionReference rightTouchpadClick;

    [SerializeField, Range(0.1f, 1f)]
    float directionThreshold = 0.45f;

    [Header("Right Touchpad Mapping")]
    [SerializeField]
    TouchpadAbilitySlot up = new TouchpadAbilitySlot();

    [SerializeField]
    TouchpadAbilitySlot right = new TouchpadAbilitySlot();

    [SerializeField]
    TouchpadAbilitySlot down = new TouchpadAbilitySlot();

    [SerializeField]
    TouchpadAbilitySlot left = new TouchpadAbilitySlot();

    [Header("Abilities")]
    [SerializeField]
    NightVision nightVision;

    [SerializeField]
    SonarSense sonarSense;

    [SerializeField]
    ScentSense scentSense;

    [SerializeField]
    HearingSense hearingSense;

    [Header("Optional Jump Unlock Gate")]
    [SerializeField]
    Behaviour jumpProvider;

    [SerializeField]
    bool gateJumpByUpgrade;

    [SerializeField]
    PreyGivesUpgrade jumpUpgrade = PreyGivesUpgrade.HigherJump;

    UpgradeSystem m_UpgradeSystem;

    void Awake()
    {
        m_UpgradeSystem =
            GetComponentInParent<UpgradeSystem>() ??
            GetComponentInChildren<UpgradeSystem>();

        if (nightVision == null)
            nightVision = GetComponentInParent<NightVision>() ?? GetComponentInChildren<NightVision>();

        if (sonarSense == null)
            sonarSense = GetComponentInParent<SonarSense>() ?? GetComponentInChildren<SonarSense>();

        if (scentSense == null)
            scentSense = GetComponentInParent<ScentSense>() ?? GetComponentInChildren<ScentSense>();

        if (hearingSense == null)
            hearingSense = GetComponentInParent<HearingSense>() ?? GetComponentInChildren<HearingSense>();
    }

    void OnEnable()
    {
        EnableAction(rightTouchpadAxis);
        Bind(rightTouchpadClick, OnRightTouchpadClicked);
        UpdateJumpProviderState();
    }

    void OnDisable()
    {
        Unbind(rightTouchpadClick, OnRightTouchpadClicked);
    }

    void Update()
    {
        UpdateJumpProviderState();
    }

    void OnRightTouchpadClicked(InputAction.CallbackContext context)
    {
        var axis = ReadVector2(rightTouchpadAxis);
        if (axis.sqrMagnitude < directionThreshold * directionThreshold)
            return;

        var slot = GetSlot(axis);
        UseAbility(slot.Ability);
    }

    TouchpadAbilitySlot GetSlot(Vector2 axis)
    {
        if (Mathf.Abs(axis.x) > Mathf.Abs(axis.y))
            return axis.x > 0f ? right : left;

        return axis.y > 0f ? up : down;
    }

    void UseAbility(AbilityKind ability)
    {
        switch (ability)
        {
            case AbilityKind.NightVision:
                if (nightVision != null)
                    nightVision.TryUseNightVision();
                break;

            case AbilityKind.Sonar:
                if (sonarSense != null)
                    sonarSense.TryUseSonar();
                break;

            case AbilityKind.Scent:
                if (scentSense != null)
                    scentSense.TryUseScentSense();
                break;

            case AbilityKind.Hearing:
                if (hearingSense != null)
                    hearingSense.TryUseHearing();
                break;
        }
    }

    void UpdateJumpProviderState()
    {
        if (!gateJumpByUpgrade || jumpProvider == null)
            return;

        jumpProvider.enabled = m_UpgradeSystem != null && m_UpgradeSystem.HasUpgrade(jumpUpgrade);
    }

    static Vector2 ReadVector2(InputActionReference actionReference)
    {
        return actionReference != null && actionReference.action != null
            ? actionReference.action.ReadValue<Vector2>()
            : Vector2.zero;
    }

    static void Bind(InputActionReference actionReference, Action<InputAction.CallbackContext> callback)
    {
        var action = GetAction(actionReference);
        if (action == null)
            return;

        action.performed += callback;
        action.Enable();
    }

    static void Unbind(InputActionReference actionReference, Action<InputAction.CallbackContext> callback)
    {
        var action = GetAction(actionReference);
        if (action == null)
            return;

        action.performed -= callback;
    }

    static void EnableAction(InputActionReference actionReference)
    {
        GetAction(actionReference)?.Enable();
    }

    static InputAction GetAction(InputActionReference actionReference)
    {
        return actionReference != null ? actionReference.action : null;
    }
}
