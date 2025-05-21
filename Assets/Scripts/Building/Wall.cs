using UnityEngine;

public class Wall : Building
{
    [SerializeField] private WallDirection direction = WallDirection.North;
    private Floor attachedFloor;
    public WallDirection Direction => direction;
    
  
}
