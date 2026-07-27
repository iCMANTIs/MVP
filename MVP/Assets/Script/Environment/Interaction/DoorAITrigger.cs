using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoorAITrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Door door;

    private Collider triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }

        if (door == null)
        {
            door = GetComponentInParent<Door>();
        }

        if (door == null)
        {
            Debug.LogError(
                $"{name}: Door reference is missing.",
                this
            );
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (door == null)
            return;

        EnemyAI enemy =
            other.GetComponentInParent<EnemyAI>();

        if (enemy == null)
            return;

        door.OpenForAI();

        Debug.Log(
            $"{enemy.name} approached {door.name}. " +
            "Door opened."
        );
    }
}