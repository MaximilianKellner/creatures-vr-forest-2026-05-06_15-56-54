using UnityEngine;

/// <summary>
/// Adds jump and gravity to an XR Origin that is moved horizontally by an XRI move provider.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class VRJump : MonoBehaviour
{
    [SerializeField]
    float jumpPower = 5f;

    [SerializeField]
    float gravity = 12f;

    [SerializeField]
    bool applyGravity;

    [SerializeField]
    float groundedStickForce = 1f;

    CharacterController m_CharacterController;
    float m_VerticalVelocity;

    void Awake()
    {
        m_CharacterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (m_CharacterController.isGrounded && m_VerticalVelocity < 0f)
            m_VerticalVelocity = -groundedStickForce;

        if (applyGravity)
            m_VerticalVelocity -= gravity * Time.deltaTime;

        m_CharacterController.Move(Vector3.up * m_VerticalVelocity * Time.deltaTime);
    }

    public void TryJump()
    {
        if (!m_CharacterController.isGrounded)
            return;

        m_VerticalVelocity = jumpPower;
    }
}
