using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [SerializeField] private string saveFileName = "gameData.json";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }

#if UNITY_EDITOR
    private void OnApplicationQuit()
    {
        SaveGameData();
    }
#endif


    //if mobile
#if UNITY_ANDROID || UNITY_IOS
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGameData();
        }
    }

#endif


    public void SaveGameData()
    {
        SaveUserData();
        SaveWorldData();
    }

    private void SaveUserData()
    {
        UserData userData = UserManager.Instance.userData;
        string json = JsonUtility.ToJson(userData, true);
        string path = Application.persistentDataPath + "/userData.json";
        Debug.Log("Saving user data to: " + path);
        System.IO.File.WriteAllText(path, json);
    }

    private void SaveWorldData()
    {
        if (GridSystem.Instance == null) return;

        WorldData worldData = new WorldData();
        worldData.chunkData = new List<ChunkData>();

        foreach (var chunk in GridSystem.Instance.chunks)
        {
            ChunkData chunkData = new ChunkData
            {
                chunkCoord = chunk.chunkCoord,
                floors = new List<FloorData>()
            };

            // Get chunk dimensions
            int sizeX = chunk.chunkData.GetLength(0);
            int sizeY = chunk.chunkData.GetLength(1);

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    Floor floor = chunk.chunkData[x, y];
                    if (floor != null && floor.buildID != BuildID.None)
                    {
                        FloorData floorData = new FloorData
                        {
                            localPos = new Vector2Int(x, y),
                            buildID = floor.buildID,
                            walls = new List<WallData>()
                        };

                        // Save wall data for this floor
                        for (int i = 0; i < floor.buildingWithDirection.Length; i++)
                        {
                            BuildingWithDirection building = floor.buildingWithDirection[i];
                            if (building.ID != BuildID.None)
                            {
                                WallData wallData = new WallData
                                {
                                    directionIndex = i,
                                    buildID = building.ID
                                };
                                floorData.walls.Add(wallData);
                            }
                        }

                        chunkData.floors.Add(floorData);
                    }
                }
            }

            worldData.chunkData.Add(chunkData);
        }

        string json = JsonUtility.ToJson(worldData, true);
        string path = Application.persistentDataPath + "/worldData.json";
        Debug.Log("Saving world data to: " + path);
        File.WriteAllText(path, json);
    }

    public UserData LoadUserData()
    {
        UserData userData = null;
        string path = Application.persistentDataPath + "/userData.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            userData = JsonUtility.FromJson<UserData>(json);
            Debug.Log("User data loaded from: " + path);
            return userData;
        }

        return null;
    }

    public WorldData LoadWorldData()
    {
        string path = Application.persistentDataPath + "/worldData.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            WorldData worldData = JsonUtility.FromJson<WorldData>(json);
            Debug.Log("World data loaded from: " + path);
            return worldData;
        }

        return null;
    }
}

[Serializable]
public class WorldData
{
    public List<ChunkData> chunkData = new List<ChunkData>();
}

[Serializable]
public class ChunkData
{
    public Vector2Int chunkCoord;
    public List<FloorData> floors = new List<FloorData>();
}

[Serializable]
public class FloorData
{
    public Vector2Int localPos;
    public BuildID buildID;
    public List<WallData> walls = new List<WallData>();
}

[Serializable]
public class WallData
{
    public int directionIndex;
    public BuildID buildID;
}