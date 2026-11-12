using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;

    [Header("Player Components")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerPowerPallet playerPowerPallet;

    [Header("Game HUD - Health")]
    [SerializeField] private Image healthFillImage;

    [Header("Game HUD - PowerPallet")]
    [SerializeField] private GameObject powerImage;
    [SerializeField] private Image powerFillImage;

    [Header("Game HUD - Counters")]
    [SerializeField] private TMP_Text gameScoreText;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text starsText;

    [Header("Other Score Texts")]
    [SerializeField] private TMP_Text mainMenuScoreText;
    [SerializeField] private TMP_Text gameOverScoreText;
    [SerializeField] private TMP_Text victoryScoreText;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button mainSoundOnButton;
    [SerializeField] private Button mainSoundOffButton;

    [Header("Game Buttons")]
    [SerializeField] private Button gamePauseButton;

    [Header("Pause Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseMainMenuButton;
    [SerializeField] private Button pauseRestartButton;
    [SerializeField] private Button pauseSoundOnButton;
    [SerializeField] private Button pauseSoundOffButton;

    [Header("Game Over Buttons")]
    [SerializeField] private Button gameOverRestartButton;
    [SerializeField] private Button gameOverMainMenuButton;

    [Header("Victory Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button victoryRestartButton;
    [SerializeField] private Button victoryMainMenuButton;

    [Header("Victory Settings")]
    [SerializeField] private int maxScore = 50;

    private int score;
    private int coins;
    private int stars;
    private int bestScore;

    private bool isPlaying;
    private bool isPaused;
    private bool gameEnded;
    private bool soundEnabled;
    private bool hasStartedGame;

    // Automatically start gameplay after reloading a scene.
    private static bool startGameAfterLoad;

    private bool shouldAutoStart;

    private const string BestScoreKey = "BestScore";
    private const string SoundKey = "SoundEnabled";

    private void Awake()
    {
        Time.timeScale = 1f;

        shouldAutoStart = startGameAfterLoad;
        startGameAfterLoad = false;

        bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        soundEnabled = PlayerPrefs.GetInt(SoundKey, 1) == 1;

        if (playerHealth == null)
        {
            playerHealth =
                FindFirstObjectByType<PlayerHealth>();
        }

        if (playerPowerPallet == null)
        {
            playerPowerPallet =
                FindFirstObjectByType<PlayerPowerPallet>();
        }

        if (audioManager == null)
            audioManager = FindFirstObjectByType<AudioManager>();

        RegisterButtons();
        ApplySound();
    }

    private void Start()
    {
        score = 0;
        coins = 0;
        stars = 0;

        if (shouldAutoStart)
        {
            StartGame();
        }
        else
        {
            ShowMainMenu();
        }
    }

    private void Update()
    {
        HandlePauseInput();

        if (!isPlaying || isPaused || gameEnded)
            return;

        UpdateHealthUI();
        UpdatePowerUI();

        CheckPlayerDeath();
    }

    // ==================================
    // BUTTONS
    // ==================================

    private void RegisterButtons()
    {
        BindButton(playButton, PlayGame);
        BindButton(quitButton, QuitGame);

        BindButton(mainSoundOnButton, DisableSound);
        BindButton(mainSoundOffButton, EnableSound);

        BindButton(gamePauseButton, PauseGame);

        BindButton(resumeButton, ResumeGame);
        BindButton(pauseMainMenuButton, ShowMainMenu);
        BindButton(pauseRestartButton, RestartGame);

        BindButton(pauseSoundOnButton, DisableSound);
        BindButton(pauseSoundOffButton, EnableSound);

        BindButton(gameOverRestartButton, RestartGame);
        BindButton(gameOverMainMenuButton, ShowMainMenu);

        BindButton(nextButton, NextLevel);
        BindButton(victoryRestartButton, RestartGame);
        BindButton(victoryMainMenuButton, ShowMainMenu);
    }

    private void BindButton(
        Button button,
        UnityAction action
    )
    {
        if (button != null)
        {
            button.onClick.AddListener(PlayButtonSound);
            button.onClick.AddListener(action);
        }
    }

    private AudioManager GetAudioManager()
    {
        return audioManager != null ? audioManager : AudioManager.Instance;
    }

    private void PlayButtonSound()
    {
        GetAudioManager()?.PlayButton();
    }

    // ==================================
    // PANELS
    // ==================================

    private void HideAllPanels()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (gamePanel != null)
            gamePanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }

    public void ShowMainMenu()
    {
        isPlaying = false;
        isPaused = false;
        gameEnded = false;

        HideAllPanels();

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        UpdateScoreUI();
        UpdateSoundButtons();

        GetAudioManager()?.PlayMainMenu();

        // Freeze gameplay while showing the menu.
        Time.timeScale = 0f;
    }

    public void PlayGame()
    {
        // If the player returns to the main menu after
        // playing, reload the scene for a clean restart.
        if (hasStartedGame)
        {
            ReloadSceneAndPlay();
            return;
        }

        StartGame();
    }

    private void StartGame()
    {
        Time.timeScale = 1f;

        hasStartedGame = true;

        isPlaying = true;
        isPaused = false;
        gameEnded = false;

        score = 0;
        coins = 0;
        stars = 0;

        HideAllPanels();

        if (gamePanel != null)
            gamePanel.SetActive(true);

        UpdateAllUI();
        GetAudioManager()?.PlayGameplay();
    }

    public void PauseGame()
    {
        if (!isPlaying || isPaused || gameEnded)
            return;

        isPaused = true;

        Time.timeScale = 0f;

        HideAllPanels();

        if (pausePanel != null)
            pausePanel.SetActive(true);

        UpdateSoundButtons();
        GetAudioManager()?.PlayPause();
    }

    public void ResumeGame()
    {
        if (!isPaused || gameEnded)
            return;

        isPaused = false;
        isPlaying = true;

        Time.timeScale = 1f;

        HideAllPanels();

        if (gamePanel != null)
            gamePanel.SetActive(true);

        UpdateAllUI();
        GetAudioManager()?.PlayGameplay();
    }

    public void RestartGame()
    {
        ReloadSceneAndPlay();
    }

    private void ReloadSceneAndPlay()
    {
        Time.timeScale = 1f;

        startGameAfterLoad = true;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void ShowGameOver()
    {
        if (gameEnded || !isPlaying)
            return;

        gameEnded = true;
        isPlaying = false;
        isPaused = false;

        SaveBestScore();

        HideAllPanels();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        UpdateScoreUI();

        GetAudioManager()?.PlayGameOver();
        Time.timeScale = 0f;
    }

    public void ShowVictory()
    {
        if (gameEnded || !isPlaying)
            return;

        gameEnded = true;
        isPlaying = false;
        isPaused = false;

        SaveBestScore();

        HideAllPanels();

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        UpdateScoreUI();

        GetAudioManager()?.PlayVictory();
        Time.timeScale = 0f;
    }

    public void NextLevel()
    {
        int nextIndex =
            SceneManager.GetActiveScene().buildIndex + 1;

        if (nextIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning("Next level is not configured!");
            return;
        }

        Time.timeScale = 1f;

        startGameAfterLoad = true;

        SceneManager.LoadScene(nextIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

        PlayerPrefs.Save();

        Debug.Log("Quit Game");

        Application.Quit();
    }

    // ==================================
    // PAUSE INPUT
    // ==================================

    private void HandlePauseInput()
    {
        if (Keyboard.current == null)
            return;

        if (!Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        if (gameEnded)
            return;

        if (isPaused)
        {
            ResumeGame();
        }
        else if (isPlaying)
        {
            PauseGame();
        }
    }

    // ==================================
    // HEALTH
    // ==================================

    private void UpdateHealthUI()
    {
        if (playerHealth == null ||
            healthFillImage == null)
            return;

        float currentHealth =
            playerHealth.GetCurrentHealth();

        float maxHealth =
            playerHealth.GetMaxHealth();

        float healthPercent = maxHealth > 0f
            ? currentHealth / maxHealth
            : 0f;

        healthFillImage.fillAmount =
            Mathf.Clamp01(healthPercent);
    }

    private void CheckPlayerDeath()
    {
        if (playerHealth == null)
            return;

        if (playerHealth.GetCurrentHealth() <= 0f)
        {
            ShowGameOver();
        }
    }

    // ==================================
    // POWER PALLET
    // ==================================

    private void UpdatePowerUI()
    {
        if (powerImage == null ||
            playerPowerPallet == null)
        {
            if (powerImage != null)
                powerImage.SetActive(false);

            return;
        }

        bool isPowered =
            playerPowerPallet.IsPoweredUp();

        float remainingTime =
            playerPowerPallet.GetRemainingPowerTime();

        float totalDuration =
            playerPowerPallet.GetPowerDuration();

        // Show the entire Power HUD only while active.
        bool shouldShow =
            isPowered && remainingTime > 0f;

        if (powerImage.activeSelf != shouldShow)
        {
            powerImage.SetActive(shouldShow);
        }

        if (!shouldShow)
        {
            if (powerFillImage != null)
                powerFillImage.fillAmount = 0f;

            return;
        }

        float powerPercent = totalDuration > 0f
            ? remainingTime / totalDuration
            : 0f;

        if (powerFillImage != null)
        {
            powerFillImage.fillAmount =
                Mathf.Clamp01(powerPercent);
        }
    }

    // ==================================
    // SCORE, COINS, STARS
    // ==================================


    public void AddScore(int amount)
    {
        if (!isPlaying || isPaused || gameEnded)
            return;

        if (amount <= 0)
            return;

        // Score cannot exceed maxScore
        score = Mathf.Clamp(
            score + amount,
            0,
            maxScore
        );

        // Update ScoreText
        UpdateScoreUI();

        Debug.Log("Current Score: " + score + "/" + maxScore);

        // Check victory condition
        if (score >= maxScore)
        {
            Debug.Log("VICTORY! Maximum Score reached!");

            ShowVictory();
        }
    }


    public void AddCoins(int amount)
    {
        if (!isPlaying || isPaused || gameEnded)
            return;

        coins = Mathf.Max(0, coins + amount);

        UpdateCountersUI();
    }

    public void AddStars(int amount)
    {
        if (!isPlaying || isPaused || gameEnded)
            return;

        stars = Mathf.Max(0, stars + amount);

        UpdateCountersUI();
    }

    private void UpdateScoreUI()
    {
        if (gameScoreText != null)
            gameScoreText.text = score.ToString("N0");

        if (mainMenuScoreText != null)
            mainMenuScoreText.text = bestScore.ToString("N0");

        if (gameOverScoreText != null)
            gameOverScoreText.text = score.ToString("N0");

        if (victoryScoreText != null)
            victoryScoreText.text = score.ToString("N0");
    }

    private void UpdateCountersUI()
    {
        if (coinsText != null)
            coinsText.text = coins.ToString();

        if (starsText != null)
            starsText.text = stars.ToString();
    }

    private void SaveBestScore()
    {
        if (score <= bestScore)
            return;

        bestScore = score;

        PlayerPrefs.SetInt(
            BestScoreKey,
            bestScore
        );

        PlayerPrefs.Save();
    }

    // ==================================
    // SOUND
    // ==================================

    public void EnableSound()
    {
        soundEnabled = true;
        ApplySound();
    }

    public void DisableSound()
    {
        soundEnabled = false;
        ApplySound();
    }

    private void ApplySound()
    {
        AudioListener.volume =
            soundEnabled ? 1f : 0f;

        PlayerPrefs.SetInt(
            SoundKey,
            soundEnabled ? 1 : 0
        );

        PlayerPrefs.Save();

        UpdateSoundButtons();
    }

    private void UpdateSoundButtons()
    {
        if (mainSoundOnButton != null)
            mainSoundOnButton.gameObject.SetActive(soundEnabled);

        if (mainSoundOffButton != null)
            mainSoundOffButton.gameObject.SetActive(!soundEnabled);

        if (pauseSoundOnButton != null)
            pauseSoundOnButton.gameObject.SetActive(soundEnabled);

        if (pauseSoundOffButton != null)
            pauseSoundOffButton.gameObject.SetActive(!soundEnabled);
    }

    // ==================================
    // UI REFRESH
    // ==================================

    private void UpdateAllUI()
    {
        UpdateHealthUI();
        UpdatePowerUI();
        UpdateScoreUI();
        UpdateCountersUI();
        UpdateSoundButtons();
    }

    public bool IsGameplayActive() => isPlaying && !isPaused && !gameEnded;

    public int GetScore() => score;
    public int GetCoins() => coins;
    public int GetStars() => stars;
}
