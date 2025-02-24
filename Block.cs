using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Block : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public Vector2Int[] blockShape;
    public Color blockColor;
    public bool isPlaced = false;
    public int blockType { get; private set; }

    private static readonly Vector2Int[][] shapes = new Vector2Int[][]
    {
        // 0: Square (O)
        new Vector2Int[] { 
            new Vector2Int(0, 0), new Vector2Int(1, 0), 
            new Vector2Int(0, 1), new Vector2Int(1, 1) 
        },
        
        // 1: Line (I) - horizontal
        new Vector2Int[] { 
            new Vector2Int(-1, 0), new Vector2Int(0, 0), 
            new Vector2Int(1, 0), new Vector2Int(2, 0) 
        },
        
        // 2: T shape
        new Vector2Int[] { 
            new Vector2Int(0, 0), new Vector2Int(-1, 0), 
            new Vector2Int(1, 0), new Vector2Int(0, 1) 
        },
        
        // 3: L shape
        new Vector2Int[] { 
            new Vector2Int(-1, 0), new Vector2Int(0, 0), 
            new Vector2Int(1, 0), new Vector2Int(1, 1) 
        },
        
        // 4: Reverse L (J)
        new Vector2Int[] { 
            new Vector2Int(-1, 0), new Vector2Int(0, 0), 
            new Vector2Int(1, 0), new Vector2Int(-1, 1) 
        },
        
        // 5: Z shape
        new Vector2Int[] { 
            new Vector2Int(-1, 0), new Vector2Int(0, 0), 
            new Vector2Int(0, 1), new Vector2Int(1, 1) 
        },
        
        // 6: S shape
        new Vector2Int[] { 
            new Vector2Int(0, 0), new Vector2Int(1, 0), 
            new Vector2Int(-1, 1), new Vector2Int(0, 1) 
        },
        
        // 7: Small L (2x2)
        new Vector2Int[] { 
            new Vector2Int(0, 0), new Vector2Int(1, 0), 
            new Vector2Int(0, 1) 
        },
        
        // 8: Single block (1x1)
        new Vector2Int[] { 
            new Vector2Int(0, 0) 
        },
        
        // 9: Plus shape
        new Vector2Int[] { 
            new Vector2Int(0, 0), new Vector2Int(-1, 0), 
            new Vector2Int(1, 0), new Vector2Int(0, 1), 
            new Vector2Int(0, -1) 
        },
        
        // 10: 3x3 Square
        new Vector2Int[] {
            new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
            new Vector2Int(-1, 0), new Vector2Int(0, 0), new Vector2Int(1, 0),
            new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(1, 1)
        },
        
        // 11: Vertical I (2 cells)
        new Vector2Int[] {
            new Vector2Int(0, 0),
            new Vector2Int(0, 1)
        },
        
        // 12: Vertical I (3 cells)
        new Vector2Int[] {
            new Vector2Int(0, -1),
            new Vector2Int(0, 0),
            new Vector2Int(0, 1)
        },
        
        // 13: Vertical I (4 cells)
        new Vector2Int[] {
            new Vector2Int(0, -1),
            new Vector2Int(0, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, 2)
        },
        
        // 14: Z shape (180 degree rotated)
        new Vector2Int[] {
            new Vector2Int(0, -1),
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(1, 1)
        },
        
        // 15: S shape (180 degree rotated)
        new Vector2Int[] {
            new Vector2Int(1, -1),
            new Vector2Int(1, 0),
            new Vector2Int(0, 0),
            new Vector2Int(0, 1)
        },
        
        // 16: T shape 90°
        new Vector2Int[] {
            new Vector2Int(0, -1), new Vector2Int(0, 0),
            new Vector2Int(0, 1), new Vector2Int(1, 0)
        },
        
        // 17: T shape 180°
        new Vector2Int[] {
            new Vector2Int(-1, 0), new Vector2Int(0, 0),
            new Vector2Int(1, 0), new Vector2Int(0, -1)
        },
        
        // 18: L shape 90°
        new Vector2Int[] {
            new Vector2Int(0, -1), new Vector2Int(0, 0),
            new Vector2Int(0, 1), new Vector2Int(1, -1)
        },
        
        // 19: L shape 180°
        new Vector2Int[] {
            new Vector2Int(-1, -1), new Vector2Int(-1, 0),
            new Vector2Int(0, 0), new Vector2Int(1, 0)
        },
        
        // 20: J shape 90°
        new Vector2Int[] {
            new Vector2Int(0, -1), new Vector2Int(0, 0),
            new Vector2Int(0, 1), new Vector2Int(-1, -1)
        },
        
        // 21: J shape 180°
        new Vector2Int[] {
            new Vector2Int(-1, 0), new Vector2Int(0, 0),
            new Vector2Int(1, 0), new Vector2Int(1, -1)
        },
        
        // 22: Small L 90°
        new Vector2Int[] {
            new Vector2Int(0, 0), new Vector2Int(0, 1),
            new Vector2Int(1, 1)
        },
        
        // 23: Small L 180°
        new Vector2Int[] {
            new Vector2Int(0, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, -1)
        },
        
        // 28: R shape (standard)
        new Vector2Int[] { 
            new Vector2Int(0, 0),   // center
            new Vector2Int(0, 1),   // top
            new Vector2Int(0, -1),  // bottom
            new Vector2Int(1, 1),   // top right
            new Vector2Int(1, 0)    // middle right
        },
        
        // 29: R shape (mirrored)
        new Vector2Int[] { 
            new Vector2Int(0, 0),   // center
            new Vector2Int(0, 1),   // top
            new Vector2Int(0, -1),  // bottom
            new Vector2Int(-1, 1),  // top left
            new Vector2Int(-1, 0)   // middle left
        },
        
        // 30: Small R shape
        new Vector2Int[] { 
            new Vector2Int(0, 0),   // center
            new Vector2Int(0, 1),   // top
            new Vector2Int(1, 1),   // top right
            new Vector2Int(0, -1)   // bottom
        },
        
        // 31: Wide R shape
        new Vector2Int[] { 
            new Vector2Int(0, 0),    // center
            new Vector2Int(0, 1),    // top
            new Vector2Int(0, -1),   // bottom
            new Vector2Int(1, 1),    // top right
            new Vector2Int(2, 0),    // far right
            new Vector2Int(1, 0)     // middle right
        }
    };

    private static readonly Color[] colors = new Color[]
    {
        new Color(1f, 1f, 0f),       // Bright Yellow (O)
        new Color(0f, 1f, 1f),       // Bright Cyan (I)
        new Color(1f, 0f, 1f),       // Bright Magenta (T)
        new Color(1f, 0.5f, 0f),     // Bright Orange (L)
        new Color(0f, 0f, 1f),       // Bright Blue (J)
        new Color(1f, 0f, 0f),       // Bright Red (Z)
        new Color(0f, 1f, 0f),       // Bright Green (S)
        new Color(0.8f, 0.4f, 0f),   // Bright Brown (Small L)
        new Color(1f, 1f, 1f),       // Pure White (Single)
        new Color(1f, 0.7f, 0.9f),   // Bright Pink (Plus)
        new Color(0f, 1f, 0.7f),     // Bright Mint (3x3)
        new Color(1f, 0.5f, 1f),     // Bright Hot Pink (I2)
        new Color(0.7f, 0f, 1f),     // Bright Purple (I3)
        new Color(0f, 1f, 1f),       // Bright Cyan (I4)
        new Color(1f, 0.5f, 0f),     // Bright Orange (Z180)
        new Color(0.5f, 1f, 0f),     // Bright Lime (S180)
        new Color(1f, 0f, 1f),       // Bright Magenta (T90)
        new Color(1f, 0f, 1f),       // Bright Magenta (T180)
        new Color(1f, 0.5f, 0f),     // Bright Orange (L90)
        new Color(1f, 0.5f, 0f),     // Bright Orange (L180)
        new Color(0f, 0f, 1f),       // Bright Blue (J90)
        new Color(0f, 0f, 1f),       // Bright Blue (J180)
        new Color(0.8f, 0.4f, 0f),   // Bright Brown (Small L90)
        new Color(0.8f, 0.4f, 0f),   // Bright Brown (Small L180)
        // 28: R shape (standard)
        new Color(0.9f, 0.2f, 0.2f),    // Bright Red
        // 29: R shape (mirrored)
        new Color(1f, 0.3f, 0.3f),      // Light Red
        // 30: Small R shape
        new Color(0.8f, 0.2f, 0.4f),    // Red-Pink
        // 31: Wide R shape
        new Color(1f, 0.15f, 0.15f)     // Deep Red
    };

    private GameObject[] cells;
    private Vector3 offset;
    private Vector3 originalPosition;
    private BlockManager blockManager;
    private GameBoard gameBoard;
    private Camera mainCamera;
    private Block ghostBlock;
    private bool isDragging = false;

    private BoxCollider2D blockCollider;
    private PlayerInput playerInput;
    private InputAction pointerAction;
    private InputAction positionAction;
    private Vector2 pointerStartPosition;

    private void Start()
    {
        blockManager = FindAnyObjectByType<BlockManager>();
        gameBoard = FindAnyObjectByType<GameBoard>();
        mainCamera = Camera.main;
        blockCollider = GetComponent<BoxCollider2D>();
        if (blockCollider == null)
        {
            blockCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        blockCollider.isTrigger = true;
    }

    public void InitializeBlock(int shapeIndex)
    {
        blockType = shapeIndex;
        blockShape = shapes[shapeIndex];
        blockColor = colors[shapeIndex];
        CreateBlock();
        UpdateColliderSize();
    }

    private void UpdateColliderSize()
    {
        if (blockCollider != null && blockShape != null)
        {
            Vector2 min = Vector2.positiveInfinity;
            Vector2 max = Vector2.negativeInfinity;
        
            foreach (var pos in blockShape)
            {
                min = Vector2.Min(min, pos);
                max = Vector2.Max(max, pos);
            }
        
            // Increase the collider size by 20% to make it more forgiving to touch
            Vector2 size = (max - min + Vector2.one) * gameBoard.cellSize * 1.2f;
            Vector2 offset = (max + min) / 2f * gameBoard.cellSize;
        
            blockCollider.size = size;
            blockCollider.offset = offset;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isPlaced)
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            if (blockCollider.OverlapPoint(new Vector2(mousePos.x, mousePos.y)))
            {
                HandlePointerDown(mousePos);
            }
        }

        if (isDragging && !isPlaced)
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePos.x + offset.x, mousePos.y + offset.y, transform.position.z);
            UpdateGhostBlock(transform.position);
            
            // Add line clear preview
            Vector2Int gridPos = blockManager.GetGridPosition(transform.position);
            gameBoard.ShowLineClearPreview(this, gridPos);
        }

        if (Input.GetMouseButtonUp(0) && isDragging && !isPlaced)
        {
            isDragging = false;
            Vector2Int gridPos = blockManager.GetGridPosition(transform.position);

            if (CanPlaceAt(gridPos, gameBoard))
            {
                PlaceBlockAtGridPosition(gridPos);
            }
            else
            {
                ReturnToOriginalPosition();
            }

            DestroyGhostBlock();
            gameBoard.ResetLineClearPreview();
        }
    }

    private void HandlePointerDown(Vector3 worldPos)
    {
        if (isPlaced) return;
    
        isDragging = true;
        originalPosition = transform.position;

        // Calculate offset from block center to touch point
        offset = transform.position - worldPos;
    
        // Keep vertical offset to ensure block stays above finger
        offset.y = gameBoard.cellSize * 3f; // Reduced from 5f for better control
        offset.z = 0;
    
        // Set initial position with offset
        transform.position = new Vector3(
            worldPos.x + offset.x, 
            worldPos.y + offset.y, 
            -2
        );

        CreateGhostBlock();
        transform.localScale = Vector3.one * blockManager.draggedScale;
    }

    private void CreateBlock()
    {
        GameBoard gameBoard = FindAnyObjectByType<GameBoard>();
        float cellSize = gameBoard != null ? gameBoard.cellSize : 1f;

        cells = new GameObject[blockShape.Length];
        for (int i = 0; i < blockShape.Length; i++)
        {
            GameObject cell = new GameObject($"Cell_{i}");
            SpriteRenderer spriteRenderer = cell.AddComponent<SpriteRenderer>();
            
            Texture2D texture = new Texture2D(64, 64);
            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    bool isEdge = x <= 2 || x >= 62 || y <= 2 || y >= 62;
                    Color pixelColor = blockColor;
                    if (isEdge)
                    {
                        pixelColor = new Color(
                            blockColor.r * 0.8f,
                            blockColor.g * 0.8f,
                            blockColor.b * 0.8f,
                            1f
                        );
                    }
                    texture.SetPixel(x, y, pixelColor);
                }
            }
            texture.Apply();
            texture.filterMode = FilterMode.Point;
        
            spriteRenderer.sprite = Sprite.Create(
                texture,
                new Rect(0, 0, 64, 64),
                new Vector2(0.5f, 0.5f),
                64f
            );
        
            spriteRenderer.sortingOrder = 2;
        
            cell.transform.SetParent(transform, false);
            cell.transform.localPosition = new Vector3(
                blockShape[i].x * cellSize, 
                blockShape[i].y * cellSize, 
                -1  // Set z-position for proper layering
            );
            cell.transform.localScale = Vector3.one * cellSize * 0.95f;
        
            cells[i] = cell;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isPlaced || isDragging) return;

        isDragging = true;
        originalPosition = transform.position;

    // Convert screen position to world position
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, 10));
    
    // Calculate offset from block center to touch point
        offset = transform.position - worldPos;
    
    // Keep vertical offset to ensure block stays above finger
        offset.y = gameBoard.cellSize * 3f; // Reduced from 6f to 3f
        offset.z = 0;

        CreateGhostBlock();
        transform.localScale = Vector3.one * blockManager.draggedScale;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || isPlaced) return;

        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, 10));
        transform.position = mousePosition + offset;
        
        if (ghostBlock != null)
        {
        UpdateGhostBlock(transform.position);
        }

        // Add line clear preview
        Vector2Int gridPos = blockManager.GetGridPosition(transform.position);
        gameBoard.ShowLineClearPreview(this, gridPos);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging || isPlaced) return;
        isDragging = false;

        Vector2Int gridPos = blockManager.GetGridPosition(transform.position);
        if (CanPlaceAt(gridPos, gameBoard))
        {
            PlaceBlockAtGridPosition(gridPos);
        }
        else
        {
            ReturnToOriginalPosition();
        }

        DestroyGhostBlock();
        gameBoard.ResetLineClearPreview();
    }

    private void PlaceBlockAtGridPosition(Vector2Int gridPos)
    {
        Vector3 finalPos = gameBoard.transform.position + new Vector3(
            (gridPos.x - gameBoard.columns/2f) * gameBoard.cellSize + gameBoard.cellSize/2f,
            (gridPos.y * gameBoard.cellSize) - (gameBoard.cellSize * 8f),
            transform.position.z
        );
        transform.position = finalPos;
        gameBoard.PlaceBlock(this, gridPos);
        blockManager.OnBlockPlaced(this);
    }

    private void ReturnToOriginalPosition()
    {
        transform.position = originalPosition;
        transform.localScale = Vector3.one * blockManager.spawnAreaScale;
    }

    private void CreateGhostBlock()
    {
        if (ghostBlock != null)
        {
            DestroyGhostBlock();
        }
    
        GameObject ghostObj = Instantiate(gameObject, transform.position, Quaternion.identity);
        ghostBlock = ghostObj.GetComponent<Block>();
    
        // Set ghost properties
        foreach (var cell in ghostBlock.GetComponentsInChildren<SpriteRenderer>())
        {
            Color ghostColor = blockManager.validPlacementColor;
            cell.color = ghostColor;
        }
    
        ghostBlock.transform.localScale = transform.localScale;
    
        // Disable components
        ghostBlock.enabled = false;
        var collider = ghostBlock.GetComponent<BoxCollider2D>();
        if (collider != null) Destroy(collider);
    }

    private void UpdateGhostBlock(Vector3 position)
    {
        if (ghostBlock == null) return;
        ghostBlock.transform.position = position;
    
        Vector2Int gridPos = blockManager.GetGridPosition(position);
        Color ghostColor = CanPlaceAt(gridPos, gameBoard) ? 
            blockManager.validPlacementColor : 
            blockManager.invalidPlacementColor;
    
        foreach (var cell in ghostBlock.GetComponentsInChildren<SpriteRenderer>())
        {
            cell.color = ghostColor;
        }
    }

    private void DestroyGhostBlock()
    {
        if (ghostBlock != null)
        {
            Destroy(ghostBlock.gameObject);
            ghostBlock = null;
        }
    }

    public bool CanPlaceAt(Vector2Int position, GameBoard gameBoard)
    {
        // Check grid boundaries first
        if (position.x < 0 || position.x >= gameBoard.columns ||
            position.y < 0 || position.y >= gameBoard.rows)
            return false;

        foreach (var pos in blockShape)
        {
            Vector2Int worldPos = position + pos;
            if (worldPos.x < 0 || worldPos.x >= gameBoard.columns ||
                worldPos.y < 0 || worldPos.y >= gameBoard.rows ||
                gameBoard.IsOccupied(worldPos))
            {
                return false;
            }
        }
        return true;
    }

    public void SetColor(Color color)
    {
        foreach (var cell in cells)
        {
            if (cell != null)
            {
                SpriteRenderer renderer = cell.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.color = color;
                }
            }
        }
    }

    public void SetTransparency(float alpha)
    {
        // Only set transparency for game over screen
        if (isPlaced)
        {
            foreach (Transform cell in transform)
            {
                if (cell != null)
                {
                    SpriteRenderer renderer = cell.GetComponent<SpriteRenderer>();
                    if (renderer != null)
                    {
                        Color color = renderer.color;
                        color.a = alpha;
                        renderer.color = color;
                    }
                }
            }
        }
    }

    public void RemoveCell(Vector2Int gridPosition)
    {
        // Find the cell at this grid position
        foreach (Transform cell in transform)
        {
            Vector2Int cellGridPos = gameBoard.WorldToGridPosition(cell.position);
            if (cellGridPos == gridPosition)
            {
                // Destroy only this cell
                Destroy(cell.gameObject);
                
                // Reset transparency for remaining cells
                foreach (Transform remainingCell in transform)
                {
                    if (remainingCell != null && remainingCell != cell)
                    {
                        SpriteRenderer renderer = remainingCell.GetComponent<SpriteRenderer>();
                        if (renderer != null)
                        {
                            Color color = renderer.color;
                            color.a = 1f;  // Reset to full opacity
                            renderer.color = color;
                        }
                    }
                }
                break;
            }
        }
    }
}