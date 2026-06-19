using UnityEngine;

public class BrotherWeaponController : MonoBehaviour
{
    [Header("References")]
    public Transform axe;

    public Transform axeBackSocket;
    public Transform axeHandSocket;

    [Header("State")]
    public bool isArmed = false;
    public AxeHitbox axeHitbox;

    public void EnableAxeHitbox()
    {
        if (axeHitbox != null)
            axeHitbox.EnableHitbox();
    }

    public void DisableAxeHitbox()
    {
        if (axeHitbox != null)
            axeHitbox.DisableHitbox();
    }
    private void Start()
    {
        PutAxeOnBack();
    }

    // Animation Event
    public void PutAxeInHand()
    {
        if (axe == null || axeHandSocket == null)
            return;

        axe.SetParent(axeHandSocket, false);

     
        axe.localPosition = Vector3.zero;
        axe.localRotation = Quaternion.identity;
        axe.localScale = Vector3.one;

        isArmed = true;
    }

    // Animation Event
    public void PutAxeOnBack()
    {
        if (axe == null || axeBackSocket == null)
            return;

        axe.SetParent(axeBackSocket, false);

        axe.localPosition = Vector3.zero;
        axe.localRotation = Quaternion.identity;
        axe.localScale = Vector3.one;

        isArmed = false;
    }
}