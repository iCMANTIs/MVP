using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PanicDark : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PanicSystem panicSystem;

    [SerializeField]
    private Volume panicVolume;

    [Header("Darkness Settings")]
    [Range(0f, 1f)]
    [SerializeField]
    private float darknessStartPanic = 0.4f;

    [SerializeField]
    private float maximumDarknessExposure = -2.5f;

    [SerializeField]
    private float smoothSpeed = 4f;

    private ColorAdjustments colorAdjustments;

    private float currentExposure = 0f;

    private void Awake()
    {
        if (panicVolume != null &&
            panicVolume.profile != null)
        {
            panicVolume.profile.TryGet(
                out colorAdjustments
            );
        }

        SetExposure(0f);
    }

    private void Update()
    {
        UpdateDarkness();
    }

    private void UpdateDarkness()
    {
        if (colorAdjustments == null)
            return;

        if (panicSystem == null)
        {
            currentExposure = Mathf.Lerp(
                currentExposure,
                0f,
                smoothSpeed * Time.deltaTime
            );

            SetExposure(currentExposure);
            return;
        }

        float panic = panicSystem.NormalizedPanic;

        float targetExposure = 0f;

        if (panic > darknessStartPanic)
        {
            float panicEffect = Mathf.InverseLerp(
                darknessStartPanic,
                1f,
                panic
            );

            panicEffect = Mathf.SmoothStep(
                0f,
                1f,
                panicEffect
            );

            targetExposure = Mathf.Lerp(
                0f,
                maximumDarknessExposure,
                panicEffect
            );
        }

        currentExposure = Mathf.Lerp(
            currentExposure,
            targetExposure,
            smoothSpeed * Time.deltaTime
        );

        SetExposure(currentExposure);
    }

    private void SetExposure(float exposure)
    {
        if (colorAdjustments == null)
            return;

        colorAdjustments.postExposure.value =
            exposure;
    }

    private void OnDisable()
    {
        currentExposure = 0f;
        SetExposure(0f);
    }
}