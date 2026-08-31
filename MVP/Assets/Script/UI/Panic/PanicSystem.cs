using UnityEngine;
using UnityEngine.UI;

public class PanicSystem : MonoBehaviour
{
    [Header("References")]
    public HandHoldSystem handHoldSystem;
    public Transform playerA;
    public Transform playerB;
    public Slider panicBar;

    [Header("Panic Value")]
    public float currentPanic = 0f;
    public float maxPanic = 100f;

    [Header("Distance Settings")]
    [Tooltip("When the two players are within this distance (but not holding hands), Panic will stay the same.")]
    public float safeDistance = 5f;

    [Header("Change Speed")]
    public float increaseSpeed = 8f;
    public float decreaseSpeed = 15f;

    [Header("Tutorial")]
    [SerializeField]
    private bool panicEnabled = true;

    public bool PanicEnabled => panicEnabled;

    public void SetPanicEnabled(bool enabled)
    {
        panicEnabled = enabled;

        if (!panicEnabled)
        {
            ResetPanic();
        }
    }
    public bool IsMaxPanic => currentPanic >= maxPanic;

    public float NormalizedPanic
    {
        get
        {
            if (maxPanic <= 0f)
                return 0f;

            return Mathf.Clamp01(currentPanic / maxPanic);
        }
    }
    public float PlayerDistance
    {
        get
        {
            if (playerA == null || playerB == null)
                return Mathf.Infinity;

            return Vector3.Distance(playerA.position, playerB.position);
        }
    }

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
        if (!panicEnabled)
        {
            return;
        }

        if (handHoldSystem == null)
            return;

        UpdatePanic();
        UpdateUI();
    }

    void UpdatePanic()
    {
        //T, decreases
        if (handHoldSystem.IsHoldingHands)
        {
            currentPanic -= decreaseSpeed * Time.deltaTime;
        }
        // Close , same
        else if (PlayerDistance <= safeDistance)
        {
            //nothing
        }
        // Far increases
        else
        {
            currentPanic += increaseSpeed * Time.deltaTime;
        }

        currentPanic = Mathf.Clamp(currentPanic, 0f, maxPanic);
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
        currentPanic = Mathf.Clamp(currentPanic, 0f, maxPanic);
        UpdateUI();
    }

    public void ReducePanic(float amount)
    {
        currentPanic -= amount;
        currentPanic = Mathf.Clamp(currentPanic, 0f, maxPanic);
        UpdateUI();
    }

    public void SetPanic(float amount)
    {
        currentPanic = Mathf.Clamp(amount, 0f, maxPanic);
        UpdateUI();
    }

    public void ResetPanic()
    {
        currentPanic = 0f;
        UpdateUI();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (playerA == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(playerA.position, safeDistance);
    }
#endif
}