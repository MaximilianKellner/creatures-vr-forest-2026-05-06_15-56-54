using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// Maps Vive wand controls to game abilities:
/// left touchpad movement is handled by XRI, right touchpad directions trigger abilities,
/// left trigger jumps, and right trigger shoots the tongue.
/// </summary>
public class ViveProAbilityInput : MonoBehaviour
{
    [Serializable]
    public class MessageTarget
    {
        [SerializeField]
        GameObject target;

        [SerializeField]
        string methodName;

        public void Invoke()
        {
            if (target == null || string.IsNullOrWhiteSpace(methodName))
                return;

            target.SendMessage(methodName, SendMessageOptions.DontRequireReceiver);
        }
    }

    [Header("Right Touchpad Abilities")]
    [SerializeField]
    InputActionReference rightTouchpadAxis;

    [SerializeField]
    InputActionReference rightTouchpadClick;

    [SerializeField, Range(0.1f, 1f)]
    float directionThreshold = 0.45f;

    [Header("Trigger Actions")]
    [SerializeField]
    InputActionReference leftTrigger;

    [SerializeField]
    InputActionReference rightTrigger;

    [Header("Touchpad Direction Messages")]
    [SerializeField]
    MessageTarget abilityUp;

    [SerializeField]
    MessageTarget abilityRight;

    [SerializeField]
    MessageTarget abilityDown;

    [SerializeField]
    MessageTarget abilityLeft;

    [Header("Trigger Events")]
    [SerializeField]
    UnityEvent jump;

    [SerializeField]
    UnityEvent tongue;

    void OnEnable()
    {
        EnableAction(rightTouchpadAxis);
        Bind(rightTouchpadClick, OnRightTouchpadClicked);
        Bind(leftTrigger, OnLeftTriggerPressed);
        Bind(rightTrigger, OnRightTriggerPressed);
    }

    void OnDisable()
    {
        Unbind(rightTouchpadClick, OnRightTouchpadClicked);
        Unbind(leftTrigger, OnLeftTriggerPressed);
        Unbind(rightTrigger, OnRightTriggerPressed);
    }

    void OnRightTouchpadClicked(InputAction.CallbackContext context)
    {
        var axis = ReadVector2(rightTouchpadAxis);
        if (axis.sqrMagnitude < directionThreshold * directionThreshold)
            return;

        if (Mathf.Abs(axis.x) > Mathf.Abs(axis.y))
        {
            if (axis.x > 0f)
                abilityRight?.Invoke();
            else
                abilityLeft?.Invoke();
        }
        else
        {
            if (axis.y > 0f)
                abilityUp?.Invoke();
            else
                abilityDown?.Invoke();
        }
    }

    void OnLeftTriggerPressed(InputAction.CallbackContext context)
    {
        jump?.Invoke();
    }

    void OnRightTriggerPressed(InputAction.CallbackContext context)
    {
        tongue?.Invoke();
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
