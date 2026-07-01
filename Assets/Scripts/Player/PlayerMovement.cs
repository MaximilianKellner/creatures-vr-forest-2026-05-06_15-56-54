/// <summary>
/// Zuständig für die First-Person-Spielerbewegung (Laufen, Sprinten, Ducken, Schwerkraft und Springen)
/// sowie die Kamerasteuerung über die Maus bzw. das VR-Headset.
///
/// STEUERUNG (Desktop):
/// - Bewegen: W, A, S, D oder Pfeiltasten
/// - Umschauen: Mausbewegung
/// - Sprinten: Shift-Taste links (halten)
/// - Springen: Leertaste
/// - Ducken: C-Taste (halten)
///
/// STEUERUNG (VR / HTC Vive Pro):
/// - Bewegen: linkes Controller-Trackpad (Richtung + Auslenkung wie ein Analogstick)
/// - Umschauen: Kopfbewegung (HMD-Tracking)
/// - Drehen: rechtes Controller-Trackpad (Snap-Turn nach links/rechts)
/// </summary>

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] Camera playerCamera;
    [SerializeField] float walkSpeed = 6f, runSpeed = 12f, jumpPower = 7f, gravity = 10f;

    [Header("Upgrade Settings")]
    [SerializeField] float speedBonusPerLevel = 0.2f;
    [SerializeField] float jumpBonusPerLevel = 0.2f;

    [Header("Look & Crouch")]
    [SerializeField] float lookSpeed = 2f, lookXLimit = 45f;
    [SerializeField] float defaultHeight = 2f, crouchHeight = 1f, crouchSpeed = 3f;

    [Header("VR Locomotion")]
    [SerializeField] private InputActionReference vrMoveAction;
    [SerializeField] private InputActionReference vrSnapTurnAction;
    [SerializeField] private float snapTurnAngle = 45f;
    [SerializeField] private float snapTurnDeadzone = 0.5f;
    [SerializeField] private float snapTurnCooldown = 0.3f;

    private Vector3 moveDirection;
    private float rotationX;
    private float snapTurnCooldownTimer;
    private CharacterController characterController;
    private UpgradeSystem upgradeSystem;
    private bool canMove = true;
    private bool canLook = true;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        upgradeSystem =
            GetComponentInParent<UpgradeSystem>() ??
            GetComponentInChildren<UpgradeSystem>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        vrMoveAction?.action?.Enable();
        vrSnapTurnAction?.action?.Enable();
    }

    private void OnDisable()
    {
        vrMoveAction?.action?.Disable();
        vrSnapTurnAction?.action?.Disable();
    }

    private void Update()
    {
        if (XRSettings.isDeviceActive)
        {
            HandleVRMovement();
            HandleSnapTurn();
            return;
        }

        // Sicherheitsabfrage: Gibt es überhaupt Eingabegeräte?
        if (Keyboard.current == null || Mouse.current == null) return;

        HandleMovement();
        HandleLook();
    }

    private void HandleMovement()
    {
        var keyboard = Keyboard.current;

        // 1. Ducken & Geschwindigkeit bestimmen
        bool isCrouching = keyboard.cKey.isPressed && canMove;
        characterController.height = isCrouching ? crouchHeight : defaultHeight;

        float currentWalkSpeed = walkSpeed;
        float currentRunSpeed = runSpeed;
        float currentJumpPower = jumpPower;

        if (upgradeSystem != null)
        {
            int speedLevel = upgradeSystem.GetUpgradeLevel(PreyGivesUpgrade.FasterRun);
            int jumpLevel = upgradeSystem.GetUpgradeLevel(PreyGivesUpgrade.HigherJump);

            currentWalkSpeed *= 1f + speedLevel * speedBonusPerLevel;
            currentRunSpeed *= 1f + speedLevel * speedBonusPerLevel;
            currentJumpPower *= 1f + jumpLevel * jumpBonusPerLevel;
        }

        bool hasSprint = upgradeSystem != null &&
                 upgradeSystem.HasUpgrade(PreyGivesUpgrade.Sprint);

        bool wantsToSprint = keyboard.leftShiftKey.isPressed && hasSprint;

        float speed = isCrouching
            ? crouchSpeed
            : (wantsToSprint ? currentRunSpeed : currentWalkSpeed);

        // 2. Richtung auslesen (WASD / Pfeiltasten)
        float moveVertical = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);
        float moveHorizontal = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);

        // 3. Bewegungs-Vektor berechnen
        float lastY = moveDirection.y;
        moveDirection = canMove ? (transform.forward * moveVertical + transform.right * moveHorizontal) * speed : Vector3.zero;
        
        // 4. Springen & Gravitation
        moveDirection.y = (keyboard.spaceKey.isPressed && canMove && characterController.isGrounded) ? currentJumpPower : lastY;
        if (!characterController.isGrounded) moveDirection.y -= gravity * Time.deltaTime;

        // 5. Ausführen
        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleLook()
    {
        if (!canLook) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * 0.1f * lookSpeed;

        rotationX = Mathf.Clamp(rotationX - mouseDelta.y, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        transform.rotation *= Quaternion.Euler(0, mouseDelta.x, 0);
    }

    private void HandleVRMovement()
    {
        Vector2 moveInput = vrMoveAction != null && vrMoveAction.action != null
            ? vrMoveAction.action.ReadValue<Vector2>()
            : Vector2.zero;

        // Blickrichtung der Kamera (vom HMD getrackt) als Bewegungsbasis, auf die Bodenebene projiziert.
        Vector3 camForward = playerCamera.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = playerCamera.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        float speed = GetCurrentWalkSpeed() * Mathf.Clamp01(moveInput.magnitude);

        float lastY = moveDirection.y;
        moveDirection = canMove ? (camForward * moveInput.y + camRight * moveInput.x) * speed : Vector3.zero;

        moveDirection.y = lastY;
        if (!characterController.isGrounded) moveDirection.y -= gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleSnapTurn()
    {
        // Kopf-Tracking bleibt unabhängig von canLook aktiv (kann physische Kopfdrehung nicht sperren) -
        // nur der zusätzliche Snap-Turn per Trackpad wird gesperrt.
        if (!canLook) return;

        snapTurnCooldownTimer -= Time.deltaTime;
        if (snapTurnCooldownTimer > 0f) return;

        Vector2 turnInput = vrSnapTurnAction != null && vrSnapTurnAction.action != null
            ? vrSnapTurnAction.action.ReadValue<Vector2>()
            : Vector2.zero;

        if (turnInput.x > snapTurnDeadzone)
        {
            transform.Rotate(0f, snapTurnAngle, 0f);
            snapTurnCooldownTimer = snapTurnCooldown;
        }
        else if (turnInput.x < -snapTurnDeadzone)
        {
            transform.Rotate(0f, -snapTurnAngle, 0f);
            snapTurnCooldownTimer = snapTurnCooldown;
        }
    }

    private float GetCurrentWalkSpeed()
    {
        float currentWalkSpeed = walkSpeed;

        if (upgradeSystem != null)
        {
            int speedLevel = upgradeSystem.GetUpgradeLevel(PreyGivesUpgrade.FasterRun);
            currentWalkSpeed *= 1f + speedLevel * speedBonusPerLevel;
        }

        return currentWalkSpeed;
    }
    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;
    }

    public void SetLookEnabled(bool enabled)
    {
        canLook = enabled;
    }
    public bool CanMove => canMove;
    public bool CanLook => canLook;
}