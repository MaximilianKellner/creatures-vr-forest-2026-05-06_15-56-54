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

    private void TryShoot()
    {
        Vector3 targetPos =
            playerCamera.transform.position +
            playerCamera.transform.forward * maxDistance;

        StartCoroutine(Shoot(targetPos));
    }

    private IEnumerator Shoot(Vector3 targetPos)
    {
        isBusy = true;

        lineRenderer.enabled = true;
        lineRenderer.positionCount = 2;

        float currentSpeed = GetTongueSpeed();

        float t = 0f;
        float distance = Vector3.Distance(
            tongueOrigin.position,
            targetPos
        );

        Prey hitPrey = null;
        ThrowableObject hitThrowable = null; 
        bool hitObstacle = false;

        // Zunge ausfahren
        while (t < 1f)
        {
            t += Time.deltaTime * currentSpeed / distance;

            Vector3 start = tongueOrigin.position;
            Vector3 pos = Vector3.Lerp(start, targetPos, t);

            if(tongueTip != null) tongueTip.position = pos;

            if (Physics.Raycast(
                start,
                (pos - start).normalized,
                out RaycastHit hit,
                Vector3.Distance(start, pos),
                collisionMask))
            {
                // Fall 1: Beute getroffen
                if (hit.collider.TryGetComponent(out Prey prey))
                {
                    hitPrey = prey;
                }
                // Fall 2: Werfbares Objekt getroffen
                else if (hit.collider.TryGetComponent(out ThrowableObject throwable))
                {
                    hitThrowable = throwable;
                    hitObstacle = true;
                    targetPos = hit.point;
                    break;
                }
                // Fall 3: Normale Wand getroffen
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

        // Objekte an der Spitze befestigen
        if (!hitObstacle && hitPrey != null)
        {
            hitPrey.AttachToTongue(tongueTip);
        }
        else if (hitThrowable != null)
        {
            hitThrowable.transform.SetParent(tongueTip);
            if(hitThrowable.GetComponent<Rigidbody>() != null) 
                hitThrowable.GetComponent<Rigidbody>().isKinematic = true;
        }

        // Zunge zurückziehen
        float backT = 1f;

        while (backT > 0f)
        {
            backT -= Time.deltaTime * currentSpeed / distance;

            Vector3 start = tongueOrigin.position;
            Vector3 pos = Vector3.Lerp(start, targetPos, backT);

            if(tongueTip != null) tongueTip.position = pos;

            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, pos);

            yield return null;
        }

        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;

        // Endstation: Essen oder Halten
        if (hitPrey != null)
        {
            hitPrey.AllowEat();
        }
        else if (hitThrowable != null)
        {
            hitThrowable.VonZungeGefangen(); 
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