using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class XRCharacterControllerGravity : MonoBehaviour
{
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float terminalVelocity = -30f;
    [SerializeField] float groundedStickVelocity = -1f;
    [SerializeField] LayerMask groundLayers = 1;
    [SerializeField] float groundCheckDistance = 20f;
    [SerializeField] float groundSnapDistance = 2f;
    [SerializeField] float groundSkin = 0.03f;

    CharacterController characterController;
    float verticalVelocity;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (TrySnapToGround())
        {
            verticalVelocity = groundedStickVelocity;
            return;
        }

        verticalVelocity = Mathf.Max(verticalVelocity + gravity * Time.deltaTime, terminalVelocity);

        characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    bool TrySnapToGround()
    {
        Bounds bounds = characterController.bounds;
        Vector3 origin = new Vector3(bounds.center.x, bounds.max.y + 0.25f, bounds.center.z);
        float castRadius = Mathf.Max(0.05f, characterController.radius * 0.8f);

        if (!Physics.SphereCast(origin, castRadius, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayers, QueryTriggerInteraction.Ignore))
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
}
