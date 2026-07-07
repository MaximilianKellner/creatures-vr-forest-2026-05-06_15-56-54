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
        Debug.Log("[VRTongueInput] OnEnable aufgerufen");
        Debug.Log($"[VRTongueInput] shootAction null? {shootAction == null}");
        Debug.Log($"[VRTongueInput] playerTongue null? {playerTongue == null}");
        Bind(shootAction, OnShootPerformed);
    }

    void OnDisable()
    {
        Debug.Log("[VRTongueInput] OnDisable aufgerufen");
        Unbind(shootAction, OnShootPerformed);
    }

    void OnShootPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("[VRTongueInput] OnShootPerformed - Input empfangen!");
        if (playerTongue != null)
        {
            Debug.Log("[VRTongueInput] TryShoot wird aufgerufen");
            playerTongue.TryShoot();
        }
        else
        {
            Debug.LogError("[VRTongueInput] playerTongue ist NULL!");
        }
    }

    static void Bind(InputActionReference actionReference, Action<InputAction.CallbackContext> callback)
    {
        var action = GetAction(actionReference);
        if (action == null)
        {
            Debug.LogError("[VRTongueInput] Bind fehlgeschlagen - action ist NULL! shootAction wurde nicht konfiguriert?");
            return;
        }

        Debug.Log($"[VRTongueInput] Binding erfolgreich für Action: {action.name}");
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
