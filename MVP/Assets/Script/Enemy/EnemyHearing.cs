using UnityEngine;

public class EnemyHearing : MonoBehaviour
{
    public EnemyAI enemyAI;

    private void Awake()
    {
        if (enemyAI == null)
            enemyAI = GetComponent<EnemyAI>();
    }

    public void ReceiveSound(Vector3 soundPosition, float loudness)
    {
        float distance = Vector3.Distance(transform.position, soundPosition);

        if (distance <= loudness)
        {
            enemyAI.HearSound(soundPosition);
        }
    }
}