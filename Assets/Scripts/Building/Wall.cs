using UnityEngine;

public class Wall : Building
{
    [SerializeField] private Direction direction = global::Direction.Top;
    private Floor attachedFloor;
  
}
