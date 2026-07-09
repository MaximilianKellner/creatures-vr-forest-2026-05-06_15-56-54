using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class VRTongueInput : MonoBehaviour
{
    [SerializeField] private InputActionReference shootAction;
    [SerializeField] private PlayerTongue playerTongue;

    private InputAction fallbackShootAction;
    private bool usingFallbackShootAction;

    private void Awake()
    {
        if (playerTongue == null)
            playerTongue = GetComponentInParent<PlayerTongue>() ??
                           GetComponentInChildren<PlayerTongue>();

        fallbackShootAction = new InputAction(
            "Tongue Shoot Fallback",
            InputActionType.Button);
        fallbackShootAction.AddBinding("<XRController>{RightHand}/{TriggerButton}");
        fallbackShootAction.AddBinding("<Keyboard>/t");
    }

    private void OnEnable()
    {
        usingFallbackShootAction = !Bind(shootAction, OnShootPerformed);

        if (usingFallbackShootAction && fallbackShootAction != null)
        {
            fallbackShootAction.performed += OnShootPerformed;
            fallbackShootAction.Enable();
        }
    }

    private void OnDisable()
    {
        Unbind(shootAction, OnShootPerformed);

        if (usingFallbackShootAction && fallbackShootAction != null)
        {
            fallbackShootAction.performed -= OnShootPerformed;
            fallbackShootAction.Disable();
        }
    }

    private void OnShootPerformed(InputAction.CallbackContext context)
    {
        if (playerTongue != null)
            playerTongue.TryShoot();
        else
            Debug.LogError("[VRTongueInput] PlayerTongue reference is missing.");
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

    private static InputAction GetAction(InputActionReference actionReference)
    {
        return actionReference != null ? actionReference.action : null;
    }
}
