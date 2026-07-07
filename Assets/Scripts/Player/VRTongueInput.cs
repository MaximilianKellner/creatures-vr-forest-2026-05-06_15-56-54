using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class VRTongueInput : MonoBehaviour
{
    [SerializeField]
    InputActionReference shootAction;

    [SerializeField]
    PlayerTongue playerTongue;

    void Awake()
    {
        if (playerTongue == null)
            playerTongue = GetComponentInParent<PlayerTongue>() ?? GetComponentInChildren<PlayerTongue>();
    }

    void OnEnable()
    {
        Bind(shootAction, OnShootPerformed);
    }

    void OnDisable()
    {
        Unbind(shootAction, OnShootPerformed);
    }

    void OnShootPerformed(InputAction.CallbackContext context)
    {
        if (playerTongue != null)
            playerTongue.TryShoot();
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

    static InputAction GetAction(InputActionReference actionReference)
    {
        return actionReference != null ? actionReference.action : null;
    }
}
