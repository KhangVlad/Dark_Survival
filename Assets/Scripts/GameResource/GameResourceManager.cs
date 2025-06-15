using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = System.Random;

public class GameResourceManager : MonoBehaviour
{
    public static GameResourceManager Instance { get; private set; }

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

    public ResourcePoolSO resourcePool;
    public int worldSeed = 12345; // Default seed for random generation

    [Range(0f, 1f)] public float spawnChancePerCell = 0.1f; // Tỷ lệ spawn 10% mỗi ô

    private void Start()
    {
        // Check if resourcePool is assigned
        if (resourcePool == null)
        {
            return;
        }

        CheckFirstTime();

    }
    private void CheckFirstTime()
    {
        if (UserManager.Instance.userData.IsFirstLogin)
        {
            SpawnResourcesOnAllChunks();
            UserManager.Instance.userData.IsFirstLogin = false; // Set to false after first login
        }
    }

    public void SetSeed(int seed)
    {
        worldSeed = seed;
    }



    public void SpawnResourcesOnAllChunks()
    {
        if (GridSystem.Instance == null || GridSystem.Instance.chunks == null)
        {
            Debug.LogError("GridSystem or chunks array is null");
            return;
        }

        foreach (var chunk in GridSystem.Instance.chunks)
        {
            if (chunk != null)
                SpawnResourcesInChunk(chunk);
            else
                Debug.LogWarning("Null chunk found in chunks list");
        }
    }


    void SpawnResourcesInChunk(Chunk chunk)
    {
        // Clear any existing resources in this chunk
        if (chunk.resourcesParent != null)
        {
            foreach (Transform child in chunk.resourcesParent.transform)
            {
                Destroy(child.gameObject);
            }
        }

        // Create a deterministic random number generator based on chunk coordinates and world seed
        Random random = new Random(worldSeed + chunk.chunkCoord.x * 1000 + chunk.chunkCoord.y);

        // Get chunk dimensions from the chunk data array
        int chunkSizeX = chunk.chunkData.GetLength(0);
        int chunkSizeY = chunk.chunkData.GetLength(1);

        // Calculate global chunk position
        Vector2Int chunkOffset = chunk.chunkCoord * new Vector2Int(chunkSizeX, chunkSizeY);

        // Iterate through each cell in the chunk
        for (int x = 0; x < chunkSizeX; x++)
        {
            for (int y = 0; y < chunkSizeY; y++)
            {
                Vector2Int localPos = new Vector2Int(x, y);
                Vector2Int globalPos = chunkOffset + localPos;

                // Skip if cell is already occupied
                if (chunk.IsCellOccupied(localPos))
                    continue;

                // Random chance to spawn a resource at this position
                if (random.NextDouble() <= spawnChancePerCell)
                {
                    // Get a random resource based on weights
                    ResourceDataSO resourceData = GetWeightedRandomResourceWithSeed(random);
                    if (resourceData == null || resourceData.prefab == null)
                        continue;
                    ResourceNode resourceNode = new ResourceNode(resourceData.ItemID);
                    Vector3 worldPos = GridSystem.Instance.GridPosToWorldPosition(globalPos);
                    worldPos += new Vector3(1 * 0.5f, 0, 1 * 0.5f);
                    float randomYRotation = (float)(random.NextDouble() * 360f); // Random rotation between 0-360 degrees
                    GameObject resourceObj = Instantiate(resourceData.prefab, worldPos, Quaternion.Euler(0, randomYRotation, 0));
                    resourceObj.transform.SetParent(chunk.resourcesParent.transform);
                    ResourceFactory.CreateResourceBehavior(resourceNode, resourceObj);
                    chunk.SetCell(localPos, resourceNode, resourceObj);
                }
            }
        }
    }
    ResourceDataSO GetWeightedRandomResourceWithSeed(Random random)
    {
        if (resourcePool == null)
        {
            Debug.LogError("ResourcePoolSO is null");
            return null;
        }

        if (resourcePool.resources == null || resourcePool.resources.Count == 0)
        {
            Debug.LogWarning("No resources found in resource pool");
            return null;
        }

        float totalWeight = 0f;
        foreach (var res in resourcePool.resources)
        {
            if (res == null) continue;
            totalWeight += res.SpawnWeight;
        }

        // Add this check to prevent accessing empty collection
        if (totalWeight <= 0f)
        {
            Debug.LogWarning("Total resource spawn weight is zero or negative");
            return null;
        }

        float rnd = (float)random.NextDouble() * totalWeight;
        float cumulativeWeight = 0f;

        foreach (var res in resourcePool.resources)
        {
            if (res == null) continue;

            cumulativeWeight += res.SpawnWeight;
            if (rnd <= cumulativeWeight)
                return res;
        }

        return resourcePool.resources[resourcePool.resources.Count - 1];
    }


    public GameObject GetResourcePrefabByID(EntityID id)
    {
        if (resourcePool == null || resourcePool.resources == null)
        {
            Debug.LogError("ResourcePoolSO or resources list is null");
            return null;
        }

        foreach (var resource in resourcePool.resources)
        {
            if (resource != null && resource.ItemID == id)
            {
                return resource.prefab;
            }
        }

        Debug.LogWarning($"Resource with ID {id} not found in pool");
        return null;
    }
}