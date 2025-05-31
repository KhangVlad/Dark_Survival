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
    public BuildingWithDirection[] buildingWithDirection;
    // public Direction coveredDirections = Direction.None;
    public Vector2Int gridPos;

    public void SetWall(Direction direction,BuildID id)
    {
        if (!GridSystemExtension.IsValidGridPosition(gridPos, GridSystem.Instance.gridWidth,
                GridSystem.Instance.gridHeight))
        {
            return;
        }

        // if (direction == Direction.None || direction == Direction.All)
        // {
        //     Debug.LogWarning("Invalid direction for setting a wall.");
        //     return;
        // }
        //
        // coveredDirections |= direction;
        
        int index = BuildingExtension.GetWallDirectionIndex(direction);
        if (index < 0 || index >= buildingWithDirection.Length)
        {
            Debug.LogError("Invalid direction index: " + index);
            return;
        }
        buildingWithDirection[index] = new BuildingWithDirection();
        buildingWithDirection[index].SetBuilding(id);
    }


    public bool IsDirectionCovered(Direction direction)
    {
        // return (coveredDirections & direction) != 0;
        int index = BuildingExtension.GetWallDirectionIndex(direction);
        if (index < 0 || index >= buildingWithDirection.Length)
        {
            Debug.LogError("Invalid direction index: " + index);
            return false;
        }
        return buildingWithDirection[index].ID != BuildID.None;
    }


    public bool IsDestroyAble()
    {
        // return coveredDirections == Direction.None;
        foreach (BuildingWithDirection building in buildingWithDirection)
        {
            if (building.ID != BuildID.None)
            {
                return false;
            }
        }
        return true;
    }

    public bool IsWallAvailable()
    {
        // return coveredDirections != Direction.All;
        foreach (BuildingWithDirection building in buildingWithDirection)
        {
            if (building.ID == BuildID.None)
            {
                return true;
            }
        }
        return false;
    }

    public Direction GetRandomNullDirection()
    {
        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            if (dir == Direction.None || dir == Direction.All) continue;
            if (!IsDirectionCovered(dir))
            {
                return dir;
            }
        }

        Debug.LogError("No uncovered direction found.");
        return Direction.None;
    }

    public bool IsHaveWallAtDirection(Direction direction)
    {
        return IsDirectionCovered(direction);
    }

    public Floor()
    {
        buildID = BuildID.None;
    }

    public Floor(BuildID buildID)
    {
        this.buildID = buildID;
        // coveredDirections = Direction.None;
        buildingWithDirection =
            new BuildingWithDirection[4]; // Initialize with 4 directions, index 0 = Top, 1 = Right, 2 = Bot, 3 = Left
    }
}

[Serializable]
public class BuildingWithDirection
{
    public BuildID ID;

    public void SetBuilding(BuildID id)
    {
        this.ID = id;
    }
}