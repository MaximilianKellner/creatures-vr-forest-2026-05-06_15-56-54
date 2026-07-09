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
}
