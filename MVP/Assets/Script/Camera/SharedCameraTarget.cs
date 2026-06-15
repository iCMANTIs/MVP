using UnityEngine;

public class SharedCameraTarget : MonoBehaviour
{
    public Transform playerA;
    public Transform playerB;

    [Header("Input")]
    public PlayerController playerAInput;
    public PlayerController_B playerBInput;

    [Header("Position")]
    public float height = 1.2f;

    [Header("Yaw")]
    public float yawSpeed = 120f;
    public float yaw;

    void Start()
    {
        yaw = transform.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (playerA == null || playerB == null)
            return;

        Vector3 center = (playerA.position + playerB.position) * 0.5f;
        transform.position = center + Vector3.up * height;

        Vector2 lookA = playerAInput != null ? playerAInput.LookInput : Vector2.zero;
        Vector2 lookB = playerBInput != null ? playerBInput.LookInput : Vector2.zero;

        Vector2 sharedLook = lookA + lookB;

        if (sharedLook.magnitude > 1f)
            sharedLook.Normalize();

        yaw += sharedLook.x * yawSpeed * Time.deltaTime;

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }
}