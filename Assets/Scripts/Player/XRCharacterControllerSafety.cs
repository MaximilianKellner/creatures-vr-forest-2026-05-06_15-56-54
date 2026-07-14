using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public class XRCharacterControllerSafety : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;

    [Header("Controller Shape")]
    [SerializeField] private bool fixLowCenter = true;
    [SerializeField] private float maxRadius = 0.35f;
    [SerializeField] private float minimumStepOffset = 0.45f;
    [SerializeField] private float minimumSlopeLimit = 55f;
    [SerializeField] private float minimumSkinWidth = 0.08f;

    [Header("Extra Body Collider")]
    [SerializeField] private bool makePlayerBodyColliderTrigger = true;
    [SerializeField] private string playerBodyName = "PlayerBody";

    [Header("Spawn Grounding")]
    [SerializeField] private bool snapToGroundOnStart = true;
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private float groundProbeUp = 12f;
    [SerializeField] private float groundProbeDown = 60f;
    [SerializeField] private float groundSkin = 0.03f;
    [SerializeField] private float minimumGroundNormalY = 0.35f;

    private void Awake()
    {
        ApplySafetySettings();
    }

    private void OnEnable()
    {
        ApplySafetySettings();
    }

    private void Start()
    {
        ApplySafetySettings();

        if (snapToGroundOnStart)
            SnapToGround();
    }

    public void ApplySafetySettings()
    {
        ResolveReferences();
        ConfigureCharacterController();
        ConfigureBodyColliders();
    }

    private void ResolveReferences()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>() ??
                                  GetComponentInChildren<CharacterController>(true);
    }

    private void ConfigureCharacterController()
    {
        if (characterController == null)
            return;

        if (maxRadius > 0f && characterController.radius > maxRadius)
            characterController.radius = maxRadius;

        if (fixLowCenter && characterController.center.y < characterController.height * 0.25f)
        {
            Vector3 center = characterController.center;
            center.y = characterController.height * 0.5f;
            characterController.center = center;
        }

        characterController.stepOffset = Mathf.Max(
            characterController.stepOffset,
            Mathf.Min(minimumStepOffset, characterController.height * 0.45f));
        characterController.slopeLimit = Mathf.Max(characterController.slopeLimit, minimumSlopeLimit);
        characterController.skinWidth = Mathf.Max(characterController.skinWidth, minimumSkinWidth);
        characterController.minMoveDistance = 0f;
        characterController.detectCollisions = true;
    }

    private void ConfigureBodyColliders()
    {
        if (!makePlayerBodyColliderTrigger)
            return;

        Collider[] colliders = GetComponentsInChildren<Collider>(true);

        foreach (Collider collider in colliders)
        {
            if (collider == null || collider is CharacterController)
                continue;

            if (!collider.gameObject.name.Equals(playerBodyName))
                continue;

            collider.isTrigger = true;

            if (collider is CapsuleCollider capsuleCollider)
                ConfigureBodyCapsule(capsuleCollider);
        }
    }

    private void ConfigureBodyCapsule(CapsuleCollider capsuleCollider)
    {
        if (maxRadius > 0f && capsuleCollider.radius > maxRadius)
            capsuleCollider.radius = maxRadius;

        if (fixLowCenter && capsuleCollider.center.y < capsuleCollider.height * 0.25f)
        {
            Vector3 center = capsuleCollider.center;
            center.y = capsuleCollider.height * 0.5f;
            capsuleCollider.center = center;
        }
    }

    private void SnapToGround()
    {
        if (characterController == null)
            return;

        Bounds bounds = characterController.bounds;
        Vector3 origin = new Vector3(
            bounds.center.x,
            transform.position.y + groundProbeUp,
            bounds.center.z);
        float radius = Mathf.Max(0.05f, characterController.radius * 0.8f);
        float distance = groundProbeUp + groundProbeDown;

        if (!TryFindGround(origin, radius, distance, out RaycastHit hit))
            return;

        float currentBottom = GetCharacterControllerBottom();
        float targetBottom = hit.point.y + groundSkin;
        float deltaY = targetBottom - currentBottom;

        if (Mathf.Abs(deltaY) < 0.001f)
            return;

        bool wasEnabled = characterController.enabled;

        if (wasEnabled)
            characterController.enabled = false;

        transform.position += Vector3.up * deltaY;

        if (wasEnabled)
            characterController.enabled = true;
    }

    private bool TryFindGround(
        Vector3 origin,
        float radius,
        float distance,
        out RaycastHit groundHit)
    {
        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            radius,
            Vector3.down,
            distance,
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

    private float GetCharacterControllerBottom()
    {
        float scaledHalfHeight = characterController.height * 0.5f * Mathf.Abs(transform.lossyScale.y);
        return transform.TransformPoint(characterController.center).y - scaledHalfHeight;
    }
}
