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
        [SerializeField] private AbilityKind ability;

        public AbilityKind Ability => ability;
    }

    [Header("Input")]
    [SerializeField] private InputActionReference rightTouchpadAxis;
    [SerializeField] private InputActionReference rightTouchpadClick;
    [SerializeField, Range(0.1f, 1f)] private float directionThreshold = 0.45f;

    [Header("Right Touchpad Mapping")]
    [SerializeField] private TouchpadAbilitySlot up = new TouchpadAbilitySlot();
    [SerializeField] private TouchpadAbilitySlot right = new TouchpadAbilitySlot();
    [SerializeField] private TouchpadAbilitySlot down = new TouchpadAbilitySlot();
    [SerializeField] private TouchpadAbilitySlot left = new TouchpadAbilitySlot();

    [Header("Abilities")]
    [SerializeField] private NightVision nightVision;
    [SerializeField] private SonarSense sonarSense;
    [SerializeField] private ScentSense scentSense;
    [SerializeField] private HearingSense hearingSense;

    [Header("Optional Jump Unlock Gate")]
    [SerializeField] private Behaviour jumpProvider;
    [SerializeField] private bool gateJumpByUpgrade;
    [SerializeField] private PreyGivesUpgrade jumpUpgrade = PreyGivesUpgrade.HigherJump;

    private UpgradeSystem upgradeSystem;
    private InputAction fallbackAxisAction;
    private InputAction fallbackClickAction;
    private bool usingFallbackClickAction;

    private void Awake()
    {
        upgradeSystem =
            GetComponentInParent<UpgradeSystem>() ??
            GetComponentInChildren<UpgradeSystem>();

        if (nightVision == null)
            nightVision = GetComponentInParent<NightVision>() ??
                          GetComponentInChildren<NightVision>();

        if (sonarSense == null)
            sonarSense = GetComponentInParent<SonarSense>() ??
                         GetComponentInChildren<SonarSense>();

        if (scentSense == null)
            scentSense = GetComponentInParent<ScentSense>() ??
                         GetComponentInChildren<ScentSense>();

        if (hearingSense == null)
            hearingSense = GetComponentInParent<HearingSense>() ??
                           GetComponentInChildren<HearingSense>();

        CreateFallbackActions();
    }

    private void OnEnable()
    {
        EnableAction(rightTouchpadAxis);
        fallbackAxisAction?.Enable();

        usingFallbackClickAction = !Bind(rightTouchpadClick, OnRightTouchpadClicked);
        if (usingFallbackClickAction && fallbackClickAction != null)
        {
            fallbackClickAction.performed += OnRightTouchpadClicked;
            fallbackClickAction.Enable();
        }

        UpdateJumpProviderState();
    }

    private void OnDisable()
    {
        Unbind(rightTouchpadClick, OnRightTouchpadClicked);

        if (usingFallbackClickAction && fallbackClickAction != null)
        {
            fallbackClickAction.performed -= OnRightTouchpadClicked;
            fallbackClickAction.Disable();
        }

        fallbackAxisAction?.Disable();
    }

    private void Update()
    {
        UpdateJumpProviderState();
    }

    private void OnRightTouchpadClicked(InputAction.CallbackContext context)
    {
        Vector2 axis = ReadAbilityAxis();
        if (axis.sqrMagnitude < directionThreshold * directionThreshold)
            return;

        TouchpadAbilitySlot slot = GetSlot(axis);
        UseAbility(slot.Ability);
    }

    private Vector2 ReadAbilityAxis()
    {
        if (TryReadVector2(rightTouchpadAxis, out Vector2 axis) &&
            axis.sqrMagnitude > 0.001f)
            return axis;

        return fallbackAxisAction != null
            ? fallbackAxisAction.ReadValue<Vector2>()
            : Vector2.zero;
    }

    private TouchpadAbilitySlot GetSlot(Vector2 axis)
    {
        if (Mathf.Abs(axis.x) > Mathf.Abs(axis.y))
            return axis.x > 0f ? right : left;

        return axis.y > 0f ? up : down;
    }

    private void UseAbility(AbilityKind ability)
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

    private void UpdateJumpProviderState()
    {
        if (!gateJumpByUpgrade || jumpProvider == null)
            return;

        jumpProvider.enabled =
            upgradeSystem != null &&
            upgradeSystem.HasUpgrade(jumpUpgrade);
    }

    private void CreateFallbackActions()
    {
        fallbackAxisAction = new InputAction(
            "Abilities Axis Fallback",
            InputActionType.Value,
            expectedControlType: "Vector2");
        fallbackAxisAction.AddBinding("<XRController>{RightHand}/{Primary2DAxis}");

        fallbackClickAction = new InputAction(
            "Abilities Click Fallback",
            InputActionType.Button);
        fallbackClickAction.AddBinding("<XRController>{RightHand}/{Primary2DAxisClick}");
    }

    private static bool TryReadVector2(
        InputActionReference actionReference,
        out Vector2 value)
    {
        value = Vector2.zero;

        if (actionReference == null || actionReference.action == null)
            return false;

        try
        {
            value = actionReference.action.ReadValue<Vector2>();
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private static bool Bind(
        InputActionReference actionReference,
        Action<InputAction.CallbackContext> callback)
    {
        InputAction action = GetAction(actionReference);
        if (action == null)
            return false;

        action.performed += callback;
        action.Enable();
        return true;
    }

    private static void Unbind(
        InputActionReference actionReference,
        Action<InputAction.CallbackContext> callback)
    {
        InputAction action = GetAction(actionReference);
        if (action == null)
            return;

        action.performed -= callback;
    }

    private static void EnableAction(InputActionReference actionReference)
    {
        GetAction(actionReference)?.Enable();
    }

    private static InputAction GetAction(InputActionReference actionReference)
    {
        return actionReference != null ? actionReference.action : null;
    }
}
