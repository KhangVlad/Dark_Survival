using System;
using UnityEngine;


[Serializable]
public class Chunk
{
    public Vector2Int chunkCoord;
    public Entity[,] chunkData;
    [HideInInspector] public GameObject chunkParent;
    [HideInInspector] public GameObject floorsParent;
    [HideInInspector] public GameObject wallsParents;
    [HideInInspector] public GameObject doorsParent;
    [HideInInspector] public GameObject resourcesParent;
    public bool NeedRebuild = false;

    public Chunk(Vector2Int coord, int sizeX, int sizeY)
    {
        // Create parent GameObject for the entire chunk
        chunkParent = new GameObject($"Chunk_{coord.x}_{coord.y}");

        // Create child GameObjects and set their parent
        floorsParent = new GameObject($"Floor_{coord.x}_{coord.y}");
        floorsParent.transform.SetParent(chunkParent.transform);

        wallsParents = new GameObject($"Wall_{coord.x}_{coord.y}");
        wallsParents.transform.SetParent(chunkParent.transform);

        doorsParent = new GameObject($"Door_{coord.x}_{coord.y}");
        doorsParent.transform.SetParent(chunkParent.transform);

        resourcesParent = new GameObject($"Resource_{coord.x}_{coord.y}");
        resourcesParent.transform.SetParent(chunkParent.transform);

        chunkCoord = coord;
        chunkData = new Entity[sizeX, sizeY];
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                chunkData[x, y] = new EmtyEntity(new Vector2Int(x, y));
            }
        }
    }


    public Entity GetCell(Vector2Int localPos)
    {
        return chunkData[localPos.x, localPos.y];
    }

    public void SetCell(Vector2Int localPos, Entity e, GameObject g)
    {
        chunkData[localPos.x, localPos.y] = e;
        if (e is Floor f)
        {
            g.transform.SetParent(floorsParent.transform);
        }
        else if (e is ResourceNode r)
        {
            g.transform.SetParent(resourcesParent.transform);
            SetFloorData(localPos, r.entityID);
        }
        else
        {
            Debug.LogWarning($"Unknown entity type: {e.GetType()} at position {localPos}");
            return;
        }
        e.gridPos = localPos;
        // g.transform.SetParent(floorsParent.transform);
        NeedRebuild = true;
    }

    public void SetWallWithDirection(Vector2Int localPos, Direction d, GameObject g, EntityID id)
    {
        Entity floor = GetCell(localPos);
        if (floor is Floor f)
        {
            f.SetWall(d, id);
            switch (id)
            {
                case EntityID.Wall:
                    g.transform.SetParent(wallsParents.transform);
                    break;
                case EntityID.Door:
                    g.transform.SetParent(doorsParent.transform);
                    break;
            }

            NeedRebuild = true;
        }
        else
        {
            Debug.LogWarning("Cannot set wall on a non-floor entity.");
        }
    }

    public void SetWallData(Vector2Int localPos, Direction d, EntityID id)
    {
        Entity floor = GetCell(localPos);
        if (floor is Floor f)
        {
            f.SetWall(d, id);
            NeedRebuild = true;
        }

    }

    public void SetFloorData(Vector2Int localPos, EntityID id)
    {
        Entity floor = GetCell(localPos);
        if (floor is Floor f)
        {
            f.entityID = id;
            f.buildingWithDirection = null;
            NeedRebuild = true;
        }

    }

    public bool IsCellOccupied(Vector2Int localPos)
    {
        return chunkData[localPos.x, localPos.y].entityID != EntityID.None;
    }
}