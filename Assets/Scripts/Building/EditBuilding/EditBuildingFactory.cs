using UnityEngine;

public static class EditBuildingFactory
{
    public static BuildBehaviour AddEditScripts(EntityID id, GameObject g)
    {
        BuildBehaviour buildBehaviour = null;

        switch (id)
        {
            case EntityID.Floor:
                buildBehaviour = g.AddComponent<FloorBehaviour>();
                break;
            case EntityID.Wall:
                buildBehaviour = g.AddComponent<WallBehaviour>();
                break;
            case EntityID.Door:
                buildBehaviour = g.AddComponent<DoorBehaviour>();
                break;
        }
        return buildBehaviour;
    }
}