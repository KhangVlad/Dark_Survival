using UnityEngine;

public class Wall : Building
{
    [SerializeField] private WallDirection direction = WallDirection.Top;
    private Floor attachedFloor;
    public WallDirection Direction => direction;
    
  
}
