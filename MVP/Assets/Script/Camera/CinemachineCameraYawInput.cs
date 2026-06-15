using UnityEngine;

public class CinemachineCameraYawInput : MonoBehaviour
{
    [Header("Player Input")]
    public PlayerController playerAInput;
    public PlayerController_B playerBInput;

    [Header("Yaw")]
    public float yawSpeed = 120f;
    public float yaw;

    void Start()
    {
        yaw = transform.eulerAngles.y;
    }

    void LateUpdate()
    {
        Vector2 lookInput = Vector2.zero;

        if (playerAInput != null)
            lookInput = playerAInput.LookInput;
        else if (playerBInput != null)
            lookInput = playerBInput.LookInput;

        yaw += lookInput.x * yawSpeed * Time.deltaTime;

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }
}