
using UnityEngine;

public abstract class Entity
{
    public EntityID entityID;
    public Vector2Int gridPos;
}


public enum EntityID
{
    None = 0,
    FloorWood = 1,
    WallRock = 2,
    DoorWood = 3,
    Log = 4,
    Bush = 5,
    Stone = 6,
    SpineTree = 7,
}