using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class BlockManager : MonoBehaviour
{
    [Header("References")]
    public GameObject blockPrefab;
    public Transform spawnArea;
    public GameBoard gameBoard;
    public Game game;
    public Canvas gameCanvas;
    
    [Header("Block Settings")]
    public float blockSpacing = 2f;
    public float bottomOffset = 2f;
    public float spawnAreaScale = 0.4f;
    public float draggedScale = 0.8f;
    
    [Header("Preview Settings")]
    public Color validPlacementColor = new Color(0.5f, 1f, 0.5f, 0.5f);
    public Color invalidPlacementColor = new Color(1f, 0.5f, 0.5f, 0.5f);

    private Block[] availableBlocks = new Block[3];
    private int placedBlockCount = 0;
    private Camera mainCamera;
    private List<Vector2Int> validPositions = new List<Vector2Int>();

    private void Start()
    {
        SetupUI();
    }

    private void SetupUI()
    {
        mainCamera = Camera.main;
        
        // Find or setup Canvas
        if (gameCanvas == null)
        {
            gameCanvas = FindAnyObjectByType<Canvas>();
            if (gameCanvas == null)
            {
                GameObject canvasObj = new GameObject("GameCanvas");
                gameCanvas = canvasObj.AddComponent<Canvas>();
                gameCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                gameCanvas.worldCamera = mainCamera;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
        }

        // Ensure Canvas is properly configured
        gameCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        gameCanvas.worldCamera = mainCamera;

        if (gameCanvas.GetComponent<GraphicRaycaster>() == null)
        {
            gameCanvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        // Setup EventSystem if needed
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        // Ensure blocks can receive input
        foreach (Block block in FindObjectsByType<Block>(FindObjectsSortMode.None))
        {
            if (block.gameObject.GetComponent<BoxCollider2D>() == null)
            {
                block.gameObject.AddComponent<BoxCollider2D>().isTrigger = true;
            }
            
            // Make sure blocks are children of the canvas
            block.transform.SetParent(gameCanvas.transform, true);
        }
    }

    public void SpawnInitialBlocks()
    {
        // Only spawn blocks if we're in the playing state
        if (game.currentState != Game.State.Playing)
        {
            return;
        }

        CleanupBlocks();
        PositionBlocksForMobile();
        placedBlockCount = 0;

        // Spawn all three blocks
        for (int i = 0; i < 3; i++)
        {
            SpawnPlaceableBlock(i);
        }

        if (!CanAnyBlockBePlaced())
        {
            game.EndGame();
        }
    }

    private void SpawnPlaceableBlock(int index)
    {
        Vector3 spawnPosition = spawnArea.position + new Vector3(index * blockSpacing, 0, 0);
        
        // If this is the first block (index 0), choose any random block
        // For subsequent blocks, consider existing blocks and available spaces
        int blockType;
        if (index == 0)
        {
            blockType = GetRandomBlockType();
        }
        else
        {
            blockType = SelectCompatibleBlockType(index);
        }
        
        GameObject blockObj = Instantiate(blockPrefab, spawnPosition, Quaternion.identity);
        Block block = blockObj.GetComponent<Block>();
        
        if (block == null)
        {
            block = blockObj.AddComponent<Block>();
        }
        
        block.InitializeBlock(blockType);
        block.transform.localScale = Vector3.one * spawnAreaScale;
        
        availableBlocks[index] = block;
        
        // Update valid positions after spawning each block
        UpdateValidPositions();
    }

    private int GetRandomBlockType()
    {
        float random = Random.value;
        if (random < 0.65f) // 65% chance for standard pieces
        {
            if (Random.value < 0.6f) // 60% chance for original orientation
            {
                return Random.Range(0, 7); // Original pieces
            }
            else // 40% chance for special shapes
            {
                if (Random.value < 0.5f)
                {
                    return Random.Range(24, 28); // T shapes
                }
                else
                {
                    return Random.Range(28, 32); // R shapes
                }
            }
        }
        else if (random < 0.8f) // 15% chance for special pieces
        {
            return Random.Range(9, 11); // Plus and 3x3
        }
        else if (random < 0.95f) // 15% chance for vertical pieces
        {
            return Random.Range(11, 14); // Vertical I pieces (2,3,4 cells)
        }
        else // 5% chance for small pieces
        {
            if (Random.value < 0.7f) // 70% chance for original small L
            {
                return 7; // Small L
            }
            else if (Random.value < 0.85f) // 15% chance for rotated small L
            {
                return Random.Range(22, 24); // Rotated Small L
            }
            else // 15% chance for single block
            {
                return 8; // Single block
            }
        }
    }

    private int SelectCompatibleBlockType(int index)
    {
        List<int> compatibleTypes = new List<int>();
        
        // Try each possible block type
        for (int type = 0; type < 10; type++)
        {
            if (IsBlockTypeCompatible(type))
            {
                compatibleTypes.Add(type);
            }
        }
        
        // If no compatible blocks found, include single block as fallback
        if (compatibleTypes.Count == 0)
        {
            compatibleTypes.Add(8); // Single block type
        }
        
        // Return random compatible block type
        return compatibleTypes[Random.Range(0, compatibleTypes.Count)];
    }

    private bool IsBlockTypeCompatible(int blockType)
    {
        // Create temporary block to test placement
        GameObject tempObj = new GameObject("TempBlock");
        Block tempBlock = tempObj.AddComponent<Block>();
        tempBlock.InitializeBlock(blockType);
        
        bool hasValidPosition = false;
        
        // Check if this block can be placed anywhere on the grid
        for (int x = 0; x < gameBoard.columns; x++)
        {
            for (int y = 0; y < gameBoard.rows; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (tempBlock.CanPlaceAt(pos, gameBoard))
                {
                    hasValidPosition = true;
                    break;
                }
            }
            if (hasValidPosition) break;
        }
        
        Destroy(tempObj);
        return hasValidPosition;
    }

    private void UpdateValidPositions()
    {
        validPositions.Clear();
        
        // For each unplaced block
        for (int i = 0; i < availableBlocks.Length; i++)
        {
            if (availableBlocks[i] != null && !availableBlocks[i].isPlaced)
            {
                // Find all valid positions for this block
                for (int x = 0; x < gameBoard.columns; x++)
                {
                    for (int y = 0; y < gameBoard.rows; y++)
                    {
                        Vector2Int pos = new Vector2Int(x, y);
                        if (availableBlocks[i].CanPlaceAt(pos, gameBoard))
                        {
                            validPositions.Add(pos);
                        }
                    }
                }
            }
        }
    }

    public bool CanAnyBlockBePlaced()
    {
        UpdateValidPositions();
        return validPositions.Count > 0;
    }

    public void OnBlockPlaced(Block block)
    {
        // Only handle block placement if we're in the playing state
        if (game.currentState != Game.State.Playing)
        {
            return;
        }
        
        if (block != null)
        {
            int index = System.Array.IndexOf(availableBlocks, block);
            if (index != -1)
            {
                availableBlocks[index] = null;
                placedBlockCount++;

                // Wait for line clears to complete before checking game over
                StartCoroutine(CheckGameOverAfterLineClear());
            }
        }
    }

    private IEnumerator CheckGameOverAfterLineClear()
    {
        // Wait a frame to allow line clear coroutine to start
        yield return null;
        
        // Wait for any line clear animations to complete
        while (gameBoard.IsProcessingLineClear())
        {
            yield return null;
        }

        // Only spawn new blocks when all three have been placed
        if (placedBlockCount >= 3)
        {
            SpawnInitialBlocks(); // This resets placedBlockCount and spawns new blocks
        }

        if (!CanAnyBlockBePlaced())
        {
            OnGameOver(); // Fade blocks when game is over
            game.EndGame();
        }
    }

    private void PositionBlocksForMobile()
    {
        float screenHeight = mainCamera.orthographicSize * 2f;
        float gridWidth = gameBoard.columns * gameBoard.cellSize;

        float gridBottom = gameBoard.transform.position.y - (gameBoard.rows * gameBoard.cellSize / 2f);
        float spawnY = gridBottom - (gameBoard.cellSize * 4f); // Increased from 4f to 6f
        blockSpacing = gridWidth * 0.35f;
    
        float totalBlocksWidth = blockSpacing * 2f;
        float spawnX = -totalBlocksWidth / 2f;

        spawnArea.position = new Vector3(spawnX, spawnY, 0);
    }

    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        // Convert world position to grid position
        Vector3 localPos = worldPosition - gameBoard.transform.position;
        return new Vector2Int(
            Mathf.FloorToInt(localPos.x/gameBoard.cellSize + gameBoard.columns/2f),
            Mathf.FloorToInt(localPos.y/gameBoard.cellSize + gameBoard.rows/2f)
        );
    }


    public void PlayRotateSound()
    {
        if (game != null)
        {
            game.PlayRotateSound();
        }
    }

    public void CleanupBlocks()
    {
        // Clean up only available (unplaced) blocks
        if (availableBlocks != null)
        {
            foreach (var block in availableBlocks)
            {
                if (block != null && !block.isPlaced)
                {
                    Destroy(block.gameObject);
                }
            }
            availableBlocks = new Block[3];
        }

        // For menu transition, find and destroy any remaining unplaced blocks
        Block[] allBlocksInScene = FindObjectsByType<Block>(FindObjectsSortMode.None);
        foreach (var block in allBlocksInScene)
        {
            if (block != null && !block.isPlaced)
            {
                Destroy(block.gameObject);
            }
        }

        placedBlockCount = 0;
    }

    public void OnMainMenuTransition()
    {
        // For menu transition, we want to clean up ALL blocks
        if (availableBlocks != null)
        {
            foreach (var block in availableBlocks)
            {
                if (block != null)
                {
                    Destroy(block.gameObject);
                }
            }
            availableBlocks = new Block[3];
        }

        Block[] allBlocksInScene = FindObjectsByType<Block>(FindObjectsSortMode.None);
        foreach (var block in allBlocksInScene)
        {
            if (block != null)
            {
                Destroy(block.gameObject);
            }
        }

        placedBlockCount = 0;
    }

    public void OnGameOver()
    {
    // Clean up any unplaced blocks
        CleanupBlocks();
    
    // Fade out all remaining blocks
        Block[] allBlocksInScene = FindObjectsByType<Block>(FindObjectsSortMode.None);
        foreach (var block in allBlocksInScene)
        {
            if (block != null)
            {
                block.SetTransparency(0.3f); // Fade to 30% opacity
            
            // Ensure all blocks are behind UI
                foreach (var renderer in block.GetComponentsInChildren<SpriteRenderer>())
                {
                    renderer.sortingOrder = -1; // Set to be behind UI
                }
            }
        }
    }

    // Helper method to convert world position to canvas position
    private Vector2 WorldToCanvasPosition(Vector3 worldPosition)
    {
        Vector2 viewportPosition = mainCamera.WorldToViewportPoint(worldPosition);
        RectTransform canvasRect = gameCanvas.GetComponent<RectTransform>();
        
        return new Vector2(
            (viewportPosition.x * canvasRect.sizeDelta.x) - (canvasRect.sizeDelta.x * 0.5f),
            (viewportPosition.y * canvasRect.sizeDelta.y) - (canvasRect.sizeDelta.y * 0.5f)
        );
    }
}