using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerTongue : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform tongueOrigin;
    [SerializeField] private Transform tongueTip;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform aimSource;
    [SerializeField] private bool useAimSourceAsTongueOrigin = true;
    [SerializeField, Min(0f)] private float aimForwardOffset = 0.25f;
    [SerializeField, Min(0f)] private float aimDownOffset = 0.1f;

    [Header("Settings")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private LayerMask collisionMask;

    [Header("Upgrade")]
    [SerializeField] private float tongueSpeedBonus = 10f;

    private UpgradeSystem upgradeSystem;
    private bool isBusy;
    private Transform aimOffsetTarget;

    private void Awake()
    {
        if (lineRenderer != null)
            lineRenderer.enabled = false;

        if (aimSource == null && playerCamera != null)
            aimSource = playerCamera.transform;

        upgradeSystem =
            GetComponentInParent<UpgradeSystem>() ??
            GetComponentInChildren<UpgradeSystem>();
    }

    private void Update()
    {
        if (!isBusy &&
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryShoot();
        }
    }

    private void OnDestroy()
    {
        if (aimOffsetTarget != null)
            Destroy(aimOffsetTarget.gameObject);
    }

    public void TryShoot()
    {
        if (lineRenderer == null)
        {
            Debug.LogError("[PlayerTongue] FEHLER: LineRenderer ist NULL! Kann nicht schießen!");
            return;
        }

        Transform shotAimSource = aimSource;

        if (shotAimSource == null && playerCamera != null)
            shotAimSource = playerCamera.transform;

        if (shotAimSource == null)
        {
            Debug.LogError("[PlayerTongue] FEHLER: Keine Kamera/Aim Source gesetzt! Kann nicht zielen.");
            return;
        }

        if (isBusy)
        {
            Debug.Log("[PlayerTongue] Zunge ist noch aktiv, ignoriere Input!");
            return;
        }

        Vector3 shotStart = GetAimStart(shotAimSource);
        Vector3 targetPos = shotStart + shotAimSource.forward * maxDistance;

        StartCoroutine(Shoot(shotAimSource, targetPos));
    }

    private IEnumerator Shoot(Transform shotAimSource, Vector3 targetPos)
    {
        isBusy = true;

        Transform visualOrigin = tongueOrigin != null ? tongueOrigin : shotAimSource;
        Transform attachTarget =
            useAimSourceAsTongueOrigin && shotAimSource != null
                ? GetAimAttachTarget(shotAimSource)
                : tongueTip != null ? tongueTip : visualOrigin;

        if (visualOrigin == null)
        {
            Debug.LogError("[PlayerTongue] FEHLER: Kein Tongue Origin gesetzt!");
            isBusy = false;
            yield break;
        }

        lineRenderer.enabled = true;
        lineRenderer.positionCount = 2;

        float currentSpeed = GetTongueSpeed();
        Vector3 start = GetShotStart(shotAimSource, visualOrigin);

        float t = 0f;
        float distance = Vector3.Distance(start, targetPos);

        if (distance <= 0.01f)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.enabled = false;
            isBusy = false;
            yield break;
        }

        Prey hitPrey = null;
        bool hitObstacle = false;

        // Zunge ausfahren
        while (t < 1f)
        {
            t += Time.deltaTime * currentSpeed / distance;

            start = GetShotStart(shotAimSource, visualOrigin);
            Vector3 pos = Vector3.Lerp(start, targetPos, t);
            Vector3 direction = pos - start;
            float rayDistance = direction.magnitude;

            if (rayDistance > 0.001f && Physics.Raycast(
                start,
                direction.normalized,
                out RaycastHit hit,
                rayDistance,
                collisionMask,
                QueryTriggerInteraction.Collide))
            {
                Prey prey =
                    hit.collider.GetComponentInParent<Prey>() ??
                    hit.collider.GetComponentInChildren<Prey>();

                if (prey != null)
                {
                    hitPrey = prey;
                    targetPos = hit.point;
                    distance = Mathf.Max(Vector3.Distance(start, targetPos), 0.01f);
                    break;
                }
                else
                {
                    hitObstacle = true;
                    targetPos = hit.point;
                    distance = Mathf.Max(Vector3.Distance(start, targetPos), 0.01f);
                    break;
                }
            }

            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, pos);

            yield return null;
        }

        // Beute an Zunge befestigen
        if (!hitObstacle && hitPrey != null)
        {
            hitPrey.AttachToTongue(attachTarget);
        }

        // Zunge zurückziehen
        float backT = 1f;

        while (backT > 0f)
        {
            backT -= Time.deltaTime * currentSpeed / distance;

            start = GetShotStart(shotAimSource, visualOrigin);
            Vector3 pos = Vector3.Lerp(start, targetPos, backT);

            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, pos);

            yield return null;
        }

        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;

        if (hitPrey != null)
        {
            hitPrey.AllowEat();
        }

        isBusy = false;
    }

    private Vector3 GetShotStart(Transform shotAimSource, Transform visualOrigin)
    {
        if (useAimSourceAsTongueOrigin && shotAimSource != null)
            return GetAimStart(shotAimSource);

        return visualOrigin.position;
    }

    private Vector3 GetAimStart(Transform shotAimSource)
    {
        if (shotAimSource == null)
            return Vector3.zero;

        return shotAimSource.TransformPoint(GetAimLocalOffset());
    }

    private Transform GetAimAttachTarget(Transform shotAimSource)
    {
        if (shotAimSource == null || (aimForwardOffset <= 0f && aimDownOffset <= 0f))
            return shotAimSource;

        if (aimOffsetTarget == null)
        {
            GameObject offsetObject = new GameObject("TongueAimOffset");
            offsetObject.hideFlags = HideFlags.HideInHierarchy;
            aimOffsetTarget = offsetObject.transform;
        }

        if (aimOffsetTarget.parent != shotAimSource)
            aimOffsetTarget.SetParent(shotAimSource, false);

        aimOffsetTarget.localPosition = GetAimLocalOffset();
        aimOffsetTarget.localRotation = Quaternion.identity;
        aimOffsetTarget.localScale = Vector3.one;

        return aimOffsetTarget;
    }

    private Vector3 GetAimLocalOffset()
    {
        return Vector3.forward * aimForwardOffset + Vector3.down * aimDownOffset;
    }

    private float GetTongueSpeed()
    {
        float currentSpeed = speed;

        if (upgradeSystem != null &&
            upgradeSystem.HasUpgrade(PreyGivesUpgrade.TongueSpeed))
        {
            currentSpeed += tongueSpeedBonus;
        }

        return currentSpeed;
    }
}
