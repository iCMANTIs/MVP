using UnityEngine;
using UnityEngine.UI;

public class PanicSystem : MonoBehaviour
{
    [Header("References")]
    public HandHoldSystem handHoldSystem;
    public Slider panicBar;

    [Header("Panic Value")]
    public float currentPanic = 0f;
    public float maxPanic = 100f;

    [Header("Change Speed")]
    public float increaseSpeed = 8f;
    public float decreaseSpeed = 15f;

    public bool IsMaxPanic => currentPanic >= maxPanic;

    void Start()
    {
        currentPanic = 0f;

        if (panicBar != null)
        {
            panicBar.minValue = 0f;
            panicBar.maxValue = maxPanic;
            panicBar.value = currentPanic;
        }
    }

    void Update()
    {
        if (handHoldSystem == null)
            return;

        UpdatePanic();

        UpdateUI();
    }

    void UpdatePanic()
    {
        if (handHoldSystem.IsHoldingHands)
        {
            currentPanic -= decreaseSpeed * Time.deltaTime;
        }
        else
        {
            currentPanic += increaseSpeed * Time.deltaTime;
        }

        currentPanic = Mathf.Clamp(
            currentPanic,
            0f,
            maxPanic
        );
    }

    void UpdateUI()
    {
        if (panicBar == null)
            return;

        panicBar.value = currentPanic;
    }

    public void AddPanic(float amount)
    {
        currentPanic += amount;

        currentPanic = Mathf.Clamp(
            currentPanic,
            0f,
            maxPanic
        );

        UpdateUI();
    }

    public void ReducePanic(float amount)
    {
        currentPanic -= amount;

        currentPanic = Mathf.Clamp(
            currentPanic,
            0f,
            maxPanic
        );

        UpdateUI();
    }

    public void SetPanic(float amount)
    {
        currentPanic = Mathf.Clamp(
            amount,
            0f,
            maxPanic
        );

        UpdateUI();
    }

    public void ResetPanic()
    {
        currentPanic = 0f;

        UpdateUI();
    }
}