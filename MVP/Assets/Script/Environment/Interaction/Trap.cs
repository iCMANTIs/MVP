using UnityEngine;

public class Trap : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField]
    private DestructibleObstacle destructibleObstacle;

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

        destructibleObstacle.DestroyObstacle();

        Debug.Log(
            $"Sister disarmed trap: {name}"
        );
    }
}
