using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

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
                entities = new List<EnityData>()
            };

            // Get chunk dimensions
            int sizeX = chunk.chunkData.GetLength(0);
            int sizeY = chunk.chunkData.GetLength(1);

            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    Entity e = chunk.chunkData[x, y];
                    if (e != null && e.entityID != EntityID.None)
                    {
                        EnityData entityData = new EnityData
                        {
                            localPos = new Vector2Int(x, y),
                            entityID = e.entityID,
                            walls = new List<WallData>()
                        };

                        if (e is Floor floor && floor.buildingWithDirection != null)
                        {
                            // Save wall data for floors
                            for (int i = 0; i < floor.buildingWithDirection.Length; i++)
                            {
                                var building = floor.buildingWithDirection[i];

                                if (building != null && building.ID != EntityID.None)
                                {
                                    WallData wallData = new WallData
                                    {
                                        directionIndex = i,
                                        entityID = building.ID
                                    };

                                    entityData.walls.Add(wallData);
                                }
                            }
                        }
                        else if (e is ResourceNode)
                        {
                            // For resource nodes, we only need to store the entity ID
                            // No wall data needed
                            entityData.walls = null;
                        }

                        chunkData.entities.Add(entityData);
                    }
                }
            }

            worldData.chunkData.Add(chunkData);
        }

        string json = JsonUtility.ToJson(worldData, true);
        string path = Application.persistentDataPath + "/worldData.json";
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
    public List<EnityData> entities = new List<EnityData>();
}

[Serializable]
public class EnityData
{
    public Vector2Int localPos;
    public EntityID entityID;
    public List<WallData> walls = new List<WallData>();
}

[Serializable]
public class WallData
{
    public int directionIndex;
    public EntityID entityID;
}