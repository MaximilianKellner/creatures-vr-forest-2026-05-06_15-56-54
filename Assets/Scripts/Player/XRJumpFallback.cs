using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class XRJumpFallback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private UpgradeSystem upgradeSystem;
    [SerializeField] private InputActionReference jumpAction;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 1.25f;
    [SerializeField] private float jumpBonusPerLevel = 0.2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpCooldown = 0.15f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private float groundCheckDistance = 0.12f;

    [Header("XRI Compatibility")]
    [SerializeField] private bool disableBuiltInJumpProvider = true;

    private InputAction fallbackJumpAction;
    private float verticalVelocity;
    private float nextJumpTime;

    public bool DisablesBuiltInJumpProvider => disableBuiltInJumpProvider;

    private void Awake()
    {
        ResolveReferences();
        CreateFallbackJumpAction();
    }

    private void OnEnable()
    {
        EnableAction(jumpAction);
        fallbackJumpAction?.Enable();
    }

    private void OnDisable()
    {
        fallbackJumpAction?.Disable();
        verticalVelocity = 0f;
    }

    private void Update()
    {
        ResolveReferences();

        if (characterController == null || !characterController.enabled)
            return;

        if (WasJumpPressed() && CanStartJump())
            StartJump();

        ApplyJumpMotion();
    }

    private void ResolveReferences()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>() ??
                                  GetComponentInChildren<CharacterController>();

        if (upgradeSystem == null)
            upgradeSystem = GetComponentInParent<UpgradeSystem>() ??
                            GetComponentInChildren<UpgradeSystem>();
    }

    private void CreateFallbackJumpAction()
    {
        fallbackJumpAction = new InputAction(
            "XR Jump Fallback",
            InputActionType.Button);
        fallbackJumpAction.AddBinding("<XRController>{RightHand}/{PrimaryButton}");
        fallbackJumpAction.AddBinding("<XRController>{RightHand}/{SecondaryButton}");
        fallbackJumpAction.AddBinding("<Keyboard>/space");
    }

    private bool WasJumpPressed()
    {
        if (jumpAction != null &&
            jumpAction.action != null &&
            jumpAction.action.WasPressedThisFrame())
            return true;

        return fallbackJumpAction != null &&
               fallbackJumpAction.WasPressedThisFrame();
    }

    private bool CanStartJump()
    {
        if (Time.time < nextJumpTime)
            return false;

        return IsGrounded();
    }

    private void StartJump()
    {
        float height = GetCurrentJumpHeight();
        verticalVelocity = Mathf.Sqrt(height * -2f * gravity);
        nextJumpTime = Time.time + jumpCooldown;
    }

    private float GetCurrentJumpHeight()
    {
        int jumpLevel = upgradeSystem != null
            ? upgradeSystem.GetUpgradeLevel(PreyGivesUpgrade.HigherJump)
            : 0;

        return jumpHeight * (1f + jumpLevel * jumpBonusPerLevel);
    }

    private void ApplyJumpMotion()
    {
        if (verticalVelocity <= 0f)
        {
            verticalVelocity = 0f;
            return;
        }

        characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
        verticalVelocity += gravity * Time.deltaTime;
    }

    private bool IsGrounded()
    {
        if (characterController.isGrounded)
            return true;

        Bounds bounds = characterController.bounds;
        Vector3 origin = new Vector3(
            bounds.center.x,
            bounds.min.y + characterController.radius + 0.03f,
            bounds.center.z);
        float radius = Mathf.Max(0.05f, characterController.radius * 0.85f);

        return Physics.SphereCast(
            origin,
            radius,
            Vector3.down,
            out _,
            groundCheckDistance,
            groundLayers,
            QueryTriggerInteraction.Ignore);
    }

    private static void EnableAction(InputActionReference actionReference)
    {
        if (actionReference != null && actionReference.action != null)
            actionReference.action.Enable();
    }
}
