using UnityEngine;

public class FloorBehaviour : BuildBehaviour
{
    public int floorIndex = 0;
    public Vector2Int gridPosition;

    public void Init(int index, BuildID id, Vector2Int position)
    {
        floorIndex = index;
        SetBuildID(id);
        gridPosition = position;
    }


    // Method to apply changes to the floor
    public override void DeleteBuilding()
    {
        Floor floor = GridSystem.Instance.GetFloorAt(gridPosition,out _);
        if (floor.IsDestroyAble())
        {
            GridSystem.Instance.SetFloorData(gridPosition, BuildID.None); 
            Destroy(this.gameObject);
        }
        else
        {
            Debug.LogError($"Cannot delete floor at position {gridPosition} because it has walls or other buildings.");
        }
    }
    public override void InteractWithBuilding()
    {
      
    }
}