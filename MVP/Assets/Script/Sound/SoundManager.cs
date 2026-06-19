using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public EnemyHearing[] enemies;

    private void Awake()
    {
        Instance = this;
    }

    public void EmitSound(Vector3 position, float loudness)
    {
        Debug.Log("Sound emitted. Radius: " + loudness);

        if (enemies == null || enemies.Length == 0)
        {
            Debug.LogWarning("SoundManager has no enemies assigned.");
            return;
        }

        foreach (EnemyHearing enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.ReceiveSound(position, loudness);
            }
        }
    }
}