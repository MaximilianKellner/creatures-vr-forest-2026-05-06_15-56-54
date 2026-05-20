/// <summary>
/// Zuständig für die First-Person-Spielerbewegung (Laufen, Sprinten, Ducken, Schwerkraft und Springen)
/// sowie die Kamerasteuerung über die Maus.
/// 
/// STEUERUNG:
/// - Bewegen: W, A, S, D oder Pfeiltasten
/// - Umschauen: Mausbewegung
/// - Sprinten: Shift-Taste links (halten)
/// - Springen: Leertaste
/// - Ducken: C-Taste (halten)
/// </summary>

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] Camera playerCamera;
    [SerializeField] float walkSpeed = 6f, runSpeed = 12f, jumpPower = 7f, gravity = 10f;
    
    [Header("Look & Crouch")]
    [SerializeField] float lookSpeed = 2f, lookXLimit = 45f;
    [SerializeField] float defaultHeight = 2f, crouchHeight = 1f, crouchSpeed = 3f;

    private Vector3 moveDirection;
    private float rotationX;
    private CharacterController characterController;
    private bool canMove = true;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
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
        float speed = isCrouching ? crouchSpeed : (keyboard.leftShiftKey.isPressed ? runSpeed : walkSpeed);

        // 2. Richtung auslesen (WASD / Pfeiltasten)
        float moveVertical = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);
        float moveHorizontal = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);

        // 3. Bewegungs-Vektor berechnen
        float lastY = moveDirection.y;
        moveDirection = canMove ? (transform.forward * moveVertical + transform.right * moveHorizontal) * speed : Vector3.zero;
        
        // 4. Springen & Gravitation
        moveDirection.y = (keyboard.spaceKey.isPressed && canMove && characterController.isGrounded) ? jumpPower : lastY;
        if (!characterController.isGrounded) moveDirection.y -= gravity * Time.deltaTime;

        // 5. Ausführen
        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleLook()
    {
        if (!canMove) return;

        // Maus-Delta auslesen und skalieren (Maus Bewegungsgeschwindigkeit)
        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * 0.1f * lookSpeed;
        
        // Vertikale Rotation (Kamera hoch/runter) cappen
        rotationX = Mathf.Clamp(rotationX - mouseDelta.y, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        
        // Horizontale Rotation (Spieler links/rechts)
        transform.rotation *= Quaternion.Euler(0, mouseDelta.x, 0);
    }
}