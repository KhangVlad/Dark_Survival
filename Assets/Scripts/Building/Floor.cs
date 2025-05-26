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


public class Floor : Building
{
    public Wall[] attachedWalls = new Wall[4];
    public void SetWallWithDirection(Direction direction, Wall w)
    {
        Vector2Int gridPosition = GridSystem.Instance.WorldToGridPosition(transform.position);

        // Check if the grid position is valid
        if (!GridSystemExtension.IsValidGridPosition(gridPosition,GridSystem.Instance.gridWidth, GridSystem.Instance.gridHeight))
        {
            return;
        }
        int directionIndex = BuildingExtension.GetWallDirectionIndex(direction);
        if (directionIndex < 0 || directionIndex >= attachedWalls.Length)
        {
            return;
        }
        attachedWalls[directionIndex] = w;
    }
    
    public bool IsDirectionCovered(Direction direction)
    {
        int directionIndex = BuildingExtension.GetWallDirectionIndex(direction);
        if (directionIndex < 0 || directionIndex >= attachedWalls.Length)
        {
            return false;
        }
        
        return attachedWalls[directionIndex] != null;
    }
    
    public bool IsDestroyAble()
    {
        // Check if all walls are null
        foreach (var wall in attachedWalls)
        {
            if (wall != null)
            {
                return false; // If any wall is present, the floor cannot be destroyed
            }
        }
        return true; // All walls are null, the floor can be destroyed
    }
    
   //check if atleast 1 null wall
    public bool IsWallAvailable()
    {
        foreach (var wall in attachedWalls)
        {
            if (wall == null)
            {
                return true; // At least one wall is available
            }
        }
        return false; // No walls are available
    }
    
    public Direction GetRandomNullDirection()
    {
        Direction[] directions = (Direction[])Enum.GetValues(typeof(Direction));
        foreach (var direction in directions)
        {
            if (direction != Direction.None && !IsDirectionCovered(direction))
            {
                return direction; // Return the first uncovered direction
            }
        }
        Debug.LogError("No uncovered direction found.");
        return Direction.None;
    }
    
    
    public bool IsHaveWallAtDirection(Direction direction)
    {
        int directionIndex = BuildingExtension.GetWallDirectionIndex(direction);
        if (directionIndex < 0 || directionIndex >= attachedWalls.Length)
        {
            return false; // Invalid direction index
        }
        
        return attachedWalls[directionIndex] != null; // Check if the wall at the specified direction is not null
    }
    
    public Wall GetWallAtDirection(Direction direction)
    {
        int directionIndex = BuildingExtension.GetWallDirectionIndex(direction);
        if (directionIndex < 0 || directionIndex >= attachedWalls.Length)
        {
            return null; // Invalid direction index
        }
        
        return attachedWalls[directionIndex]; // Return the wall at the specified direction
    }
   
    
    
}
