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

    public void SetWall(Direction direction, BuildID id)
    {
        if (!GridSystemExtension.IsValidGridPosition(gridPos, GridSystem.Instance.gridWidth,
                GridSystem.Instance.gridHeight))
        {
            return;
        }
        //none means no wall, so we can set it

        int index = BuildingExtension.GetWallDirectionIndex(direction);
        buildingWithDirection[index] = new BuildingWithDirection();
        buildingWithDirection[index].SetBuilding(id);
    }


    public bool IsDirectionCovered(Direction direction)
    {
        int index = BuildingExtension.GetWallDirectionIndex(direction);
        return buildingWithDirection[index].ID != BuildID.None;
    }

    public bool IsDestroyAble()
    {
        foreach (BuildingWithDirection building in buildingWithDirection)
        {
            if (building.ID != BuildID.None)
            {
                return false; // Can't destroy if any wall exists
            }
        }

        return true;
    }

    public bool IsWallAvailable()
    {
        foreach (BuildingWithDirection building in buildingWithDirection)
        {
            if (building == null)
            {
                Debug.LogError("BuildingWithDirection is null in Floor.");
                continue;
            }

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


    public Floor(BuildID buildID)
    {
        this.buildID = buildID;
        // coveredDirections = Direction.None;
        buildingWithDirection =
            new BuildingWithDirection[4]; // Initialize with 4 directions, index 0 = Top, 1 = Right, 2 = Bot, 3 = Left
        for (int i = 0; i < buildingWithDirection.Length; i++)
        {
            buildingWithDirection[i] = new BuildingWithDirection();
        }
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