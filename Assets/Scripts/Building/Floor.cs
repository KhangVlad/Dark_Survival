using System;
using UnityEngine;

[Flags]
public enum Direction
{
    None = 0,
    Top = 1 << 0, 
    Right = 1 << 1,   
    Bot = 1 << 2, 
    Left = 1 << 3,  
    All = Top | Right | Bot | Left
}
// public enum WallDirection
// {
//     None = 0,
//     Top = 1 << 0, 
//     Right = 1 << 1,   
//     Bot = 1 << 2, 
//     Left = 1 << 3,  
//     All = Top | Right | Bot | Left
// }

public class Floor : Building
{
    public Wall[] attachedWalls = new Wall[4];
    public void SetWallWithDirection(Direction direction, Wall w)
    {
        Vector2Int gridPosition = GridSystem.Instance.WorldToGridPosition(transform.position);

        // Check if the grid position is valid
        if (!GridSystemExtension.IsValidGridPosition(gridPosition,GridSystem.Instance.gridWidth, GridSystem.Instance.gridHeight))
        {
            Debug.LogWarning($"Cannot set wall. Floor at {gridPosition} is outside of grid bounds.");
            return;
        }
    

        // Map WallDirection to array index
        int directionIndex = BuildingExtension.GetWallDirectionIndex(direction);
        if (directionIndex < 0 || directionIndex >= attachedWalls.Length)
        {
            Debug.LogError($"Invalid WallDirection: {direction}. Index {directionIndex} is out of bounds.");
            return;
        }

        Debug.Log($"SetWallWithDirection {direction}");
        attachedWalls[directionIndex] = w;
    }
    
    public bool IsDirectionCovered(Direction direction)
    {
        int directionIndex = BuildingExtension.GetWallDirectionIndex(direction);
        if (directionIndex < 0 || directionIndex >= attachedWalls.Length)
        {
            Debug.LogError($"Invalid WallDirection: {direction}. Index {directionIndex} is out of bounds.");
            return false;
        }
        
        return attachedWalls[directionIndex] != null;
    }
    
    public Direction GetRandomAvailableDirection()
    {
        Direction availableDirections = Direction.None;
        for (int i = 0; i < attachedWalls.Length; i++)
        {
            if (attachedWalls[i] == null)
            {
                availableDirections |= (Direction)(1 << i);
            }
        }

        if (availableDirections == Direction.None)
        {
            return Direction.None; // No available directions
        }

        // Randomly select one of the available directions
        int randomIndex = UnityEngine.Random.Range(0, (int)Mathf.Pow(2, 4));
        return (Direction)randomIndex & availableDirections;
    }
  

    // private int GetWallDirectionIndex(WallDirection direction)
    // {
    //     return direction switch
    //     {
    //         WallDirection.Top => 0,
    //         WallDirection.Right => 1,
    //         WallDirection.Bot => 2,
    //         WallDirection.Left => 3,
    //         _ => -1 // Invalid direction
    //     };
    // }
    
}
