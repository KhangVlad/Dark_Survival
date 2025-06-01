using UnityEngine;

public static class EditBuildingFactory
{
    public static BuildBehaviour AddEditScripts(BuildID id, GameObject g)
    {
        BuildBehaviour buildBehaviour = null;

        switch (id)
        {
            case BuildID.Floor:
                buildBehaviour = g.AddComponent<FloorBehaviour>();
                break;
            case BuildID.Wall:
                buildBehaviour = g.AddComponent<WallBehaviour>();
                break;
            case BuildID.Door:
                buildBehaviour = g.AddComponent<DoorBehaviour>();
                break;
        }
        return buildBehaviour;
    }
}