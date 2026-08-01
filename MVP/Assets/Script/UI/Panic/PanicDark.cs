using UnityEngine;
using UnityEngine.UI;

public class PanicDark : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PanicSystem panicSystem;
    [SerializeField] private Image darknessOverlay;

    [Header("Darkness Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float darknessStartPanic = 0.4f;

    [Range(0f, 1f)]
    [SerializeField] private float maximumDarknessAlpha = 0.75f;

    [SerializeField] private float smoothSpeed = 4f;

    private float currentAlpha;

    private void Awake()
    {
        SetOverlayAlpha(0f);
    }

    private void Update()
    {
        UpdateDarkness();
    }

    private void UpdateDarkness()
    {
        if (darknessOverlay == null)
            return;

        if (panicSystem == null)
        {
            currentAlpha = Mathf.MoveTowards(
                currentAlpha,
                0f,
                smoothSpeed * Time.deltaTime
            );

            SetOverlayAlpha(currentAlpha);
            return;
        }

        float panic = panicSystem.NormalizedPanic;
        float targetAlpha = 0f;

        if (panic > darknessStartPanic)
        {
            float panicEffect = Mathf.InverseLerp(darknessStartPanic,1f,panic);

            panicEffect = Mathf.SmoothStep(0f,1f,panicEffect);

            targetAlpha =panicEffect * maximumDarknessAlpha;
        }

        currentAlpha = Mathf.Lerp(
            currentAlpha,
            targetAlpha,
            smoothSpeed * Time.deltaTime
        );

        SetOverlayAlpha(currentAlpha);
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (darknessOverlay == null)
            return;

        Color color = darknessOverlay.color;
        color.r = 0f;
        color.g = 0f;
        color.b = 0f;
        color.a = Mathf.Clamp01(alpha);

        darknessOverlay.color = color;
    }

    private void OnDisable()
    {
        currentAlpha = 0f;
        SetOverlayAlpha(0f);
    }
}