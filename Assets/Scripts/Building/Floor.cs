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
    
    //could contain up to 4 wall and 1 object, 
}