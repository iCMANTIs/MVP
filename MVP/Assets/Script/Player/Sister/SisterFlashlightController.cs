using UnityEngine;

public class SisterFlashlightController : MonoBehaviour
{
    [Header("References")]
    public Transform flashlight;
    public Transform flashlightHolsterSocket;
    public Transform flashlightHandSocket;
    public Light flashlightLight;

    [Header("State")]
    public bool isFlashlightInHand;
    public bool isLightOn;

    private void Start()
    {
        PutFlashlightInHolster();
        SetLight(false);
    }

    // Animation Event
    public void PutFlashlightInHand()
    {
        if (flashlight == null || flashlightHandSocket == null)
            return;

        flashlight.SetParent(flashlightHandSocket, false);
        isFlashlightInHand = true;
    }

    // Animation Event
    public void PutFlashlightInHolster()
    {
        if (flashlight == null || flashlightHolsterSocket == null)
            return;

        flashlight.SetParent(flashlightHolsterSocket, false);
        isFlashlightInHand = false;
        SetLight(false);
    }

    public void SetLight(bool on)
    {
        isLightOn = on;

        if (flashlightLight != null)
            flashlightLight.enabled = on;
    }

    public void ToggleLight()
    {
        if (!isFlashlightInHand)
            return;

        SetLight(!isLightOn);
    }
}