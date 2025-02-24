using UnityEngine;

public class DebugHelper : MonoBehaviour
{
    private float updateInterval = 1.0f;
    private float nextUpdate = 0.0f;
    
    void Update()
    {
        if (Time.time > nextUpdate)
        {
            nextUpdate = Time.time + updateInterval;
            PrintDebugInfo();
        }
    }
    
    void PrintDebugInfo()
    {
        GameBoard board = FindAnyObjectByType<GameBoard>();
        BlockManager blockManager = FindAnyObjectByType<BlockManager>();
        
        if (board == null || blockManager == null) return;

        // Grid configuration check
        if (board.columns != 8 || board.rows != 8)
        {
            Debug.Log($"Grid info: {board.columns}x{board.rows}, Cell: {board.cellSize}");
        }
        
        // Spawn position check
        Vector3 spawnPos = blockManager.spawnArea.position;
        Vector3 gridPos = board.transform.position;
        float gridBottom = gridPos.y - (board.rows * board.cellSize / 2f);
        float expectedY = gridBottom - (board.cellSize * 4f); // Match BlockManager's calculation
        
        // Only log if misalignment is significant (more than half a cell)
        if (Mathf.Abs(spawnPos.y - expectedY) > board.cellSize / 2f)
        {
            Debug.Log($"Spawn position: {spawnPos.y:F2}, Expected: {expectedY:F2}, Difference: {(spawnPos.y - expectedY):F2}");
        }
    }
}