using UnityEngine;

public class BladeDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField]
    private int damage = 1;

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth health =
            other.GetComponentInParent<PlayerHealth>();

        if (health == null)
            return;

        health.TakeDamage(damage);

        Debug.Log(
            $"{name} hit {health.gameObject.name}"
        );
    }
}