using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject gameplayPanel;
    public GameObject pauseMenuPanel;
    public GameObject gameOverPanel;

    [Header("Gameplay UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public GameObject scorePopupPrefab;
    public Transform scorePopupParent;
    public Button pauseButton;

    [Header("Game Over UI")]
    public TextMeshProUGUI finalScoreText;
    public Button restartButton;
    public Button mainMenuButton;

    [Header("Animation Settings")]
    public float scorePopupDuration = 1f;
    public float scorePopupDistance = 100f;

    private Game game;

    private void Start()
    {
        // Find game reference if not set
        if (game == null)
            game = FindAnyObjectByType<Game>();

        // Log warning if components are missing
        if (finalScoreText == null)
            Debug.LogWarning("Final Score Text not assigned in UIManager");
        if (gameOverPanel == null)
            Debug.LogWarning("Game Over Panel not assigned in UIManager");

        // Setup button listeners
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        SetupUI();
    }

    private void SetupUI()
    {
        if (pauseButton != null)
            pauseButton.onClick.AddListener(game.PauseGame);
        
        // Configure score text
        if (scoreText != null)
        {
            scoreText.fontSize = 72f; // Make the score much bigger
            RectTransform scoreRect = scoreText.GetComponent<RectTransform>();
            if (scoreRect != null)
            {
                scoreRect.anchoredPosition = new Vector2(0f, 500f);
            }
        }
        
        // Configure high score text style
        if (highScoreText != null)
        {
            highScoreText.fontSize = 50f; // 70% of main score size
            highScoreText.color = new Color(1f, 1f, 1f, 0.7f);
            
            RectTransform highScoreRect = highScoreText.GetComponent<RectTransform>();
            if (highScoreRect != null)
            {
                highScoreRect.anchoredPosition = new Vector2(-250f, 600f); // Further left and higher
            }
        }
        
        // Set up initial score display
        UpdateScore(0);
        UpdateHighScore(PlayerPrefs.GetInt("HighScore", 0));
    }

    public void UpdateUIForGameState(Game.State state)
    {
        // Null checks for panels
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(state == Game.State.Menu);
        if (gameplayPanel != null)
            gameplayPanel.SetActive(state == Game.State.Playing);
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(state == Game.State.Paused);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(state == Game.State.GameOver);

        if (state == Game.State.GameOver)
        {
            ShowGameOverUI();
        }
    }

    public void ShowScorePopup(int score, Vector3 position)
    {
        StartCoroutine(AnimateScorePopup(score, position));
    }

    private IEnumerator AnimateScorePopup(int score, Vector3 position)
    {
        GameObject popupObj = Instantiate(scorePopupPrefab, scorePopupParent);
        popupObj.transform.position = Camera.main.WorldToScreenPoint(position);
        TextMeshProUGUI popupText = popupObj.GetComponent<TextMeshProUGUI>();

        if (popupText != null)
        {
            popupText.text = $"+{score}";
            Vector3 startPos = popupObj.transform.position;
            Vector3 endPos = startPos + Vector3.up * scorePopupDistance;
            float elapsed = 0f;

            // Add scale animation
            popupObj.transform.localScale = Vector3.zero;
            
            while (elapsed < scorePopupDuration)
            {
                float t = elapsed / scorePopupDuration;
                
                // Ease out for position
                float positionT = 1 - (1 - t) * (1 - t);
                popupObj.transform.position = Vector3.Lerp(startPos, endPos, positionT);
                
                // Pop in, then fade out
                float scaleT = t < 0.3f ? t / 0.3f : 1f;
                float scale = Mathf.Sin(scaleT * Mathf.PI) * 1.2f;
                popupObj.transform.localScale = Vector3.one * scale;
                
                // Fade out in last half
                popupText.alpha = t < 0.5f ? 1f : 1f - ((t - 0.5f) / 0.5f);
                
                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(popupObj);
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"{score:N0}";
        }
    }

    public void UpdateHighScore(int highScore)
    {
        if (highScoreText != null)
        {
            highScoreText.text = $"Best: {highScore:N0}";
        }
    }

    private void ShowGameOverUI()
    {
        // Null checks for required components
        if (game == null || game.board == null || finalScoreText == null || 
            gameOverPanel == null)
        {
            Debug.LogError("Missing required components for game over UI");
            return;
        }

        // Make sure game over panel is active
        gameOverPanel.SetActive(true);
    
        // Set the final score
        int finalScore = game.board.GetScore();
        finalScoreText.text = $"Final Score: {finalScore:N0}";
    
        // Ensure buttons are properly set up
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartClicked);
        }
    
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
    
        StartCoroutine(AnimateGameOverPanel());
    }

    private IEnumerator AnimateGameOverPanel()
    {
        if (gameOverPanel == null) yield break;

        // Get or add CanvasGroup to the game over panel
        CanvasGroup group = gameOverPanel.GetComponent<CanvasGroup>();
        if (group == null)
            group = gameOverPanel.AddComponent<CanvasGroup>();

        // Make sure panel starts fully transparent
        group.alpha = 0;
        gameOverPanel.SetActive(true);
    
        // Add a semi-transparent black background to darken the game board
        GameObject overlay = new GameObject("GameOverOverlay");
        overlay.transform.SetParent(gameOverPanel.transform, false);
        overlay.transform.SetAsFirstSibling(); // Put it behind other game over UI elements
    
        RectTransform overlayRect = overlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.sizeDelta = Vector2.zero;
    
        Image overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = new Color(0, 0, 0, 0.7f); // Semi-transparent black
    
        // Animate the fade in
        float elapsed = 0;
        float duration = 0.5f;
    
        // Ensure game board is still visible but faded
        if (game != null && game.board != null)
        {
            game.board.FadeBlocks(0.5f); // Set blocks to 50% opacity
        }
    
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(0, 1, elapsed / duration);
            yield return null;
        }
    
        // Ensure the panel and its elements are fully visible
        group.alpha = 1;
    
        // Make sure buttons are interactable
        if (restartButton != null) restartButton.interactable = true;
        if (mainMenuButton != null) mainMenuButton.interactable = true;
    }

    // UI Button Callbacks
    public void OnStartGameClicked()
    {
        // Reset score display before starting new game
        UpdateScore(0);
        game.StartGame();
    }

    public void OnResumeClicked()
    {
        game.ResumeGame();
    }

    public void OnRestartClicked()
    {
        Debug.Log("Restart button clicked");
        if (game != null)
        {
            // Make sure game over panel is hidden
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        
            // Reset score display before restarting
            UpdateScore(0);
        
            // Restart the game
            game.RestartGame();
        }
    }
    public void OnMainMenuClicked()
    {
        Debug.Log("Main menu button clicked");
        if (game != null)
        {
            // Reset time scale in case it was paused
            Time.timeScale = 1;
        
            // Hide game over panel
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        
            // Clear the board when returning to menu
            if (game.board != null)
            {
                game.board.ResetBoard();
            }
        
            // Clean up blocks
            BlockManager blockManager = FindAnyObjectByType<BlockManager>();
            if (blockManager != null)
            {
                blockManager.CleanupBlocks();
            }
        
            // Reset score display
            UpdateScore(0);
        
            // Show main menu
            UpdateUIForGameState(Game.State.Menu);
        }
    }
    private void OnDestroy()
    {
        // Clean up button listeners
        if (restartButton != null)
            restartButton.onClick.RemoveListener(OnRestartClicked);
        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
    }
}