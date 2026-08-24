using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("Sister2 brother3")]
    [SerializeField]
    private int maxHealth = 2;

    [SerializeField]
    private int currentHealth;

    [Header("Damage")]
    [Tooltip("interval")]
    [SerializeField]
    private float invulnerabilityDuration = 1f;

    [Header("Animation")]
    [SerializeField]
    private Animator animator;

    [Header("Revive")]
    [SerializeField] private PlayerHealth teammateHealth;

    [SerializeField] private float reviveDistance = 2f;
    [SerializeField] private float reviveDuration = 3f;
    [SerializeField] private int reviveHealth = 1;

    [Header("Revive UI")]
    [SerializeField] private GameObject reviveCanvas;
    [SerializeField] private Slider reviveSlider;

    private bool reviveInputHeld;
    private float reviveProgress;

    [Tooltip("EMPTY/HURT anim trigger")]
    [SerializeField]
    private string hurtTriggerName = "Hurt";

    [Tooltip(" EMPTY/down anim bool")]
    [SerializeField]
    private string downedBoolName = "IsDowned";

    private float invulnerabilityTimer;
    private bool isDowned;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDowned => isDowned;

    public event Action<PlayerHealth> HealthChanged;
    public event Action<PlayerHealth> PlayerDowned;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        currentHealth = maxHealth;
        isDowned = false;

        if (animator != null &&
            !string.IsNullOrEmpty(downedBoolName))
        {
            animator.SetBool(downedBoolName, false);
        }
    }

    private void Update()
    {
        if (invulnerabilityTimer > 0f)
        {
            invulnerabilityTimer -= Time.deltaTime;
        }
        UpdateRevive();
    }


    public void TakeDamage(int damageAmount)
    {
        if (damageAmount <= 0)
            return;

        if (isDowned)
            return;

        if (invulnerabilityTimer > 0f)
            return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(
            currentHealth,
            0,
            maxHealth
        );

        invulnerabilityTimer =
            invulnerabilityDuration;

        Debug.Log(
            gameObject.name +
            " took " +
            damageAmount +
            " damage. Health: " +
            currentHealth +
            "/" +
            maxHealth
        );

        HealthChanged?.Invoke(this);

        if (currentHealth <= 0)
        {
            EnterDownedState();
        }
        else
        {
            PlayHurtAnimation();
        }
    }

    public void Heal(int healAmount)
    {
        if (healAmount <= 0)
            return;

        if (isDowned)
            return;

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(
            currentHealth,
            0,
            maxHealth
        );

        HealthChanged?.Invoke(this);
    }

    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
        isDowned = false;
        invulnerabilityTimer = 0f;

        PlayerController sister = GetComponent<PlayerController>();
        if (sister != null)
            sister.canControl = true;

        PlayerController_B brother = GetComponent<PlayerController_B>();
        if (brother != null)
            brother.canControl = true;

        HealthChanged?.Invoke(this);

    }

    public void Revive(int reviveHealth = 1)
    {
        if (!isDowned)
            return;

        currentHealth = Mathf.Clamp(
            reviveHealth,
            1,
            maxHealth
        );

        isDowned = false;


        invulnerabilityTimer = invulnerabilityDuration;

        PlayerController sister =
            GetComponent<PlayerController>();

        if (sister != null)
        {
            sister.canControl = true;
        }

        PlayerController_B brother =
            GetComponent<PlayerController_B>();

        if (brother != null)
        {
            brother.canControl = true;
        }

        if (animator != null &&
            !string.IsNullOrEmpty(downedBoolName))
        {
            animator.SetBool(
                downedBoolName,
                false
            );

            animator.SetFloat(
                "Speed",
                0f
            );
        }

        Debug.Log(
            gameObject.name +
            " revived with " +
            currentHealth +
            " HP."
        );

        HealthChanged?.Invoke(this);
    }


    public void SetReviveInput(bool held)
    {
        reviveInputHeld = held;
    }

    private void UpdateRevive()
    {
        if (teammateHealth == null)
        {
            ResetReviveProgress();
            return;
        }

        if (isDowned)
        {
            ResetReviveProgress();
            return;
        }

        if (!teammateHealth.IsDowned)
        {
            ResetReviveProgress();
            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            teammateHealth.transform.position
        );

        if (distance > reviveDistance)
        {
            ResetReviveProgress();
            return;
        }

        if (teammateHealth.reviveCanvas != null)
        {
            teammateHealth.reviveCanvas.SetActive(true);
        }

        if (!reviveInputHeld)
        {
            reviveProgress = 0f;
            UpdateReviveSlider();
            return;
        }

        reviveProgress += Time.deltaTime;

        UpdateReviveSlider();

        if (reviveProgress >= reviveDuration)
        {
            teammateHealth.Revive(reviveHealth);

            ResetReviveProgress();
        }
    }

    private void UpdateReviveSlider()
    {
        if (teammateHealth == null)
            return;

        if (teammateHealth.reviveSlider == null)
            return;

        teammateHealth.reviveSlider.value = Mathf.Clamp01(reviveProgress / reviveDuration);
    }

    private void ResetReviveProgress()
    {
        reviveProgress = 0f;

        if (teammateHealth == null)
            return;

        if (teammateHealth.reviveSlider != null)
        {
            teammateHealth.reviveSlider.value = 0f;
        }

        if (teammateHealth.reviveCanvas != null)
        {
            teammateHealth.reviveCanvas.SetActive(false);
        }
    }
    public bool CanReviveTeammate()
    {
        if (isDowned)
            return false;

        if (teammateHealth == null)
            return false;

        if (!teammateHealth.IsDowned)
            return false;

        float distance = Vector3.Distance(
            transform.position,
            teammateHealth.transform.position
        );

        return distance <= reviveDistance;
    }

    private void PlayHurtAnimation()
    {
        if (animator == null)
            return;

        if (string.IsNullOrEmpty(hurtTriggerName))
            return;

        animator.SetTrigger(hurtTriggerName);
    }

    private void EnterDownedState()
    {
        if (isDowned)
            return;

        isDowned = true;
        currentHealth = 0;

        Debug.Log(
            gameObject.name + " is downed."
        );

        // ban movement
        PlayerController sister =
            GetComponent<PlayerController>();

        if (sister != null)
        {
            sister.canControl = false;
        }

        PlayerController_B brother =
            GetComponent<PlayerController_B>();

        if (brother != null)
        {
            brother.canControl = false;
        }

        // Animator
        if (animator != null)
        {
            animator.SetFloat(
                "Speed",
                0f
            );

            if (!string.IsNullOrEmpty(
                downedBoolName))
            {
                animator.SetBool(
                    downedBoolName,
                    true
                );
            }
        }

        PlayerDowned?.Invoke(this);
    }

    // test only -1
    [ContextMenu("Test Take 1 Damage")]
    private void TestTakeOneDamage()
    {
        TakeDamage(1);
    }

    // test only heal
    [ContextMenu("Test Restore Health")]
    private void TestRestoreHealth()
    {
        RestoreFullHealth();
    }
}