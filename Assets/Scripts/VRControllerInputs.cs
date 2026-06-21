using UnityEngine;
using UnityEngine.InputSystem;

public class VRControllerInput : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;
    public PlayerTongue playerTongue;
    public CreatureSenses creatureSenses;

    [Header("Left Controller")]
    public InputActionProperty leftStickMove;
    public InputActionProperty leftTriggerJump;

    [Header("Right Controller")]
    public InputActionProperty rightStickSenses;
    public InputActionProperty rightTriggerTongue;

    private bool senseStickWasNeutral = true;

    private void Update()
    {
        Vector2 move = leftStickMove.action.ReadValue<Vector2>();
        bool jumpPressed = leftTriggerJump.action.WasPressedThisFrame();

        if (playerMovement != null)
        {
            playerMovement.SetVRMoveInput(move);
            if (jumpPressed)
                playerMovement.RequestVRJump();
        }

        if (rightTriggerTongue.action.WasPressedThisFrame() && playerTongue != null)
        {
            playerTongue.TryShootVR();
        }

        Vector2 senses = rightStickSenses.action.ReadValue<Vector2>();

        if (senseStickWasNeutral && senses.y > 0.7f)
        {
            if (creatureSenses != null)
                creatureSenses.ToggleNightVisionVR();

            senseStickWasNeutral = false;
        }
        else if (senseStickWasNeutral && senses.x > 0.7f)
        {
            Debug.Log("Right Stick RIGHT: Ultrasound/Sonar später hier auslösen");
            senseStickWasNeutral = false;
        }
        else if (senseStickWasNeutral && senses.y < -0.7f)
        {
            Debug.Log("Right Stick DOWN: anderer Sinn später");
            senseStickWasNeutral = false;
        }
        else if (senseStickWasNeutral && senses.x < -0.7f)
        {
            Debug.Log("Right Stick LEFT: anderer Sinn später");
            senseStickWasNeutral = false;
        }

        if (senses.magnitude < 0.25f)
            senseStickWasNeutral = true;
    }
}
