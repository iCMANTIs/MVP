using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] private PlayerHealth sisterHealth;
    [SerializeField] private PlayerHealth brotherHealth;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;

    [Header("Settings")]
    [SerializeField] private bool pauseOnGameOver = true;

    private bool gameOverTriggered;

    private void Awake()
    {

        Time.timeScale = 1f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (sisterHealth != null)
        {
            sisterHealth.HealthChanged += OnPlayerHealthChanged;
            sisterHealth.PlayerDowned += OnPlayerDowned;
        }

        if (brotherHealth != null)
        {
            brotherHealth.HealthChanged += OnPlayerHealthChanged;
            brotherHealth.PlayerDowned += OnPlayerDowned;
        }
    }

    private void OnDisable()
    {
        if (sisterHealth != null)
        {
            sisterHealth.HealthChanged -= OnPlayerHealthChanged;
            sisterHealth.PlayerDowned -= OnPlayerDowned;
        }

        if (brotherHealth != null)
        {
            brotherHealth.HealthChanged -= OnPlayerHealthChanged;
            brotherHealth.PlayerDowned -= OnPlayerDowned;
        }
    }

    private void Start()
    {
        CheckGameOver();
    }

    private void OnPlayerHealthChanged(PlayerHealth playerHealth)
    {
        CheckGameOver();
    }

    private void OnPlayerDowned(PlayerHealth playerHealth)
    {
        CheckGameOver();
    }

    private void CheckGameOver()
    {
        if (gameOverTriggered)
            return;

        if (sisterHealth == null || brotherHealth == null)
            return;

        bool sisterIsDowned =
            sisterHealth.CurrentHealth <= 0 ||
            sisterHealth.IsDowned;

        bool brotherIsDowned =
            brotherHealth.CurrentHealth <= 0 ||
            brotherHealth.IsDowned;

        if (sisterIsDowned && brotherIsDowned)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        if (gameOverTriggered)
            return;

        gameOverTriggered = true;

        Debug.Log("Both players are down. Game Over.");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "Game Over Panel is not assigned.",
                this
            );
        }

        if (pauseOnGameOver)
        {
            Time.timeScale = 0f;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.buildIndex
        );
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}