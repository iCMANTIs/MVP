using System.Collections.Generic;
using UnityEngine;

public class AxeHitbox : MonoBehaviour
{
    [Header("Hitbox")]
    public float enemyStunDuration = 2f;

    private Collider hitbox;
    private bool canHit;

    private HashSet<EnemyAI> hitEnemies = new HashSet<EnemyAI>();

    private void Awake()
    {
        hitbox = GetComponent<Collider>();

        if (hitbox != null)
        {
            hitbox.isTrigger = true;
            hitbox.enabled = false;
        }
    }

    public void EnableHitbox()
    {
        canHit = true;
        hitEnemies.Clear();

        if (hitbox != null)
            hitbox.enabled = true;

        Debug.Log("Axe hitbox enabled");
    }

    public void DisableHitbox()
    {
        canHit = false;

        if (hitbox != null)
            hitbox.enabled = false;

        Debug.Log("Axe hitbox disabled");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canHit)
            return;

        // 1. obstacle
        DestructibleObstacle obstacle =
    other.GetComponentInParent<DestructibleObstacle>();

        if (obstacle != null)
        {
            obstacle.DestroyByWeapon();

            Debug.Log(
                "Destroyed obstacle loudly: " +
                obstacle.name
            );

            DisableHitbox();
            return;
        }

        // enemy
        EnemyAI enemy =
            other.GetComponentInParent<EnemyAI>();

        if (enemy != null)
        {
            if (hitEnemies.Contains(enemy))
                return;

            hitEnemies.Add(enemy);

            enemy.Stun(enemyStunDuration);

            Debug.Log("Enemy stunned by axe: " + enemy.name);

            DisableHitbox();
            return;
        }
    }
}