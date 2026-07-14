using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class XRPlayerBodyTrigger : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform headTransform;
    [SerializeField] private string bodyName = "PlayerBody";
    [SerializeField] private float cameraBackOffset = 0.18f;
    [SerializeField] private bool followCameraHorizontally = true;

    private Transform bodyTransform;
    private CapsuleCollider bodyCollider;
    private Rigidbody bodyRigidbody;

    private void Awake()
    {
        ResolveReferences();
        ConfigureBody();
    }

    private void LateUpdate()
    {
        ConfigureBody();
    }

    private void ResolveReferences()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>() ??
                                  GetComponentInChildren<CharacterController>(true);

        Transform body = transform.Find(bodyName);
        if (body == null)
            body = FindChildByName(transform, bodyName);

        if (body == null)
        {
            GameObject bodyObject = new GameObject(bodyName);
            bodyObject.transform.SetParent(transform, false);
            body = bodyObject.transform;
        }

        bodyTransform = body;
        bodyCollider = body.GetComponent<CapsuleCollider>() ??
                       body.gameObject.AddComponent<CapsuleCollider>();
        bodyRigidbody = body.GetComponent<Rigidbody>() ??
                        body.gameObject.AddComponent<Rigidbody>();

        try
        {
            body.gameObject.tag = "Player";
        }
        catch (UnityException)
        {
            // The tag exists in the project; this just keeps test scenes from failing hard.
        }
    }

    private void ConfigureBody()
    {
        if (characterController == null)
            return;

        ResolveHeadTransform();

        if (bodyCollider == null || bodyRigidbody == null)
            ResolveReferences();

        PositionBodyBehindCamera();

        bodyCollider.isTrigger = true;
        bodyCollider.radius = characterController.radius;
        bodyCollider.height = characterController.height;
        bodyCollider.center = Vector3.up * (characterController.height * 0.5f);
        bodyCollider.direction = 1;

        bodyRigidbody.isKinematic = true;
        bodyRigidbody.useGravity = false;
        bodyRigidbody.detectCollisions = true;
    }

    private void ResolveHeadTransform()
    {
        if (headTransform != null)
            return;

        Camera mainCamera = Camera.main;
        if (mainCamera != null && mainCamera.transform.IsChildOf(transform))
            headTransform = mainCamera.transform;
    }

    private void PositionBodyBehindCamera()
    {
        if (!followCameraHorizontally || bodyTransform == null || headTransform == null)
            return;

        Vector3 forward = headTransform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            forward = transform.forward;

        forward.Normalize();

        float bottomY = characterController.bounds.min.y;
        Vector3 target = headTransform.position - forward * cameraBackOffset;
        target.y = bottomY;

        bodyTransform.position = target;
        bodyTransform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

    private static Transform FindChildByName(Transform root, string childName)
    {
        if (root == null)
            return null;

        foreach (Transform child in root)
        {
            if (child.name == childName)
                return child;

            Transform match = FindChildByName(child, childName);
            if (match != null)
                return match;
        }

        return null;
    }
}
