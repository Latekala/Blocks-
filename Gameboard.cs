using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class GameBoard : MonoBehaviour
{
    [Header("Game Settings")]
    public int rows = 8;   
    public int columns = 8; 
    public float cellSize = 1.0f;
    
    [Header("Scoring")]
    public int baseLineScore = 1000;
    public float comboMultiplier = 1.5f;
    private int score = 0;
    private int currentCombo = 0;
    private float comboResetTime = 0.5f;
    private float lastClearTime;

    public UnityEvent<int> onScoreChanged;
    
    private GameObject[,] grid;
    private Game game;
    private GameObject gridVisuals;
    private VFXManager vfxManager;
    private Camera mainCamera;
    private UIManager uiManager;

    private bool isProcessingLineClear = false;

    private void Start()
    {
        mainCamera = Camera.main;
        grid = new GameObject[columns, rows];
        game = FindAnyObjectByType<Game>();
        vfxManager = FindAnyObjectByType<VFXManager>();
        uiManager = FindAnyObjectByType<UIManager>();

        // Calculate dimensions
        float screenHeight = mainCamera.orthographicSize * 2f;
        float screenWidth = screenHeight * mainCamera.aspect;
        float desiredGridWidth = screenWidth * 0.8f;
        cellSize = desiredGridWidth / columns;

        transform.position = Vector3.zero; // Center the grid at (0,0,0)
        
        LoadScore();
        CreateGridBackground();
        ShowGrid(true);
    }
    public void ShowGrid(bool show)
    {
        Debug.Log($"ShowGrid called with show={show}. GridVisuals null? " + (gridVisuals == null));
        if (gridVisuals != null)
        {
            gridVisuals.SetActive(show);
            Debug.Log("Grid visibility set to: " + show);
        }
        else
        {
            Debug.LogWarning("GridVisuals is null when trying to show/hide grid");
        }
    }

    private void CreateGridBackground()
    {
        gridVisuals = new GameObject("GridVisuals");
        gridVisuals.transform.parent = transform;
    
        float totalWidth = columns * cellSize;
        float totalHeight = rows * cellSize;
        float startX = -totalWidth / 2f;
        float startY = -totalHeight / 2f;

        // Create grid cells centered at (0,0,0)
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                GameObject cell = new GameObject($"Cell_{x}_{y}");
                SpriteRenderer spriteRenderer = cell.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = CreateGridCellSprite();
                spriteRenderer.sortingOrder = -1;
            
                cell.transform.parent = gridVisuals.transform;
                cell.transform.localPosition = new Vector3(
                    startX + (x * cellSize) + (cellSize / 2f),
                    startY + (y * cellSize) + (cellSize / 2f),
                    0
                );
                cell.transform.localScale = Vector3.one * cellSize * 0.99f;
            }
        }
    }
    private Sprite CreateGridCellSprite()
    {
        Texture2D texture = new Texture2D(64, 64);
        // Darker orange color (RGB: 204, 85, 0)
        Color darkOrange = new Color(0.8f, 0.33f, 0f, 0.4f);
        Color darkOrangeBorder = new Color(0.8f, 0.33f, 0f, 0.5f);
    
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                bool isEdge = x <= 2 || x >= 62 || y <= 2 || y >= 62;
                texture.SetPixel(x, y, isEdge ? darkOrangeBorder : darkOrange);
            }
        }
    
        texture.Apply();
        texture.filterMode = FilterMode.Point;
    
        return Sprite.Create(
            texture,
            new Rect(0, 0, 64, 64),
            new Vector2(0.5f, 0.5f),
            64f
        );
    }

    // For blocks to use
    public static Sprite CreateBlockCellSprite(Color baseColor)
    {
        Texture2D texture = new Texture2D(64, 64);
        
        // Create a block cell with depth effect
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                Color pixelColor = baseColor;
                
                // Add shading for depth effect
                bool isTopHighlight = y > 58;
                bool isRightHighlight = x > 58;
                bool isBottomShadow = y < 6;
                bool isLeftShadow = x < 6;
                
                if (isTopHighlight || isRightHighlight)
                {
                    // Lighten the edges for highlight
                    pixelColor = new Color(
                        Mathf.Clamp01(baseColor.r * 1.2f),
                        Mathf.Clamp01(baseColor.g * 1.2f),
                        Mathf.Clamp01(baseColor.b * 1.2f),
                        1f
                    );
                }
                else if (isBottomShadow || isLeftShadow)
                {
                    // Darken the edges for shadow
                    pixelColor = new Color(
                        baseColor.r * 0.8f,
                        baseColor.g * 0.8f,
                        baseColor.b * 0.8f,
                        1f
                    );
                }
                
                texture.SetPixel(x, y, pixelColor);
            }
        }
        
        texture.Apply();
        texture.filterMode = FilterMode.Point;
    
        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f
        );
    }
    public void ResetBoard()
    {
        // Clear all blocks from the grid
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (grid[x, y] != null)
                {
                    Destroy(grid[x, y]);
                    grid[x, y] = null;
                }
            }
        }
        
        // Reset score
        score = 0;
        currentCombo = 0;
        onScoreChanged.Invoke(score);
        
        // Reset grid array
        grid = new GameObject[columns, rows];
        
        // Ensure grid is visible
        ShowGrid(true);
    }

    public bool IsOccupied(Vector2Int position)
    {
        if (position.x < 0 || position.x >= columns || position.y < 0 || position.y >= rows)
            return true;
        return grid[position.x, position.y] != null;
    }

    public void PlaceBlock(Block block, Vector2Int position)
    {
        // Check if we can place the block
        foreach (var pos in block.blockShape)
        {
            Vector2Int worldPos = position + pos;
            if (worldPos.x < 0 || worldPos.x >= columns ||
                worldPos.y < 0 || worldPos.y >= rows ||
                IsOccupied(worldPos))
            {
                return;
            }
        }

        // Place the block
        foreach (var pos in block.blockShape)
        {
            Vector2Int worldPos = position + pos;
            grid[worldPos.x, worldPos.y] = block.gameObject;
        }
        block.isPlaced = true;
        
        // Convert grid position to world position, centered at (0,0,0)
        Vector3 finalPos = GridToWorldPosition(position);
        block.transform.position = finalPos;
        
        // Play sound and visual effect
        if (game != null)
        {
            game.PlayBlockPlaceSound();
    }

        if (vfxManager != null)
    {
            Vector3 effectPos = finalPos;  // Use the same position we just calculated
            vfxManager.PlayBlockPlaceEffect(effectPos);
        }

        // Add score for placing block
        int blockScore = block.blockShape.Length * 10; // Score based on block size
        AddScore(blockScore);

        // Show score popup at block position
        if (uiManager != null)
        {
            uiManager.ShowScorePopup(blockScore, block.transform.position);
        }

        // Check for line clears
        StartCoroutine(CheckLinesWithAnimation());
    }

    private void UpdateScore(int linesCleared)
    {
        if (Time.time - lastClearTime < comboResetTime)
        {
            currentCombo++;
        }
        else
        {
            currentCombo = 0;
        }

        float multiplier = 1 + (currentCombo * (comboMultiplier - 1));
        int scoreIncrease = Mathf.RoundToInt(baseLineScore * linesCleared * multiplier);
        score += scoreIncrease;
        
        onScoreChanged.Invoke(score);
        
        if (score > PlayerPrefs.GetInt("HighScore", 0))
        {
            PlayerPrefs.SetInt("HighScore", score);
            PlayerPrefs.Save();
        }

        lastClearTime = Time.time;
    }

    private IEnumerator CheckLinesWithAnimation()
    {
        isProcessingLineClear = true;  // Set flag at start
        
        List<int> fullRows = new List<int>();
        List<int> fullColumns = new List<int>();
        
        // Find full lines
        for (int y = 0; y < rows; y++)
        {
            if (IsRowFull(y)) fullRows.Add(y);
        }
        for (int x = 0; x < columns; x++)
        {
            if (IsColumnFull(x)) fullColumns.Add(x);
        }

        if (fullRows.Count > 0 || fullColumns.Count > 0)
        {
            yield return StartCoroutine(FlashLinesAnimation(fullRows, fullColumns));
            
            // Play line clear effects
            if (vfxManager != null)
            {
                foreach (int row in fullRows)
                {
                    float yPos = transform.position.y + ((row - rows/2f) * cellSize) + cellSize/2;
                    Vector3 effectPos = new Vector3(transform.position.x, yPos, 0);
                    vfxManager.PlayLineClearEffect(effectPos, true, columns * cellSize);
                }
                
                foreach (int column in fullColumns)
                {
                    float xPos = transform.position.x + ((column - columns/2f) * cellSize) + cellSize/2;
                    Vector3 effectPos = new Vector3(xPos, transform.position.y, 0);
                    vfxManager.PlayLineClearEffect(effectPos, false, rows * cellSize);
                }
            }
            
            // Clear the lines
            foreach (int row in fullRows) ClearRow(row);
            foreach (int column in fullColumns) ClearColumn(column);
            
            if (game != null) game.PlayLineClearSound();
            
            // Add score for cleared lines
            int clearedLines = fullRows.Count + fullColumns.Count;
            int clearScore = CalculateLineClearScore(clearedLines);
            AddScore(clearScore);
            
            // Show score popup
            if (uiManager != null && (fullRows.Count > 0 || fullColumns.Count > 0))
            {
                Vector3 popupPos = transform.position;
                if (fullRows.Count > 0)
                {
                    popupPos.y += ((fullRows[0] - rows/2f) * cellSize);
                }
                else if (fullColumns.Count > 0)
                {
                    popupPos.x += ((fullColumns[0] - columns/2f) * cellSize);
                }
                uiManager.ShowScorePopup(clearScore, popupPos);
            }
        }

        isProcessingLineClear = false;  // Clear flag when done
        yield return null;
    }

    private IEnumerator FlashLinesAnimation(List<int> rowsToFlash, List<int> columnsToFlash)
    {
        float elapsed = 0;
        float duration = 0.3f;

        // Store original colors
        Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();

        // Get all renderers that will be flashed
        foreach (int row in rowsToFlash)
        {
            for (int x = 0; x < columns; x++)
            {
                if (grid[x, row] != null)
                {
                    foreach (var renderer in grid[x, row].GetComponentsInChildren<Renderer>())
                    {
                        if (!originalColors.ContainsKey(renderer))
                        {
                            originalColors.Add(renderer, renderer.material.color);
                        }
                    }
                }
            }
        }
        
        foreach (int column in columnsToFlash)
        {
            for (int y = 0; y < rows; y++)
            {
                if (grid[column, y] != null)
                {
                    foreach (var renderer in grid[column, y].GetComponentsInChildren<Renderer>())
                    {
                        if (!originalColors.ContainsKey(renderer))
        {
                            originalColors.Add(renderer, renderer.material.color);
                        }
                    }
                }
            }
        }

        while (elapsed < duration)
            {
            float flash = Mathf.PingPong(elapsed * 8, 1);
            Color flashColor = Color.white * flash;

            // Flash rows
            foreach (int row in rowsToFlash)
            {
                for (int x = 0; x < columns; x++)
                {
                    if (grid[x, row] != null)
                    {
                        foreach (var renderer in grid[x, row].GetComponentsInChildren<Renderer>())
                        {
                            renderer.material.color = flashColor;
                        }
                    }
                }
            }

            // Flash columns
            foreach (int column in columnsToFlash)
            {
                for (int y = 0; y < rows; y++)
                {
                    if (grid[column, y] != null)
                    {
                        foreach (var renderer in grid[column, y].GetComponentsInChildren<Renderer>())
                        {
                            renderer.material.color = flashColor;
                        }
                    }
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
            }

        // Reset all colors back to original
        foreach (var kvp in originalColors)
        {
            if (kvp.Key != null)
            {
                kvp.Key.material.color = kvp.Value;
            }
        }
    }

    private void ClearRow(int row)
                {
        for (int x = 0; x < columns; x++)
        {
            if (grid[x, row] != null)
            {
                // Find the Block component
                Block block = grid[x, row].GetComponent<Block>();
                if (block != null)
                {
                    // Only destroy this specific cell of the block
                    block.RemoveCell(new Vector2Int(x, row));
                }
                
                // Clear the grid reference
                grid[x, row] = null;
                }
        }
    }

    private void ClearColumn(int column)
                {
        for (int y = 0; y < rows; y++)
        {
            if (grid[column, y] != null)
            {
                // Find the Block component
                Block block = grid[column, y].GetComponent<Block>();
                if (block != null)
                {
                    // Only destroy this specific cell of the block
                    block.RemoveCell(new Vector2Int(column, y));
                }
                
                // Clear the grid reference
                grid[column, y] = null;
            }
        }
    }

    private IEnumerator ApplyGravityWithAnimation()
    {
        float gravityDuration = 0.3f;
        
        // Process columns from bottom to top
        for (int x = 0; x < columns; x++)
        {
            int writeIndex = 0;  // Where we'll write the next block
            
            // First, collect all blocks in this column
            List<GameObject> blocksInColumn = new List<GameObject>();
            for (int y = 0; y < rows; y++)
            {
                if (grid[x, y] != null)
                {
                    blocksInColumn.Add(grid[x, y]);
                    grid[x, y] = null;  // Clear the grid position
                }
            }
            
            // Then place them back starting from the bottom
            foreach (GameObject block in blocksInColumn)
            {
                Vector3 startPos = block.transform.position;
                Vector3 endPos = transform.position + new Vector3(
                    (x - columns/2f) * cellSize + cellSize/2,
                    writeIndex * cellSize + cellSize/2,
                    0
                );
                
                grid[x, writeIndex] = block;  // Update grid position
                
                float elapsed = 0;
                while (elapsed < gravityDuration)
                {
                    float t = elapsed / gravityDuration;
                    t = 1 - (1 - t) * (1 - t); // Ease out quad
                    block.transform.position = Vector3.Lerp(startPos, endPos, t);
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                
                block.transform.position = endPos;
                writeIndex++;
            }
        }
    }

    private bool IsRowFull(int row)
    {
        for (int x = 0; x < columns; x++)
        {
            if (!IsOccupied(new Vector2Int(x, row))) return false;
        }
        return true;
    }

    private bool IsColumnFull(int column)
    {
        for (int y = 0; y < rows; y++)
        {
            if (!IsOccupied(new Vector2Int(column, y))) return false;
        }
        return true;
    }

    private void LoadScore()
    {
        score = PlayerPrefs.GetInt("LastScore", 0);
        onScoreChanged.Invoke(score);
    }

    public int GetScore()
    {
        return score;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        float totalWidth = columns * cellSize;
        float totalHeight = rows * cellSize;
        Vector3 origin = transform.position;

        // Draw vertical lines
        for (int x = 0; x <= columns; x++)
        {
            float xPos = origin.x + (x - columns/2f) * cellSize;
            Gizmos.DrawLine(
                new Vector3(xPos, origin.y, 0),
                new Vector3(xPos, origin.y + totalHeight, 0)
            );
        }

        // Draw horizontal lines
        for (int y = 0; y <= rows; y++)
        {
            float yPos = origin.y + y * cellSize;
            Gizmos.DrawLine(
                new Vector3(origin.x - (columns/2f) * cellSize, yPos, 0),
                new Vector3(origin.x + (columns/2f) * cellSize, yPos, 0)
            );
        }
    }

    public bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < columns && pos.y >= 0 && pos.y < rows && !IsOccupied(pos);
    }

    // Add these helper methods to convert between grid and world positions
    private Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return transform.position + new Vector3(
            (gridPos.x - columns/2f) * cellSize + cellSize/2,
            (gridPos.y - rows/2f) * cellSize + cellSize/2,
            0
        );
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        Vector3 localPos = worldPos - transform.position;
        return new Vector2Int(
            Mathf.FloorToInt(localPos.x/cellSize + columns/2f),
            Mathf.FloorToInt(localPos.y/cellSize + rows/2f)
        );
    }

    private int CalculateLineClearScore(int lineCount)
    {
        // Exponential scoring for multiple lines
        return lineCount * lineCount * 100;
    }

    private void AddScore(int points)
    {
        score += points;
        
        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateScore(score);
            
            // Check for new high score
            int currentHighScore = PlayerPrefs.GetInt("HighScore", 0);
            if (score > currentHighScore)
            {
                PlayerPrefs.SetInt("HighScore", score);
                uiManager.UpdateHighScore(score);
            }
        }
    }

    public void FadeBlocks(float targetAlpha)
    {
        // Fade all placed blocks
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (grid[x, y] != null)
                {
                    Block block = grid[x, y].GetComponent<Block>();
                    if (block != null)
                    {
                        block.SetTransparency(targetAlpha);
                    }
                }
            }
        }
    }

    public void ShowLineClearPreview(Block block, Vector2Int position)
    {
        // Reset any previous preview
        ResetLineClearPreview();
        
        // Check which lines would be filled if the block was placed here
        List<int> previewRows = new List<int>();
        List<int> previewColumns = new List<int>();
        
        // Create temporary grid state
        bool[,] tempGrid = new bool[columns, rows];
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                tempGrid[x, y] = grid[x, y] != null;
            }
        }
        
        // Add block to temporary grid
        foreach (var pos in block.blockShape)
        {
            Vector2Int worldPos = position + pos;
            if (worldPos.x >= 0 && worldPos.x < columns && 
                worldPos.y >= 0 && worldPos.y < rows)
            {
                tempGrid[worldPos.x, worldPos.y] = true;
            }
        }
        
        // Check for full rows and columns
        for (int y = 0; y < rows; y++)
        {
            bool rowFull = true;
            for (int x = 0; x < columns; x++)
            {
                if (!tempGrid[x, y])
                {
                    rowFull = false;
                    break;
                }
            }
            if (rowFull) previewRows.Add(y);
        }
        
        for (int x = 0; x < columns; x++)
        {
            bool columnFull = true;
            for (int y = 0; y < rows; y++)
            {
                if (!tempGrid[x, y])
                {
                    columnFull = false;
                    break;
                }
            }
            if (columnFull) previewColumns.Add(x);
        }
        
        // Show preview effect
        if (previewRows.Count > 0 || previewColumns.Count > 0)
        {
            ShowPreviewEffect(previewRows, previewColumns);
        }
    }

    private void ShowPreviewEffect(List<int> rows, List<int> columns)
    {
        // Make the preview more visible with a brighter color and higher alpha
        Color previewColor = new Color(1f, 1f, 1f, 0.5f); // Increased alpha from 0.3f to 0.5f
        
        foreach (int row in rows)
        {
            for (int x = 0; x < this.columns; x++)
            {
                Vector3 pos = GridToWorldPosition(new Vector2Int(x, row));
                // Create preview effect (e.g., transparent white overlay)
                GameObject preview = new GameObject($"Preview_Row_{row}_{x}");
                preview.transform.parent = transform;
                preview.transform.position = pos;
                
                SpriteRenderer sr = preview.AddComponent<SpriteRenderer>();
                sr.sprite = CreateBlockCellSprite(Color.white);
                sr.color = previewColor;
                sr.sortingOrder = 1;
                preview.transform.localScale = Vector3.one * cellSize * 0.95f;
            }
        }
        
        foreach (int col in columns)
        {
            for (int y = 0; y < this.rows; y++)
            {
                Vector3 pos = GridToWorldPosition(new Vector2Int(col, y));
                GameObject preview = new GameObject($"Preview_Col_{col}_{y}");
                preview.transform.parent = transform;
                preview.transform.position = pos;
                
                SpriteRenderer sr = preview.AddComponent<SpriteRenderer>();
                sr.sprite = CreateBlockCellSprite(Color.white);
                sr.color = previewColor;
                sr.sortingOrder = 1;
                preview.transform.localScale = Vector3.one * cellSize * 0.95f;
            }
        }
    }

    public void ResetLineClearPreview()
    {
        // Remove all preview objects
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Preview_"))
            {
                Destroy(child.gameObject);
            }
        }
    }

    public bool IsProcessingLineClear()
    {
        return isProcessingLineClear;
    }
}