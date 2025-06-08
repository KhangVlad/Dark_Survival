using UnityEngine;

public class WallBehaviour : BuildBehaviour
{
    public int floorIndex = -1;
    public int wallIndex = -1;
    public Vector2Int gridPosition;

    public void Init(int wIndex, int floor, EntityID id, Vector2Int position)
    {
        wallIndex = wIndex;
        floorIndex = floor;
        SetBuildID(id);
        gridPosition = position;
    }


    public override void DeleteBuilding()
    {
        Direction direction = BuildingExtension.GetWallDirectionByIndex(wallIndex);
        GridSystem.Instance.SetWallData(gridPosition, direction, EntityID.None);
        Destroy(this.gameObject);
    }
    
    public override void InteractWithBuilding()
    {
      
    }
}