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

    [Header("UI Freischaltungen")]
    public TutorialPopup tutorialPopup; // Ziehe hier das Tutorial-Objekt rein
    public Map mapScript;               // Ziehe hier das Map-Objekt rein

    [Header("Settings")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private LayerMask collisionMask;

    [Header("Upgrade")]
    [SerializeField] private float tongueSpeedBonus = 10f;

    private UpgradeSystem upgradeSystem;
    private bool isBusy;
    private bool hasUsedTongueOnce = false; // Merkt sich den ersten Klick

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
        // NEU: Einmaliger UI-Check beim ersten Schießen
        if (!hasUsedTongueOnce)
        {
            // 1. Tutorial ausblenden
            if (tutorialPopup != null) 
            {
                tutorialPopup.HideTutorial();
            }
            
            // 2. Map einblenden
            if (mapScript != null) 
            {
                mapScript.ShowMap();
            }
            
            hasUsedTongueOnce = true; 
        }

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
        bool hitObstacle = false;

        // Zunge ausfahren
        while (t < 1f)
        {
            t += Time.deltaTime * currentSpeed / distance;

            Vector3 start = tongueOrigin.position;
            Vector3 pos = Vector3.Lerp(start, targetPos, t);

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