using UnityEngine;
using UnityEngine.InputSystem;

public class VRDirectInputTest : MonoBehaviour
{
    public InputActionReference leftMove;
    public InputActionReference leftTrigger;
    public InputActionReference rightMove;
    public InputActionReference rightTrigger;

    private void OnEnable()
    {
        leftMove?.action.Enable();
        leftTrigger?.action.Enable();
        rightMove?.action.Enable();
        rightTrigger?.action.Enable();
    }

    private void OnDisable()
    {
        leftMove?.action.Disable();
        leftTrigger?.action.Disable();
        rightMove?.action.Disable();
        rightTrigger?.action.Disable();
    }

    private void Update()
    {
        Vector2 left = leftMove.action.ReadValue<Vector2>();
        Vector2 right = rightMove.action.ReadValue<Vector2>();

        if (left.magnitude > 0.2f)
            Debug.Log("LEFT TOUCHPAD / MOVE: " + left);

        if (right.magnitude > 0.2f)
            Debug.Log("RIGHT TOUCHPAD / SENSES: " + right);

        if (leftTrigger.action.WasPressedThisFrame())
            Debug.Log("LEFT TRIGGER / JUMP");

        if (rightTrigger.action.WasPressedThisFrame())
            Debug.Log("RIGHT TRIGGER / TONGUE");
    }
}
