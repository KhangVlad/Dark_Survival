using System;
using UnityEngine;
using System.Collections.Generic;

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
    public BuildingWithDirection[] buildingWithDirection;

    // public Direction coveredDirections = Direction.None;
    public Vector2Int gridPos;

    public void SetWall(Direction direction, EntityID id)
    {
        if (!GridSystemExtension.IsValidGridPosition(gridPos, GridSystem.Instance.GridWidth,
                GridSystem.Instance.GridHeight))
        {
            return;
        }
        int index = BuildingExtension.GetWallDirectionIndex(direction);
        buildingWithDirection[index] = new BuildingWithDirection();
        buildingWithDirection[index].SetBuilding(id);
    }


    public bool IsDirectionCovered(Direction direction)
    {
        int index = BuildingExtension.GetWallDirectionIndex(direction);
        return buildingWithDirection[index].ID != EntityID.None;
    }

    public bool IsDestroyAble()
    {
        foreach (BuildingWithDirection building in buildingWithDirection)
        {
            if (building.ID != EntityID.None)
            {
                return false; // Can't destroy if any wall exists
            }
        }

        return true;
    }

  
    public bool IsWallAvailable()
    {
       
        for (int i = 0; i < buildingWithDirection.Length; i++)
        {
            BuildingWithDirection building = buildingWithDirection[i];
            if (building == null)
            {
                Debug.LogError($"BuildingWithDirection at index {i} is null in Floor at position {gridPos}");
                continue;
            }

            if (building.ID == EntityID.None)
            {
                return true;
            }
        }

        return false;
    }

 
 

    public Floor(EntityID entityID)
    {
        this.entityID = entityID;
        // coveredDirections = Direction.None;
        buildingWithDirection =
            new BuildingWithDirection[4];
        for (int i = 0; i < buildingWithDirection.Length; i++)
        {
            buildingWithDirection[i] = new BuildingWithDirection();
        }
    }
}

[Serializable]
public class BuildingWithDirection
{
    public EntityID ID;

    public void SetBuilding(EntityID id)
    {
        this.ID = id;
    }
}