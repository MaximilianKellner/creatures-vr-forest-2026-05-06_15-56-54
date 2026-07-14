using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

[RequireComponent(typeof(CharacterController))]
public class XRControllerMoveFallback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform headTransform;
    [SerializeField] private UpgradeSystem upgradeSystem;
    [SerializeField] private InputActionReference moveAction;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6.6f;
    [SerializeField] private float speedBonusPerLevel = 0.2f;
    [SerializeField] private bool allowRightHandMoveFallback;
    [SerializeField] private bool disableXriContinuousMoveProviders = true;

    private InputAction fallbackMoveAction;
    private ContinuousMoveProvider[] xriMoveProviders;

    private void Awake()
    {
        ResolveReferences();
        CreateFallbackMoveAction();
        ApplyXriMoveProviderState();
    }

    private void OnEnable()
    {
        EnableAction(moveAction);
        fallbackMoveAction?.Enable();
        ApplyXriMoveProviderState();
    }

    private void OnDisable()
    {
        fallbackMoveAction?.Disable();
    }

    private void Update()
    {
        ResolveReferences();

        if (characterController == null || !characterController.enabled || Time.timeScale <= 0f)
            return;

        Vector2 input = ReadMoveInput();
        if (input.sqrMagnitude < 0.0001f)
            return;

        Vector3 forward = headTransform != null ? headTransform.forward : transform.forward;
        Vector3 right = headTransform != null ? headTransform.right : transform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 move = right * input.x + forward * input.y;
        if (move.sqrMagnitude > 1f)
            move.Normalize();

        characterController.Move(move * GetCurrentMoveSpeed() * Time.deltaTime);
    }

    private void ResolveReferences()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>() ??
                                  GetComponentInChildren<CharacterController>(true);

        if (headTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null && mainCamera.transform.IsChildOf(transform))
                headTransform = mainCamera.transform;
        }

        if (upgradeSystem == null)
            upgradeSystem = GetComponent<UpgradeSystem>() ??
                            GetComponentInChildren<UpgradeSystem>(true);

        if (xriMoveProviders == null || xriMoveProviders.Length == 0)
            xriMoveProviders = GetComponentsInChildren<ContinuousMoveProvider>(true);
    }

    private void CreateFallbackMoveAction()
    {
        fallbackMoveAction = new InputAction(
            "XR Direct Move Fallback",
            InputActionType.Value,
            expectedControlType: "Vector2");

        fallbackMoveAction.AddBinding("<XRController>{LeftHand}/{Primary2DAxis}");
        if (allowRightHandMoveFallback)
            fallbackMoveAction.AddBinding("<XRController>{RightHand}/{Primary2DAxis}");

        fallbackMoveAction.AddBinding("<Gamepad>/leftStick");

        fallbackMoveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
    }

    private Vector2 ReadMoveInput()
    {
        if (moveAction != null && moveAction.action != null)
            return Vector2.ClampMagnitude(moveAction.action.ReadValue<Vector2>(), 1f);

        return fallbackMoveAction != null
            ? Vector2.ClampMagnitude(fallbackMoveAction.ReadValue<Vector2>(), 1f)
            : Vector2.zero;
    }

    private float GetCurrentMoveSpeed()
    {
        int speedLevel = upgradeSystem != null
            ? upgradeSystem.GetUpgradeLevel(PreyGivesUpgrade.FasterRun)
            : 0;

        return moveSpeed * (1f + speedLevel * speedBonusPerLevel);
    }

    private void ApplyXriMoveProviderState()
    {
        if (disableXriContinuousMoveProviders)
            SetXriMoveProvidersEnabled(false);
    }

    private void SetXriMoveProvidersEnabled(bool enabled)
    {
        if (xriMoveProviders == null)
            return;

        foreach (ContinuousMoveProvider provider in xriMoveProviders)
        {
            if (provider != null)
                provider.enabled = enabled;
        }
    }

    private static void EnableAction(InputActionReference actionReference)
    {
        if (actionReference != null && actionReference.action != null)
            actionReference.action.Enable();
    }
}
