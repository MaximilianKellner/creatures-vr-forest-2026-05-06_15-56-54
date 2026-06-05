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

    private bool isBusy;

    private void Awake()
    {
        lineRenderer.enabled = false;
    }

    private void Update()
    {
        if (!isBusy && Mouse.current.leftButton.wasPressedThisFrame)
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

        float t = 0f;
        float distance = Vector3.Distance(tongueOrigin.position, targetPos);

        Prey hitPrey = null;
        bool hitObstacle = false;

        while (t < 1f)
        {
            t += Time.deltaTime * speed / distance;

            Vector3 start = tongueOrigin.position;
            Vector3 pos = Vector3.Lerp(start, targetPos, t);

            if (Physics.Raycast(start, (pos - start).normalized, out RaycastHit hit, Vector3.Distance(start, pos), collisionMask))
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

        //  Wenn Beute getroffen wird
        if (!hitObstacle && hitPrey != null)
        {
            hitPrey.AttachToTongue(tongueTip); //Dann heftet es an der Zunge
        }

        // zurückziehen
        float backT = 1f;

        while (backT > 0f)
        {
            backT -= Time.deltaTime * speed / distance;

            Vector3 start = tongueOrigin.position;
            Vector3 pos = Vector3.Lerp(start, targetPos, backT);

            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, pos);

            yield return null;
        }

        lineRenderer.positionCount = 0;
        lineRenderer.enabled = false;

        isBusy = false;
    }
}