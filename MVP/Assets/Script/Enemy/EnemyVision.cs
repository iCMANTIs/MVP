using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    public EnemyAI enemyAI;

    [Header("Targets")]
    public Transform brother;
    public Transform sister;

    [Header("Vision")]
    public float viewDistance = 10f;
    public float viewAngle = 80f;
    public float eyeHeight = 1.5f;
    public float loseSightTime = 2f;

    [Header("Debug")]
    public bool showDebugRays = true;

    private Transform seenTarget;
    private float loseSightTimer;

    private void Awake()
    {
        if (enemyAI == null)
            enemyAI = GetComponent<EnemyAI>();
    }

    private void Update()
    {
        DrawVisionCone();

        Transform visibleTarget = GetVisibleTarget();

        if (visibleTarget != null)
        {
            seenTarget = visibleTarget;
            loseSightTimer = loseSightTime;
            enemyAI.SeePlayer(visibleTarget);
        }
        else
        {
            if (seenTarget != null)
            {
                loseSightTimer -= Time.deltaTime;

                if (loseSightTimer <= 0f)
                {
                    seenTarget = null;
                    enemyAI.LosePlayer();
                }
            }
        }
    }

    private Transform GetVisibleTarget()
    {
        if (CanSeeTarget(brother))
            return brother;

        if (CanSeeTarget(sister))
            return sister;

        return null;
    }

    private bool CanSeeTarget(Transform target)
    {
        if (target == null)
            return false;

        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPosition = target.position + Vector3.up * 1f;

        Vector3 dirToTarget = targetPosition - eyePosition;
        float distance = dirToTarget.magnitude;

        if (showDebugRays)
        {
            Debug.DrawRay(eyePosition, dirToTarget.normalized * distance, Color.gray);
        }

        if (distance > viewDistance)
            return false;

        float angle = Vector3.Angle(transform.forward, dirToTarget.normalized);

        if (angle > viewAngle * 0.5f)
            return false;

        if (Physics.Raycast(eyePosition, dirToTarget.normalized, out RaycastHit hit, viewDistance))
        {
            if (showDebugRays)
            {
                Color rayColor = Color.red;

                if (hit.transform == target || hit.transform.IsChildOf(target))
                    rayColor = Color.green;

                Debug.DrawRay(eyePosition, dirToTarget.normalized * hit.distance, rayColor);
            }

            if (hit.transform == target || hit.transform.IsChildOf(target))
                return true;
        }

        return false;
    }

    private void DrawVisionCone()
    {
        if (!showDebugRays)
            return;

        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;

        Vector3 leftDir = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0, viewAngle * 0.5f, 0) * transform.forward;

        Debug.DrawRay(eyePosition, transform.forward * viewDistance, Color.blue);
        Debug.DrawRay(eyePosition, leftDir * viewDistance, Color.cyan);
        Debug.DrawRay(eyePosition, rightDir * viewDistance, Color.cyan);
    }
}