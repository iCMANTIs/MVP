using UnityEngine;

public class RotatingBlade : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField]
    private Transform bladePivot;

    [SerializeField]
    private float rotationSpeed = 360f;

    [SerializeField]
    private bool isActive = true;

    [SerializeField]
    private BladeDamage[] bladeDamages;
    public bool IsActive => isActive;

    [Header("Path Blocker")]
    [SerializeField]
    private Collider pathBlocker;

    private void Update()
    {
        if (!isActive)
            return;

        if (bladePivot == null)
            return;

        bladePivot.Rotate(
            Vector3.down,
            rotationSpeed * Time.deltaTime,
            Space.Self
        );
    }

    public void DisableBlade()
    {
        isActive = false;

        foreach (BladeDamage bladeDamage in bladeDamages)
        {
            if (bladeDamage != null)
                bladeDamage.enabled = false;
        }

        if (pathBlocker != null)
            pathBlocker.enabled = false;
    }

    public void EnableBlade()
    {
        isActive = true;

        foreach (BladeDamage bladeDamage in bladeDamages)
        {
            if (bladeDamage != null)
                bladeDamage.enabled = true;
        }

        if (pathBlocker != null)
            pathBlocker.enabled = true;
    }
}