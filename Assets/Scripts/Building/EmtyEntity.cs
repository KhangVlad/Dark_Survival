using UnityEngine;

public class EmtyEntity : Entity
{
    public EmtyEntity(Vector2Int v)
    {
        entityID = EntityID.None;
        gridPos = v;
    }
}