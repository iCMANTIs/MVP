using System;
using UnityEngine;

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

        HealthChanged?.Invoke(this);
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

        Debug.Log(gameObject.name + " is downed.");

        // ban movement
        PlayerController sister = GetComponent<PlayerController>();
        if (sister != null)
            sister.canControl = false;

        PlayerController_B brother = GetComponent<PlayerController_B>();
        if (brother != null)
            brother.canControl = false;

        // stop movement animation  
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
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