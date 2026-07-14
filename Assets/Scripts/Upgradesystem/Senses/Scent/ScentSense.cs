using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class ScentSense : MonoBehaviour
{
    [Header("Upgrade")]
    [SerializeField] private bool needsUpgrade = true;

    [Header("Input")]
    [SerializeField] private Key key = Key.G;

    [Header("References")]
    [SerializeField] private Transform playerPosition;
    [SerializeField] private LineRenderer linePrefab;

    [Header("Scent Settings")]
    [SerializeField] private float searchRadius = 40f;
    [SerializeField] private LayerMask preyMask;
    [SerializeField] private float pathHeightOffset = 0.15f;
    [SerializeField] private float navMeshSampleDistance = 10f;

    [Header("Timing")]
    [SerializeField] private float activeTime = 5f;
    [SerializeField] private float cooldown = 5f;
    [SerializeField] private float fadeOutTime = 1.2f;

    [Header("Line Look")]
    [SerializeField] private float lineWidth = 0.35f;

    [Header("Line Animation")]
    [SerializeField] private float scrollSpeed = 0.6f;
    [SerializeField] private float textureTilingX = 10f;

    [Header("Materials")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material upgradeMaterial;
    [SerializeField] private Material poisonMaterial;

    [Header("Audio")]
    [SerializeField] private AudioSource ScentSound;

    [Header("UI")]
    [SerializeField] private PlayerAbilityUI abilityUI;

    private UpgradeSystem upgradeSystem;
    private bool isActive;
    private bool isOnCooldown;
    private readonly List<LineRenderer> activeLines = new List<LineRenderer>();

    private void Awake()
    {
        ResolveRuntimeReferences();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current[key].wasPressedThisFrame)
            TryUseScentSense();

        AnimateLines();
    }

    public void TryUseScentSense()
    {
        ResolveRuntimeReferences();

        if (isActive || isOnCooldown)
        {
            Debug.Log("Geruchssinn ist noch im Cooldown.");
            return;
        }

        if (needsUpgrade &&
            (upgradeSystem == null ||
             !upgradeSystem.HasUpgrade(PreyGivesUpgrade.ScentSense)))
        {
            Debug.Log("Geruchssinn noch nicht freigeschaltet.");
            abilityUI?.SetLocked(PreyGivesUpgrade.ScentSense);
            return;
        }

        StartCoroutine(ScentRoutine());

        if (ScentSound != null)
        {
            ScentSound.Stop();
            ScentSound.Play();
        }
    }

    private IEnumerator ScentRoutine()
    {
        isActive = true;
        isOnCooldown = true;

        abilityUI?.SetAbilityState(
            PreyGivesUpgrade.ScentSense,
            true,
            0f
        );

        ClearLines();

        float timer = 0f;

        while (timer < activeTime)
        {
            timer += Time.deltaTime;

            ClearLines();
            CreateAllScentPaths(timer / activeTime);

            yield return null;
        }

        yield return StartCoroutine(FadeOutLines());

        ClearLines();

        isActive = false;

        abilityUI?.SetAbilityState(
            PreyGivesUpgrade.ScentSense,
            false,
            1f
        );

        float cooldownTimer = cooldown;

        while (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;

            abilityUI?.SetAbilityState(
                PreyGivesUpgrade.ScentSense,
                false,
                cooldownTimer / cooldown
            );

            yield return null;
        }

        isOnCooldown = false;

        abilityUI?.SetReady(PreyGivesUpgrade.ScentSense);

        Debug.Log("Geruchssinn wieder bereit.");
    }

    private void CreateAllScentPaths(float progress)
    {
        ResolveRuntimeReferences();

        Collider[] hits = Physics.OverlapSphere(
            playerPosition.position,
            searchRadius,
            preyMask
        );

        HashSet<ScentTarget> foundTargets = new HashSet<ScentTarget>();

        foreach (Collider hit in hits)
        {
            ScentTarget target =
                hit.GetComponentInParent<ScentTarget>() ??
                hit.GetComponentInChildren<ScentTarget>();

            if (target == null || !target.canBeTracked)
                continue;

            if (foundTargets.Contains(target))
                continue;

            foundTargets.Add(target);
            DrawAnimatedPathToTarget(target, progress);
        }
    }
        private void DrawAnimatedPathToTarget(ScentTarget target, float progress)
    {
        if (linePrefab == null)
            return;

        Vector3 startPosition = playerPosition.position;
        Vector3 targetPosition = target.transform.position;

        if (!NavMesh.SamplePosition(
                startPosition,
                out NavMeshHit startHit,
                navMeshSampleDistance,
                NavMesh.AllAreas))
            return;

        if (!NavMesh.SamplePosition(
                targetPosition,
                out NavMeshHit navHit,
                navMeshSampleDistance,
                NavMesh.AllAreas))
            return;

        NavMeshPath path = new NavMeshPath();

        bool pathFound = NavMesh.CalculatePath(
            startHit.position,
            navHit.position,
            NavMesh.AllAreas,
            path
        );

        if (!pathFound || path.corners.Length < 2)
            return;

        List<Vector3> points = new List<Vector3>();

        for (int i = 0; i < path.corners.Length; i++)
        {
            Vector3 point = path.corners[i];
            point.y += pathHeightOffset;
            points.Add(point);
        }

        bool targetIsInAir = Vector3.Distance(navHit.position, targetPosition) > 0.5f;

        if (targetIsInAir)
        {
            Vector3 airPoint = targetPosition;
            airPoint.y += pathHeightOffset;
            points.Add(airPoint);
        }

        List<Vector3> animatedPoints = GetPartialPath(points, progress);

        if (animatedPoints.Count < 2)
            return;

        LineRenderer line = Instantiate(linePrefab);
        line.useWorldSpace = true;
        line.textureMode = LineTextureMode.Tile;
        line.alignment = LineAlignment.View;
        line.widthMultiplier = lineWidth;

        Material mat = new Material(GetMaterial(target.scentType));
        mat.mainTextureScale = new Vector2(textureTilingX, 1f);
        mat.mainTextureOffset = new Vector2(-Time.time * scrollSpeed, 0f);
        line.material = mat;

        line.positionCount = animatedPoints.Count;

        for (int i = 0; i < animatedPoints.Count; i++)
            line.SetPosition(i, animatedPoints[i]);

        activeLines.Add(line);
    }

    private void AnimateLines()
    {
        foreach (LineRenderer line in activeLines)
        {
            if (line == null || line.material == null)
                continue;

            Vector2 offset = line.material.mainTextureOffset;
            offset.x -= scrollSpeed * Time.deltaTime;
            line.material.mainTextureOffset = offset;
        }
    }

    private List<Vector3> GetPartialPath(List<Vector3> points, float progress)
    {
        List<Vector3> result = new List<Vector3>();

        float totalLength = GetPathLength(points);
        float targetLength = totalLength * progress;
        float currentLength = 0f;

        result.Add(points[0]);

        for (int i = 1; i < points.Count; i++)
        {
            Vector3 previous = points[i - 1];
            Vector3 current = points[i];

            float segmentLength = Vector3.Distance(previous, current);

            if (currentLength + segmentLength <= targetLength)
            {
                result.Add(current);
                currentLength += segmentLength;
            }
            else
            {
                float remaining = targetLength - currentLength;
                float t = Mathf.Clamp01(remaining / segmentLength);

                result.Add(Vector3.Lerp(previous, current, t));
                break;
            }
        }

        return result;
    }

    private float GetPathLength(List<Vector3> points)
    {
        float length = 0f;

        for (int i = 1; i < points.Count; i++)
            length += Vector3.Distance(points[i - 1], points[i]);

        return length;
    }

    private IEnumerator FadeOutLines()
    {
        float timer = 0f;

        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeOutTime);

            foreach (LineRenderer line in activeLines)
            {
                if (line == null || line.material == null)
                    continue;

                Color color = line.material.color;
                color.a = alpha;
                line.material.color = color;

                if (line.material.HasProperty("_BaseColor"))
                {
                    Color baseColor = line.material.GetColor("_BaseColor");
                    baseColor.a = alpha;
                    line.material.SetColor("_BaseColor", baseColor);
                }
            }

            yield return null;
        }
    }

    private Material GetMaterial(ScentType type)
    {
        switch (type)
        {
            case ScentType.Upgrade:
                return upgradeMaterial;

            case ScentType.Poison:
                return poisonMaterial;

            default:
                return normalMaterial;
        }
    }

    private void ClearLines()
    {
        for (int i = 0; i < activeLines.Count; i++)
        {
            if (activeLines[i] != null)
                Destroy(activeLines[i].gameObject);
        }

        activeLines.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = playerPosition != null ? playerPosition : transform;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(origin.position, searchRadius);
    }

    public bool IsActive => isActive;
    public bool IsOnCooldown => isOnCooldown;

    private void ResolveRuntimeReferences()
    {
        if (upgradeSystem == null ||
            !VRUIRuntimeSupport.IsLikelyVrPlayer(upgradeSystem.transform))
        {
            upgradeSystem =
                VRUIRuntimeSupport.FindBestUpgradeSystem() ??
                GetComponentInParent<UpgradeSystem>() ??
                GetComponentInChildren<UpgradeSystem>(true);
        }

        if (abilityUI == null)
            abilityUI = VRUIRuntimeSupport.FindBestPlayerAbilityUI();

        if (VRUIRuntimeSupport.IsLikelyVrScene())
        {
            Transform body = VRUIRuntimeSupport.FindBestPlayerBodyTransform();
            if (body != null)
                playerPosition = body;
        }
        else if (playerPosition == null)
        {
            playerPosition = transform;
        }
    }
}
