using System;
using UnityEngine;

[Serializable]
public class Chunk
{
    public Vector2Int chunkCoord;
    public Floor[,] chunkData;
    public Floor[] floors;
    public GameObject floorsParent;
    public GameObject wallsParents;
    public GameObject doorsParent; //no need combine mesh, just for edit
    public bool NeedRebuild = false;

    public Chunk(Vector2Int coord, int sizeX, int sizeY)
    {
        floorsParent = new GameObject($"Floor_{coord.x}_{coord.y}");
        wallsParents = new GameObject($"Wall_{coord.x}_{coord.y}");
        doorsParent = new GameObject($"Door_{coord.x}_{coord.y}");
        chunkCoord = coord;
        chunkData = new Floor[sizeX, sizeY];

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                chunkData[x, y] = new Floor(BuildID.None);
                chunkData[x, y].gridPos = new Vector2Int(x, y);
            }
        }

        floors = new Floor[sizeX * sizeY];
    }

    public Floor GetCell(Vector2Int localPos)
    {
        return chunkData[localPos.x, localPos.y];
    }

    public void SetCell(Vector2Int localPos, Floor floor, GameObject g, int chunkWidth)
    {
        chunkData[localPos.x, localPos.y] = floor;
        int index = localPos.x + localPos.y * chunkWidth;
        floors[index] = floor;
        g.transform.SetParent(floorsParent.transform);
        NeedRebuild = true;
    }

    public bool IsCellOccupied(Vector2Int localPos)
    {
        return chunkData[localPos.x, localPos.y].buildID != BuildID.None;
    }

    public void SetWallWithDirection(Vector2Int localPos, Direction d, GameObject g, BuildID id)
    {
        chunkData[localPos.x, localPos.y].SetWall(d, id);
        switch (id)
        {
            case BuildID.Wall:
                g.transform.SetParent(wallsParents.transform);
                break;
            case BuildID.Door:
                g.transform.SetParent(doorsParent.transform);
                break;
        }

        NeedRebuild = true;
    }

    public void SetWallData(Vector2Int localPos, Direction d, BuildID id)
    {
        if (chunkData[localPos.x, localPos.y] == null)
        {
            Debug.LogWarning("Cannot set wall on an empty cell.");
            return;
        }

        chunkData[localPos.x, localPos.y].SetWall(d, id);
        NeedRebuild = true;
    }

    public void SetFloorData(Vector2Int localPos, BuildID id)
    {
        chunkData[localPos.x, localPos.y].buildID = id;
        chunkData[localPos.x, localPos.y].buildingWithDirection = null;
        NeedRebuild = true;
    }
}