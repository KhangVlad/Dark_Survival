using UnityEngine;

public static class GridSystemExtension
{
    /// <summary>
    /// Converts a world position to a grid position
    /// </summary>
    public static Vector2Int WorldToGridPosition(Vector3 worldPosition, Vector3 gridOrigin, float cellSize)
    {
        Vector3 localPosition = worldPosition - gridOrigin;
        int x = Mathf.FloorToInt(localPosition.x / cellSize);
        int z = Mathf.FloorToInt(localPosition.z / cellSize);
        return new Vector2Int(x, z);
    }

    /// <summary>
    /// Converts a grid position to a world position
    /// </summary>
    public static Vector3 GridToWorldPosition(Vector2Int gridPosition, Vector3 gridOrigin, float cellSize)
    {
        return gridOrigin + new Vector3(gridPosition.x * cellSize, 0, gridPosition.y * cellSize);
    }

    /// <summary>
    /// Gets the center position of a grid cell
    /// </summary>
    public static Vector3 GetGridCenterPosition(Vector2Int gridPosition, Vector3 gridOrigin, float cellSize)
    {
        return gridOrigin + new Vector3(gridPosition.x * cellSize + cellSize / 2, 0,
            gridPosition.y * cellSize + cellSize / 2);
    }

    /// <summary>
    /// Calculates the direction of a wall based on a hit point and floor center
    /// </summary>
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

    /// <summary>
    /// Checks if a grid position is within bounds
    /// </summary>
    public static bool IsValidGridPosition(Vector2Int gridPosition, int gridWidth, int gridHeight)
    {
        return gridPosition.x >= 0 && gridPosition.x < gridWidth &&
               gridPosition.y >= 0 && gridPosition.y < gridHeight;
    }

    /// <summary>
    /// Shuffles an array using Fisher-Yates algorithm
    /// </summary>
    public static void ShuffleArray<T>(T[] array)
    {
        System.Random random = new System.Random();
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            T temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }
}