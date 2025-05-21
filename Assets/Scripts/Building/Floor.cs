using System;
using UnityEngine;

[Flags]
public enum WallDirection
{
    None = 0,
    North = 1 << 0, 
    East = 1 << 1,   
    South = 1 << 2, 
    West = 1 << 3,  
    All = North | East | South | West
}

public class Floor : Building
{
    private Wall[] attachedWalls = new Wall[4];
    public void SetWallWithDirection(WallDirection direction, Wall w)
    {
        attachedWalls[(int)direction] = w;
    }
    
}
