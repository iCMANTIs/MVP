using UnityEngine;

public class CinemachineCoopCameraManager : MonoBehaviour
{
    public HandHoldSystem handHoldSystem;

    public GameObject cameraLeft;
    public GameObject cameraRight;
    public GameObject cameraShared;

    private bool usingShared;

    void Start()
    {
        SetSplitScreen();
    }

    void Update()
    {
        if (handHoldSystem.IsHoldingHands && !usingShared)
            SetSharedScreen();
        else if (!handHoldSystem.IsHoldingHands && usingShared)
            SetSplitScreen();
    }

    void SetSplitScreen()
    {
        usingShared = false;

        cameraLeft.SetActive(true);
        cameraRight.SetActive(true);
        cameraShared.SetActive(false);
    }

    void SetSharedScreen()
    {
        usingShared = true;

        cameraLeft.SetActive(false);
        cameraRight.SetActive(false);
        cameraShared.SetActive(true);
    }
}