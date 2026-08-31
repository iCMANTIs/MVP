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

    [Header("Spot Detection")]
    public float maxSpotDistance = 15f;
    public LayerMask environmentMask;

    [HideInInspector]
    public bool hasValidSpot;

    [HideInInspector]
    public Vector3 currentSpotPosition;

    [HideInInspector]
    public Vector3 currentSpotNormal;

    [Header("Panic Flicker")]
    [SerializeField] private PanicSystem panicSystem;

    [Range(0f, 1f)]
    [SerializeField] private float flickerStartPanic = 0.6f;

    [Range(0f, 1f)]
    [SerializeField] private float minimumIntensityMultiplier = 0.25f;

    [SerializeField] private float flickerSpeed = 18f;

    [Range(0f, 1f)]
    [SerializeField] private float flickerStrength = 0.75f;

    private float normalLightIntensity;


    public bool showDebugSpot = true;

    private FlashlightReveal currentReveal;

    private void Start()
    {
        if (panicSystem == null)
        {
            panicSystem = FindAnyObjectByType<PanicSystem>();
        }

        if (flashlightLight != null)
        {
            normalLightIntensity = flashlightLight.intensity;
        }

        PutFlashlightInHolster();
        SetLight(false);
    }

    private void Update()
    {
        UpdatePanicFlicker();
        UpdateSpot();
    }

    // Animation Event
    public void PutFlashlightInHand()
    {
        if (flashlight == null || flashlightHandSocket == null)
            return;

        flashlight.SetParent(flashlightHandSocket, false);
        isFlashlightInHand = true;
        SetLight(true);
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
        {
            flashlightLight.enabled = on;

            if (on)
            {
                flashlightLight.intensity =
                    normalLightIntensity;
            }
        }

        if (!on)
        {
            hasValidSpot = false;
        }
    }

    public void ToggleLight()
    {
        if (!isFlashlightInHand)
            return;

        SetLight(!isLightOn);
    }

    private void UpdateSpot()
    {
        hasValidSpot = false;

        if (!isLightOn)
        {
            ClearReveal();
            return;
        }

        if (flashlightLight == null)
        {
            ClearReveal();
            return;
        }

        Transform lightTransform =
            flashlightLight.transform;

        if (Physics.Raycast(
            lightTransform.position,
            lightTransform.forward,
            out RaycastHit hit,
            maxSpotDistance,
            environmentMask,
            QueryTriggerInteraction.Ignore))
        {
            hasValidSpot = true;

            currentSpotPosition = hit.point;
            currentSpotNormal = hit.normal;

            FlashlightReveal reveal =
                hit.collider.GetComponentInParent<FlashlightReveal>();

            if (reveal != currentReveal)
            {
                ClearReveal();

                currentReveal = reveal;

                if (currentReveal != null)
                    currentReveal.Reveal();
            }

            if (showDebugSpot)
            {
                Debug.DrawLine(
                    lightTransform.position,
                    hit.point,
                    Color.yellow
                );

                Debug.DrawRay(
                    hit.point,
                    hit.normal * 0.25f,
                    Color.cyan
                );
            }

            return;
        }

        ClearReveal();
    }

    private void ClearReveal()
    {
        if (currentReveal != null)
        {
            currentReveal.Hide();
            currentReveal = null;
        }
    }

    private void UpdatePanicFlicker()
    {
        if (flashlightLight == null)
            return;

        if (!isLightOn)
            return;

        if (panicSystem == null)
        {
            flashlightLight.intensity = normalLightIntensity;
            return;
        }

        float panic = panicSystem.NormalizedPanic;

        if (panic <= flickerStartPanic)
        {
            flashlightLight.intensity = normalLightIntensity;
            return;
        }

        float panicEffect = Mathf.InverseLerp(flickerStartPanic,1f,panic);

        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed,0f);

        float effectiveStrength =flickerStrength * panicEffect;

        float brightnessMultiplier = Mathf.Lerp(1f,noise,effectiveStrength);

        brightnessMultiplier = Mathf.Max(brightnessMultiplier,minimumIntensityMultiplier);

        flashlightLight.intensity =normalLightIntensity * brightnessMultiplier;
    }
}