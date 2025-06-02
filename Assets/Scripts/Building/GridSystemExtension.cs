using UnityEngine;

public static class GridSystemExtension
{
    public static Vector3 GridToWorldPosition(Vector2Int gridPosition, Vector3 gridOrigin, float cellSize)
    {
        return gridOrigin + new Vector3(gridPosition.x * cellSize, 0, gridPosition.y * cellSize);
    }

    public static Vector3 GetGridCenterPosition(Vector2Int gridPosition, Vector3 gridOrigin, float cellSize)
    {
        return gridOrigin + new Vector3(gridPosition.x * cellSize + cellSize / 2, 0,
            gridPosition.y * cellSize + cellSize / 2);
    }


    public static Direction CalculateWallDirection(Vector3 hitPoint, Vector3 floorCenter)
    {
        Vector2 directionVector = new Vector2(hitPoint.x - floorCenter.x, hitPoint.z - floorCenter.z).normalized;
        float angle = Mathf.Atan2(directionVector.y, directionVector.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        if (angle >= 45f && angle < 135f) return Direction.Top;
        if (angle >= 135f && angle < 225f) return Direction.Left;
        if (angle >= 225f && angle < 315f) return Direction.Bot;
        return Direction.Right;
    }

    public static bool IsValidGridPosition(Vector2Int gridPosition, int gridWidth, int gridHeight)
    {
        return gridPosition.x >= 0 && gridPosition.x < gridWidth &&
               gridPosition.y >= 0 && gridPosition.y < gridHeight;
    }
    
}