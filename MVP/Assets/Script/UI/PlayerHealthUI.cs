using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;

    [Tooltip("draw to square")]
    [SerializeField] private Image[] healthBlocks;

    [Header("Colors")]
    [SerializeField] private Color fullColor = Color.red;

    [SerializeField]
    private Color emptyColor =
        new Color(0.2f, 0.2f, 0.2f, 0.65f);

    private void Awake()
    {
        if (playerHealth == null)
        {
            Debug.LogWarning(
                "PlayerHealthUI has no PlayerHealth assigned.",
                this
            );
        }
    }

    private void OnEnable()
    {
        if (playerHealth == null)
            return;

        playerHealth.HealthChanged += OnHealthChanged;

        RefreshUI();
    }

    private void OnDisable()
    {
        if (playerHealth == null)
            return;

        playerHealth.HealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(PlayerHealth health)
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (playerHealth == null)
            return;

        if (healthBlocks == null)
            return;

        for (int i = 0; i < healthBlocks.Length; i++)
        {
            if (healthBlocks[i] == null)
                continue;

            bool isFull =
                i < playerHealth.CurrentHealth;

            healthBlocks[i].color =
                isFull ? fullColor : emptyColor;
        }
    }
}