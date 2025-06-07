using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class GameResourceManager : MonoBehaviour
{
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

        Utilities.WaitAfter(1f, () => SpawnResourcesOnAllChunks());
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

        Debug.Log($"Attempting to spawn resources on {GridSystem.Instance.chunks.Count} chunks");

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
        int sizeX = chunk.chunkData.GetLength(0);
        int sizeY = chunk.chunkData.GetLength(1);

        // Create a seeded random for this chunk
        int chunkSeed = worldSeed +
                        chunk.chunkCoord.x * 1000 +
                        chunk.chunkCoord.y * 100000;
        Random chunkRandom = new Random(chunkSeed);

        int spawnedCount = 0;
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                Vector2Int localPos = new(x, y);
                if (chunk.IsCellOccupied(localPos)) continue;
                int cellSeed = chunkSeed + x * 73 + y * 31;
                Random cellRandom = new Random(cellSeed);

                float spawnChance = (float)cellRandom.NextDouble();
                if (spawnChance < spawnChancePerCell)
                {
                    var resource = GetWeightedRandomResourceWithSeed(cellRandom);
                    if (resource == null) continue;

                    Vector3 worldPos = GridSystem.Instance.GetWorldPositionFromChunk(chunk.chunkCoord, localPos);

                    // Generate offsets deterministically
                    float offsetX = (float)(cellRandom.NextDouble() * 0.8f - 0.4f);
                    float offsetZ = (float)(cellRandom.NextDouble() * 0.8f - 0.4f);
                    worldPos += new Vector3(offsetX, 0, offsetZ);

                    float rotationY = (float)(cellRandom.NextDouble() * 360f);
                    Quaternion rot = Quaternion.Euler(0f, rotationY, 0f);
                    GameObject instance = Instantiate(resource.prefab, worldPos, rot);
                    if (instance != null)
                    {
                        spawnedCount++;
                        instance.name = $"Resource_{resource.name}";
                    }
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
            totalWeight += res.spawnWeight;
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

            cumulativeWeight += res.spawnWeight;
            if (rnd <= cumulativeWeight)
                return res;
        }

        return resourcePool.resources[resourcePool.resources.Count - 1];
    }
  
   

    // Keep for compatibility with other code that might call it

}