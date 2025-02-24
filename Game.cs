using UnityEngine;
using UnityEngine.Events;

public class Game : MonoBehaviour
{
    public enum State
    {
        Menu,
        Playing,
        Paused,
        GameOver
    }

    [Header("Game Objects")]
    public GameBoard board;
    public BlockManager blockManager;
    public UIManager uiManager;

    [Header("Audio")]
    public AudioClip blockPlaceSound;
    public AudioClip lineClearSound;
    public AudioClip gameOverSound;
    public AudioClip rotateSound;

    public State currentState { get; private set; }
    private AudioSource audioSource;
    private VFXManager vfxManager;

    private void Start()
    {
        // Initialize components
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;  // Prevent auto-playing
        }
        
        // Find VFX Manager if not assigned
        if (vfxManager == null)
        {
            vfxManager = FindAnyObjectByType<VFXManager>();
        }

        // Validate required components
        if (board == null)
        {
            Debug.LogWarning("GameBoard not assigned to Game component");
        }
        if (blockManager == null)
        {
            Debug.LogWarning("BlockManager not assigned to Game component");
        }
        if (uiManager == null)
        {
            Debug.LogWarning("UIManager not assigned to Game component");
        }

        // Start in menu state
        SetState(State.Menu);
    }

    public void StartGame()
    {
        // Make sure we have required components
        if (board == null || blockManager == null)
        {
            Debug.LogError("Required components missing. Cannot start game.");
            return;
        }

        SetState(State.Playing);
        board.ResetBoard();
        board.ShowGrid(true);
        blockManager.SpawnInitialBlocks();
    }

    public void PauseGame()
    {
        if (currentState == State.Playing)
        {
            SetState(State.Paused);
            Time.timeScale = 0;
        }
    }

    public void ResumeGame()
    {
        if (currentState == State.Paused)
        {
            SetState(State.Playing);
            Time.timeScale = 1;
        }
    }

    public void EndGame()
    {
        if (currentState != State.GameOver)
        {
            SetState(State.GameOver);
        
            // Play game over sound
            PlaySound(gameOverSound);
        
            // Play VFX if available
            if (vfxManager != null && board != null)
            {
                Vector3 centerPos = board.transform.position + new Vector3(
                    board.columns * board.cellSize / 2,
                    board.rows * board.cellSize / 2,
                    0
                );
                vfxManager.PlayGameOverEffect(centerPos);
            }
        
            // Save high score if needed
            if (board != null)
            {
                int currentScore = board.GetScore();
                int highScore = PlayerPrefs.GetInt("HighScore", 0);
                if (currentScore > highScore)
                {
                    PlayerPrefs.SetInt("HighScore", currentScore);
                    PlayerPrefs.Save();
                }
            }
        
            // Handle block cleanup and fading
            if (blockManager != null)
            {
                blockManager.OnGameOver();
            }
        }
    }

    public void RestartGame()
    {
        // Reset time scale
        Time.timeScale = 1;
        
        // Re-enable block manager
        if (blockManager != null)
        {
            blockManager.enabled = true;
        }
        
        StartGame();
    }

    public void PlayBlockPlaceSound()
    {
        PlaySound(blockPlaceSound);
    }

    public void PlayLineClearSound()
    {
        PlaySound(lineClearSound);
    }

    public void PlayRotateSound()
    {
        PlaySound(rotateSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null && audioSource.isActiveAndEnabled)
        {
            try
            {
                audioSource.PlayOneShot(clip);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to play sound: {e.Message}");
            }
        }
    }

    private void SetState(State newState)
    {
        currentState = newState;
        
        // Update UI if available
        if (uiManager != null)
        {
            uiManager.UpdateUIForGameState(currentState);
        }
    }

    public bool IsPlaying()
    {
        return currentState == State.Playing;
    }

    public bool IsGameOver()
    {
        return currentState == State.GameOver;
    }

    public State GetState()
    {
        return currentState;
    }

    private void OnDisable()
    {
        // Clean up audio source when disabled
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}