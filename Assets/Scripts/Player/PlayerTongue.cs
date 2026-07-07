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

    [Header("Settings")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private LayerMask collisionMask;

    [Header("Upgrade")]
    [SerializeField] private float tongueSpeedBonus = 10f;

    private UpgradeSystem upgradeSystem;
    private bool isBusy;

    private void Awake()
    {
        lineRenderer.enabled = false;

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

    public void TryShoot()
    {
        if (isBusy)
        {
            Debug.Log("[PlayerTongue] Zunge ist noch aktiv, ignoriere Input!");
            return;
        }

        Vector3 targetPos =
            playerCamera.transform.position +
            playerCamera.transform.forward * maxDistance;

        StartCoroutine(Shoot(targetPos));
    }

    private IEnumerator Shoot(Vector3 targetPos)
    {
        isBusy = true;

        Debug.Log($"[PlayerTongue] Shoot gestartet. LineRenderer null? {lineRenderer == null}");
        
        if (lineRenderer == null)
        {
            Debug.LogError("[PlayerTongue] FEHLER: LineRenderer ist NULL! Kann nicht schießen!");
            isBusy = false;
            yield break;
        }

        lineRenderer.enabled = true;
        lineRenderer.positionCount = 2;

        Debug.Log($"[PlayerTongue] LineRenderer Material: {lineRenderer.material}");
        Debug.Log($"[PlayerTongue] LineRenderer StartWidth: {lineRenderer.startWidth}");
        Debug.Log($"[PlayerTongue] LineRenderer EndWidth: {lineRenderer.endWidth}");

        float currentSpeed = GetTongueSpeed();

        float t = 0f;
        float distance = Vector3.Distance(
            tongueOrigin.position,
            targetPos
        );

        Debug.Log($"[PlayerTongue] Distance: {distance}, Speed: {currentSpeed}");

        Prey hitPrey = null;
        bool hitObstacle = false;

        // Zunge ausfahren
        while (t < 1f)
        {
            Debug.Log($"[PlayerTongue] While-Loop Iteration: t={t:F3}");
            t += Time.deltaTime * currentSpeed / distance;

            Vector3 start = tongueOrigin.position;
            Vector3 pos = Vector3.Lerp(start, targetPos, t);

            Debug.Log($"[PlayerTongue] SetPosition - start: {start}, pos: {pos}");

            if (Physics.Raycast(
                start,
                (pos - start).normalized,
                out RaycastHit hit,
                Vector3.Distance(start, pos),
                collisionMask))
            {
                if (hit.collider.TryGetComponent(out Prey prey))
                {
                    hitPrey = prey;
                }
                else
                {
                    hitObstacle = true;
                    targetPos = hit.point;
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
            hitPrey.AttachToTongue(tongueTip);
        }

        // Zunge zurückziehen
        float backT = 1f;

        while (backT > 0f)
        {
            backT -= Time.deltaTime * currentSpeed / distance;

            Vector3 start = tongueOrigin.position;
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
