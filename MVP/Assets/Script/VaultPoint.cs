using UnityEngine;

public class VaultPoint : MonoBehaviour
{
    [Header("Vault Points")]
    public Transform startPoint;
    public Transform endPoint;

    [Header("Vault Settings")]
    public bool requireFacing = true;
    public float maxFacingAngle = 90f;

    public bool CanVault(Transform player)
    {
        if (!requireFacing)
            return true;

        Vector3 toVault = transform.position - player.position;
        toVault.y = 0f;

        if (toVault.sqrMagnitude < 0.001f)
            return true;

        float angle = Vector3.Angle(player.forward, toVault.normalized);

        return angle <= maxFacingAngle;
    }
}