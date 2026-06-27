using UnityEngine;
using UnityEngine.AI;

public class GameDirectorAI : MonoBehaviour
{
    [Header("References")]
    public EnemyAI enemy;
    public Transform brother;
    public Transform sister;

    [Header("Pressure Input")]
    public float panicValue;
    public bool isHoldingHands;

    [Header("Director Timing")]
    public float decisionInterval = 5f;
    private float decisionTimer;

    [Header("Threat")]
    public float threat;
    public float lowThreatThreshold = 25f;
    public float highThreatThreshold = 80f;

    [Header("Hint")]
    public float hintOffsetRadius = 6f;

    [Header("Back Off")]
    public float backOffDistance = 15f;

    private void Update()
    {
        decisionTimer -= Time.deltaTime;

        if (decisionTimer <= 0f)
        {
            decisionTimer = decisionInterval;
            UpdateDirector();
        }
    }

    private void UpdateDirector()
    {
        if (enemy == null || brother == null || sister == null)
            return;

        threat = CalculateThreat();

        if (threat < lowThreatThreshold)
        {
            Vector3 hint = GetHintNearPlayers();
            enemy.ReceiveDirectorHint(hint);
        }
        else if (threat > highThreatThreshold)
        {
            Vector3 safePos = GetBackOffPosition();
            enemy.BackOff(safePos);
        }
    }

    private float CalculateThreat()
    {
        float distance = Vector3.Distance(brother.position, sister.position);

        float separationThreat = distance * 5f;
        float panicThreat = panicValue;

        float holdingHandsBonus = isHoldingHands ? -20f : 20f;

        float result = panicThreat + separationThreat + holdingHandsBonus;

        return Mathf.Clamp(result, 0f, 100f);
    }

    private Vector3 GetHintNearPlayers()
    {
        Vector3 center = (brother.position + sister.position) * 0.5f;

        Vector2 randomCircle = Random.insideUnitCircle * hintOffsetRadius;
        Vector3 randomPoint = center + new Vector3(randomCircle.x, 0f, randomCircle.y);

        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, hintOffsetRadius, NavMesh.AllAreas))
        {
            Debug.DrawLine(enemy.transform.position, hit.position, Color.magenta, 3f);
            return hit.position;
        }

        return center;
    }

    private Vector3 GetBackOffPosition()
    {
        Vector3 playersCenter = (brother.position + sister.position) * 0.5f;
        Vector3 awayDir = (enemy.transform.position - playersCenter).normalized;

        Vector3 target = enemy.transform.position + awayDir * backOffDistance;

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, backOffDistance, NavMesh.AllAreas))
        {
            Debug.DrawLine(enemy.transform.position, hit.position, Color.white, 3f);
            return hit.position;
        }

        return enemy.transform.position;
    }
}