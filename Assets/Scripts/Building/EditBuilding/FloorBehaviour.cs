using UnityEngine;

public class FloorBehaviour : BuildBehaviour
{
    public Vector2Int gridPosition;

    public void Init(EntityID id, Vector2Int position)
    {
        SetBuildID(id);
        gridPosition = position;
    }


    // Method to apply changes to the floor
    public override void DeleteBuilding()
    {
        Floor floor = GridSystem.Instance.GetEntityById<Floor>(gridPosition,out _);
        if (floor.IsDestroyAble())
        {
            GridSystem.Instance.SetFloorData(gridPosition, EntityID.None); 
            Destroy(this.gameObject);
        }
    }
    public override void InteractWithBuilding()
    {
    }
}