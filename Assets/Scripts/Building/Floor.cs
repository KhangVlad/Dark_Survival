using System;
using UnityEngine;

[Flags]
public enum WallDirection
{
    None = 0,
    North = 1 << 0,  // +Z direction
    East = 1 << 1,   // +X direction
    South = 1 << 2,  // -Z direction
    West = 1 << 3,   // -X direction
    All = North | East | South | West
}

public class Floor : Building
{
    [SerializeField] private WallDirection wallDirection;
    
    // Dictionary to store walls in each direction
    private Wall[] attachedWalls = new Wall[4];

    public WallDirection WallDirection => wallDirection;

    public void SetWallWithDirection(WallDirection direction, Wall w)
    {
        wallDirection = direction;
        attachedWalls[(int)direction] = w;
    }
    

    private void OnDestroy()
    {
        // Clean up walls when floor is destroyed
        for (int i = 0; i < attachedWalls.Length; i++)
        {
            if (attachedWalls[i] != null)
            {
                Destroy(attachedWalls[i].gameObject);
            }
        }
    }
}
