using UnityEngine;

public class Trap : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField]
    private DestructibleObstacle destructibleObstacle;

    [Header("Trap Damage")]
    [SerializeField]
    private int damage = 1;

    [SerializeField]
    private bool triggerOnlyOnce = true;

    private bool hasTriggered;

    private void Awake()
    {
        if (destructibleObstacle == null)
        {
            destructibleObstacle =
                GetComponent<DestructibleObstacle>();
        }
    }

    public void Interact(GameObject interactor)
    {
        if (destructibleObstacle == null)
        {
            Debug.LogError(
                $"{name}: DestructibleObstacle is missing.",
                this
            );

            return;
        }

        if (destructibleObstacle.destroyed)
            return;

        PlayerController sister =
            interactor.GetComponentInParent<PlayerController>();

        if (sister == null)
        {
            Debug.Log(
                $"{interactor.name} cannot disarm {name}."
            );

            return;
        }
        hasTriggered = true;
        destructibleObstacle.DestroyObstacle();

        Debug.Log(
            $"Sister disarmed trap: {name}"
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trap Trigger Enter: " + other.name);
        if (triggerOnlyOnce && hasTriggered)
            return;

        if (destructibleObstacle != null &&
            destructibleObstacle.destroyed)
            return;

        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
            return;

        playerHealth.TakeDamage(damage);

        Debug.Log(
            $"{playerHealth.name} triggered trap {name}"
        );

        hasTriggered = true;
    }
}
