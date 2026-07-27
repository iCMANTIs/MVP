using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction Detection")]
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float interactionRadius = 1.5f;
    [SerializeField] private LayerMask interactableLayer;

    public bool TryInteract()
    {
        Vector3 detectionPosition =
            interactionPoint != null
                ? interactionPoint.position
                : transform.position;

        Collider[] detectedColliders = Physics.OverlapSphere(
            detectionPosition,
            interactionRadius,
            interactableLayer,
            QueryTriggerInteraction.Collide
        );

        IInteractable closestInteractable = null;
        float closestDistance = float.MaxValue;

        foreach (Collider detectedCollider in detectedColliders)
        {
            IInteractable interactable =
                detectedCollider.GetComponentInParent<IInteractable>();

            if (interactable == null)
                continue;

            float distance = Vector3.Distance(
                detectionPosition,
                detectedCollider.ClosestPoint(detectionPosition)
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestInteractable = interactable;
            }
        }

        if (closestInteractable == null)
            return false;

        closestInteractable.Interact(gameObject);
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 detectionPosition =
            interactionPoint != null
                ? interactionPoint.position
                : transform.position;

        Gizmos.DrawWireSphere(
            detectionPosition,
            interactionRadius
        );
    }
}