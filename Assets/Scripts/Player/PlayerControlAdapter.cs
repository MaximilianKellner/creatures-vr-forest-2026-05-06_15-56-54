using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Jump;

public class PlayerControlAdapter : MonoBehaviour
{
    [Header("Classic Player")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("XR Locomotion")]
    [SerializeField] private LocomotionProvider[] locomotionProviders;
    [SerializeField] private XRJumpFallback[] jumpFallbacks;
    [SerializeField] private XRControllerMoveFallback[] moveFallbacks;

    [Header("Optional Extra Controls")]
    [SerializeField] private Behaviour[] movementBehaviours;
    [SerializeField] private Behaviour[] lookBehaviours;

    private void Awake()
    {
        ResolveReferences();
    }

    public void SetMovementEnabled(bool enabled)
    {
        ResolveReferences();

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(enabled);

        SetLocomotionProvidersEnabled(
            locomotionProviders,
            enabled,
            ShouldDisableBuiltInJumpProviders());
        SetBehavioursEnabled(jumpFallbacks, enabled);
        SetBehavioursEnabled(moveFallbacks, enabled);
        SetBehavioursEnabled(movementBehaviours, enabled);
    }

    public void SetLookEnabled(bool enabled)
    {
        ResolveReferences();

        if (playerMovement != null)
            playerMovement.SetLookEnabled(enabled);

        SetBehavioursEnabled(lookBehaviours, enabled);
    }

    public Transform PlayerTransform
    {
        get
        {
            if (playerMovement != null)
                return playerMovement.transform;

            return transform;
        }
    }

    public CharacterController CharacterController
    {
        get
        {
            CharacterController controller = GetComponent<CharacterController>();
            if (controller != null)
                return controller;

            return GetComponentInChildren<CharacterController>();
        }
    }

    private void ResolveReferences()
    {
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>() ??
                             GetComponentInParent<PlayerMovement>() ??
                             GetComponentInChildren<PlayerMovement>();

        if (locomotionProviders == null || locomotionProviders.Length == 0)
            locomotionProviders = GetComponentsInChildren<LocomotionProvider>(true);

        if (jumpFallbacks == null || jumpFallbacks.Length == 0)
            jumpFallbacks = GetComponentsInChildren<XRJumpFallback>(true);

        if (moveFallbacks == null || moveFallbacks.Length == 0)
            moveFallbacks = GetComponentsInChildren<XRControllerMoveFallback>(true);
    }

    private bool ShouldDisableBuiltInJumpProviders()
    {
        if (jumpFallbacks == null)
            return false;

        foreach (XRJumpFallback jumpFallback in jumpFallbacks)
        {
            if (jumpFallback != null && jumpFallback.DisablesBuiltInJumpProvider)
                return true;
        }

        return false;
    }

    private static void SetBehavioursEnabled(Behaviour[] behaviours, bool enabled)
    {
        if (behaviours == null)
            return;

        foreach (Behaviour behaviour in behaviours)
        {
            if (behaviour != null)
                behaviour.enabled = enabled;
        }
    }

    private static void SetLocomotionProvidersEnabled(
        LocomotionProvider[] providers,
        bool enabled,
        bool disableBuiltInJumpProviders)
    {
        if (providers == null)
            return;

        foreach (LocomotionProvider provider in providers)
        {
            if (provider == null)
                continue;

            if (provider is GravityProvider)
            {
                provider.enabled = true;
                continue;
            }

            if (disableBuiltInJumpProviders && provider is JumpProvider)
            {
                provider.enabled = false;
                continue;
            }

            provider.enabled = enabled;
        }
    }
}
