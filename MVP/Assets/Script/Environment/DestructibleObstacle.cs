using UnityEngine;

public class DestructibleObstacle : MonoBehaviour
{
    [Header("Object Parts")]
    public GameObject modelRoot;
    public Collider hitCollider;
    public Collider blockCollider;

    [Header("State")]
    public bool destroyed;

    public void DestroyObstacle()
    {
        if (destroyed) return;

        destroyed = true;

        if (modelRoot != null)
            modelRoot.SetActive(false);

        if (hitCollider != null)
            hitCollider.enabled = false;

        if (blockCollider != null)
            blockCollider.enabled = false;
    }
}