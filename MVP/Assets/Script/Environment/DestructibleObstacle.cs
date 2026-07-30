using UnityEngine;

public class DestructibleObstacle : MonoBehaviour
{
    [Header("Object Parts")]
    public GameObject modelRoot;
    public Collider hitCollider;
    public Collider blockCollider;

    [Header("State")]
    public bool destroyed;

    [Header("Weapon Destruction")]
    [Tooltip("被武器摧毁时，敌人能听见的范围")]
    [SerializeField]
    private float weaponNoiseRadius = 12f;

    [Tooltip("被武器摧毁时播放给玩家听的音效")]
    [SerializeField]
    private AudioClip weaponDestroySound;

    [Range(0f, 1f)]
    [SerializeField]
    private float weaponDestroyVolume = 1f;

    public void DestroyObstacle()
    {
        DestroyObstacleInternal(false);
    }


    public void DestroyByWeapon()
    {
        DestroyObstacleInternal(true);
    }

    private void DestroyObstacleInternal(bool emitNoise)
    {
        if (destroyed)
            return;

        destroyed = true;

        Vector3 destructionPosition = transform.position;

        if (emitNoise)
        {
            PlayWeaponDestroySound(destructionPosition);
            NotifyNearbyEnemies(destructionPosition);
        }

        if (modelRoot != null)
            modelRoot.SetActive(false);

        if (hitCollider != null)
            hitCollider.enabled = false;

        if (blockCollider != null)
            blockCollider.enabled = false;
    }

    private void PlayWeaponDestroySound(Vector3 position)
    {
        if (weaponDestroySound == null)
            return;

        AudioSource.PlayClipAtPoint(
            weaponDestroySound,
            position,
            weaponDestroyVolume
        );
    }

    private void NotifyNearbyEnemies(Vector3 soundPosition)
    {
        EnemyHearing[] enemyHearings =
            FindObjectsByType<EnemyHearing>();

        foreach (EnemyHearing hearing in enemyHearings)
        {
            if (hearing == null)
                continue;

            hearing.ReceiveSound(
                soundPosition,
                weaponNoiseRadius
            );
        }

        Debug.Log(
            $"{name} emitted destruction noise. " +
            $"Radius: {weaponNoiseRadius}"
        );
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            weaponNoiseRadius
        );
    }
#endif
}