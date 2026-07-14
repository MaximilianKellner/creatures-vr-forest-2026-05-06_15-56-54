using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;

[RequireComponent(typeof(CharacterController))]
public class XRCharacterControllerGravity : MonoBehaviour
{
    [SerializeField] bool disableWhenXriGravityProviderExists = true;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float terminalVelocity = -30f;
    [SerializeField] float groundedStickVelocity = -1f;
    [SerializeField] LayerMask groundLayers = ~0;
    [SerializeField] float groundCheckDistance = 20f;
    [SerializeField] float groundSnapDistance = 2f;
    [SerializeField] float groundSkin = 0.03f;
    [SerializeField] float minimumGroundNormalY = 0.35f;

    CharacterController characterController;
    float verticalVelocity;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (disableWhenXriGravityProviderExists && HasActiveXriGravityProvider())
            enabled = false;
    }

    void Update()
    {
        if (verticalVelocity <= 0f && TrySnapToGround())
        {
            verticalVelocity = groundedStickVelocity;
            return;
        }

        verticalVelocity = Mathf.Max(verticalVelocity + gravity * Time.deltaTime, terminalVelocity);
        CollisionFlags flags = characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);

        if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
            verticalVelocity = 0f;
    }

    public void AddJumpVelocity(float velocity)
    {
        verticalVelocity = Mathf.Max(verticalVelocity, velocity);
    }

    public bool IsGrounded()
    {
        if (characterController == null)
            return false;

        if (characterController.isGrounded)
            return true;

        Bounds bounds = characterController.bounds;
        Vector3 origin = new Vector3(bounds.center.x, bounds.max.y + 0.25f, bounds.center.z);
        float castRadius = Mathf.Max(0.05f, characterController.radius * 0.8f);

        if (!TryFindGround(origin, castRadius, out RaycastHit hit))
            return false;

        float deltaY = hit.point.y + groundSkin - bounds.min.y;
        return deltaY >= -groundSnapDistance && deltaY <= groundSnapDistance;
    }

    bool TrySnapToGround()
    {
        Bounds bounds = characterController.bounds;
        Vector3 origin = new Vector3(bounds.center.x, bounds.max.y + 0.25f, bounds.center.z);
        float castRadius = Mathf.Max(0.05f, characterController.radius * 0.8f);

        if (!TryFindGround(origin, castRadius, out RaycastHit hit))
            return false;

        float currentBottom = bounds.min.y;
        float targetBottom = hit.point.y + groundSkin;
        float deltaY = targetBottom - currentBottom;

        if (deltaY < -groundSnapDistance || deltaY > groundSnapDistance)
            return false;

        if (Mathf.Abs(deltaY) > 0.001f)
            characterController.Move(Vector3.up * deltaY);

        return true;
    }

    bool TryFindGround(Vector3 origin, float castRadius, out RaycastHit groundHit)
    {
        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            castRadius,
            Vector3.down,
            groundCheckDistance,
            groundLayers,
            QueryTriggerInteraction.Ignore);

        Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null || hit.collider.transform.IsChildOf(transform))
                continue;

            if (hit.normal.y < minimumGroundNormalY)
                continue;

            groundHit = hit;
            return true;
        }

        groundHit = default;
        return false;
    }

    bool HasActiveXriGravityProvider()
    {
        var gravityProvider = GetComponentInParent<GravityProvider>() ??
                              GetComponentInChildren<GravityProvider>(true);

        return gravityProvider != null && gravityProvider.isActiveAndEnabled;
    }
}
