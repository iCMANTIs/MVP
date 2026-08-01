using UnityEngine;

public class PanicCameraShake : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PanicSystem panicSystem;

    [Header("Panic Threshold")]
    [Range(0f, 1f)]
    [SerializeField] private float shakeStartPanic = 0.6f;

    [Header("Shake Settings")]

    [SerializeField] private float maxPositionStrength = 0.08f;
    [SerializeField] private float maxRotationStrength = 1.2f;
    [SerializeField] private float shakeSpeed = 18f;
    [SerializeField] private float recoverySpeed = 10f;

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    private float noiseSeedX;
    private float noiseSeedY;
    private float noiseSeedZ;

    private void Awake()
    {
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;

        noiseSeedX = Random.Range(0f, 100f);
        noiseSeedY = Random.Range(100f, 200f);
        noiseSeedZ = Random.Range(200f, 300f);
    }

    private void LateUpdate()
    {
        if (panicSystem == null)
        {
            RecoverToOriginal();
            return;
        }

        float panic = panicSystem.NormalizedPanic;

        if (panic <= shakeStartPanic)
        {
            RecoverToOriginal();
            return;
        }

        float shakeAmount = Mathf.InverseLerp(
            shakeStartPanic,
            1f,
            panic
        );

        ApplyShake(shakeAmount);
    }

    private void ApplyShake(float shakeAmount)
    {
        float time = Time.time * shakeSpeed;

        float xNoise =
            Mathf.PerlinNoise(noiseSeedX, time) * 2f - 1f;

        float yNoise =
            Mathf.PerlinNoise(noiseSeedY, time) * 2f - 1f;

        float zNoise =
            Mathf.PerlinNoise(noiseSeedZ, time) * 2f - 1f;

        Vector3 positionOffset = new Vector3(
            xNoise,
            yNoise,
            0f
        ) * maxPositionStrength * shakeAmount;

        Vector3 rotationOffset = new Vector3(
            yNoise,
            xNoise,
            zNoise
        ) * maxRotationStrength * shakeAmount;

        transform.localPosition =
            originalLocalPosition + positionOffset;

        transform.localRotation =
            originalLocalRotation *
            Quaternion.Euler(rotationOffset);
    }

    private void RecoverToOriginal()
    {
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            originalLocalPosition,
            recoverySpeed * Time.deltaTime
        );

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            originalLocalRotation,
            recoverySpeed * Time.deltaTime
        );
    }

    private void OnDisable()
    {
        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;
    }
}