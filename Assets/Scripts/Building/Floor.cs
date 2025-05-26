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


[Serializable]
public class Floor : Building
{
    public Wall[] attachedWalls;
    public Vector2Int gridPos;
    public void SetWallWithDirection(Direction direction, Wall w)
    {
        Debug.Log($"Setting wall at direction: {direction}");
        Vector2Int gridPosition = gridPos;

        // Check if the grid position is valid
        if (!GridSystemExtension.IsValidGridPosition(gridPosition, GridSystem.Instance.gridWidth, GridSystem.Instance.gridHeight))
        {
            return;
        }
    
        // Check if attachedWalls is initialized
        if (attachedWalls == null)
        {
            attachedWalls = new Wall[4];
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
        if (attachedWalls[directionIndex] == null)
        {
            return false;
        }
        if (attachedWalls[directionIndex].buildID == BuildID.None)
        {
            return false;
        }
        return true;
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
    // public bool IsWallAvailable()
    // {
    //     
    //     // foreach (var wall in attachedWalls)
    //     // {
    //     //     if (wall == null)
    //     //     {
    //     //         return true; // At least one wall is available
    //     //     }
    //     // }
    //     // return false; // No walls are available
    //     
    //     //check if any wall buildid == BuildID.None
    //     foreach (var wall in attachedWalls)
    //     {
    //         if ( wall.buildID == BuildID.None)
    //         {
    //             return true; // At least one wall is available
    //         }
    //     }
    //     return false; // No walls are available
    // }
    public bool IsWallAvailable()
    {
        foreach (var wall in attachedWalls)
        {
            if (wall == null || wall.buildID == BuildID.None)
            {
                return true; // Wall is null or has BuildID.None
            }
        }
        return false; // No available walls
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


    public Floor()
    {
        buildID = BuildID.None; // Set the default buildID to None
    }

    public Floor(BuildID buildID)
    {
        this.buildID = buildID; // Set the buildID to the provided value
        attachedWalls = new Wall[4]; // Initialize the attachedWalls array with 4 elements
    }
}
