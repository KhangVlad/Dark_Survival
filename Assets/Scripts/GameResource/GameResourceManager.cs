using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameResourceManager : MonoBehaviour
{
    public static GameResourceManager Instance { get; private set; }
    
    [Header("Resource Settings")]
    [SerializeField] private ResourceDataSO[] resourcesData;
    [SerializeField] private float resourceDensity = 0.05f; // Percentage of grid to fill
    [SerializeField] private Transform resourcesParent;
    [SerializeField] private LayerMask groundLayer;

    private GridSystem gridSystem;
    private List<GameObject> spawnedResources = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        
        if (resourcesParent == null)
        {
            resourcesParent = new GameObject("Resources").transform;
            resourcesParent.SetParent(transform);
        }
    }

    private void Start()
    {
        gridSystem = GridSystem.Instance;
        InitializeResourcesToGrid();
    }

    // private void InitializeResourcesToGrid()
    // {
    //     //resourcesData = Resources.LoadAll<ResourceDataSO>("Resources");
    //     resourcesData =Resources.LoadAll<ResourceDataSO>("EnviromentResources");
    //     
    //     if (resourcesData == null || resourcesData.Length == 0)
    //     {
    //         Debug.LogWarning("No resource data assigned to GameResourceManager");
    //         return;
    //     }
    //
    //     // Calculate total weight for weighted random selection
    //     float totalWeight = 0;
    //     foreach (ResourceDataSO data in resourcesData)
    //     {
    //         totalWeight += data.spawnWeight;
    //     }
    //     
    //     // Get available positions for resources
    //     List<Vector2Int> availablePositions = GetAvailableGridPositions();
    //     ShuffleList(availablePositions);
    //     
    //     // Calculate number of resources to spawn
    //     int gridSize = gridSystem.gridWidth * gridSystem.gridHeight;
    //     int resourceCount = Mathf.FloorToInt(gridSize * resourceDensity);
    //     
    //     // Spawn each type of resource according to their min/max amounts
    //     foreach (ResourceDataSO resourceData in resourcesData)
    //     {
    //         int amountToSpawn = Random.Range(resourceData.minAmount, resourceData.maxAmount + 1);
    //         for (int i = 0; i < amountToSpawn && availablePositions.Count > 0; i++)
    //         {
    //             // Get a random position from available positions
    //             int randomIndex = Random.Range(0, availablePositions.Count);
    //             Vector2Int position = availablePositions[randomIndex];
    //             availablePositions.RemoveAt(randomIndex);
    //             
    //             SpawnResource(resourceData, position);
    //         }
    //     }
    //     
    //     // Spawn additional random resources up to resource count
    //     while (spawnedResources.Count < resourceCount && availablePositions.Count > 0)
    //     {
    //         // Select resource based on weight
    //         ResourceDataSO selectedResource = SelectRandomResource(totalWeight);
    //         
    //         // Get a random position from available positions
    //         int randomIndex = Random.Range(0, availablePositions.Count);
    //         Vector2Int position = availablePositions[randomIndex];
    //         availablePositions.RemoveAt(randomIndex);
    //         
    //         SpawnResource(selectedResource, position);
    //     }
    //     
    //     Debug.Log($"Spawned {spawnedResources.Count} resources");
    // }
   private void InitializeResourcesToGrid()
{
    resourcesData = Resources.LoadAll<ResourceDataSO>("EnviromentResources");
    Debug.Log($"Loaded {resourcesData.Length} resource types");

    if (resourcesData == null || resourcesData.Length == 0)
    {
        Debug.LogWarning("No resource data assigned to GameResourceManager");
        return;
    }

    // Log each resource's configuration
    foreach (ResourceDataSO data in resourcesData)
    {
        Debug.Log($"Resource {data.itemID}: Weight={data.spawnWeight}, Min={data.minAmount}, Max={data.maxAmount}");
    }

    // Calculate total weight for weighted random selection
    float totalWeight = 0;
    foreach (ResourceDataSO data in resourcesData)
    {
        totalWeight += data.spawnWeight;
    }
    Debug.Log($"Total weight: {totalWeight}");

    // Get available positions for resources
    List<Vector2Int> availablePositions = GetAvailableGridPositions();
    Debug.Log($"Found {availablePositions.Count} available positions");
    ShuffleList(availablePositions);

    // Calculate number of resources to spawn
    int gridSize = gridSystem.gridWidth * gridSystem.gridHeight;
    int resourceCount = Mathf.FloorToInt(gridSize * resourceDensity);
    Debug.Log($"Grid size: {gridSize}, Resource density: {resourceDensity}, Target count: {resourceCount}");

    // Spawn each type of resource according to their min/max amounts
    foreach (ResourceDataSO resourceData in resourcesData)
    {
        int amountToSpawn = Random.Range(resourceData.minAmount, resourceData.maxAmount + 1);
        Debug.Log($"Attempting to spawn {amountToSpawn} of {resourceData.itemID}");
        
        for (int i = 0; i < amountToSpawn && availablePositions.Count > 0; i++)
        {
            // Get a random position from available positions
            int randomIndex = Random.Range(0, availablePositions.Count);
            Vector2Int position = availablePositions[randomIndex];
            availablePositions.RemoveAt(randomIndex);

            SpawnResource(resourceData, position);
        }
    }

    // Spawn additional random resources up to resource count
    while (spawnedResources.Count < resourceCount && availablePositions.Count > 0)
    {
        // Select resource based on weight
        ResourceDataSO selectedResource = SelectRandomResource(totalWeight);

        // Get a random position from available positions
        int randomIndex = Random.Range(0, availablePositions.Count);
        Vector2Int position = availablePositions[randomIndex];
        availablePositions.RemoveAt(randomIndex);

        SpawnResource(selectedResource, position);
    }

    Debug.Log($"Spawned {spawnedResources.Count} resources");
}

    private List<Vector2Int> GetAvailableGridPositions()
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        for (int x = 0; x < gridSystem.gridWidth; x++)
        {
            for (int y = 0; y < gridSystem.gridHeight; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Vector3 worldPos = GridToWorldPosition(pos);

                // Changed from ~groundLayer to groundLayer
                if (!Physics.Raycast(worldPos + Vector3.up * 5, Vector3.down, 10f, groundLayer))
                {
                    positions.Add(pos);
                }
            }
        }

        return positions;
    }
    
    private Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(
            gridSystem.gridOrigin.x + gridPosition.x * gridSystem.cellSize + gridSystem.cellSize * 0.5f,
            gridSystem.gridOrigin.y,
            gridSystem.gridOrigin.z + gridPosition.y * gridSystem.cellSize + gridSystem.cellSize * 0.5f
        );
    }

    private ResourceDataSO SelectRandomResource(float totalWeight)
    {
        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;
        
        foreach (ResourceDataSO data in resourcesData)
        {
            cumulativeWeight += data.spawnWeight;
            if (randomValue <= cumulativeWeight)
            {
                return data;
            }
        }
        
        return resourcesData[0];
    }

    private void SpawnResource(ResourceDataSO resourceData, Vector2Int gridPosition)
    {
        Vector3 worldPosition = GridToWorldPosition(gridPosition);
        GameObject resource = Instantiate(resourceData.prefab, worldPosition, Quaternion.identity, resourcesParent);
        resource.name = $"{resourceData.itemID}_{gridPosition.x}_{gridPosition.y}";
        spawnedResources.Add(resource);
    }

    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = 0; i < n; i++)
        {
            int r = i + Random.Range(0, n - i);
            T temp = list[i];
            list[i] = list[r];
            list[r] = temp;
        }
    }
    
    // Public method to get resource at a specific position
    public GameObject GetResourceAt(Vector2Int gridPosition)
    {
        Vector3 worldPos = GridToWorldPosition(gridPosition);
        
        Collider[] colliders = Physics.OverlapSphere(worldPos, gridSystem.cellSize * 0.4f);
        foreach (Collider collider in colliders)
        {
            // Check if this is one of our resources
            if (collider.transform.parent == resourcesParent)
            {
                return collider.gameObject;
            }
        }
        
        return null;
    }
    
    // Public method to remove a resource
    public bool RemoveResourceAt(Vector2Int gridPosition)
    {
        GameObject resource = GetResourceAt(gridPosition);
        if (resource != null)
        {
            spawnedResources.Remove(resource);
            Destroy(resource);
            return true;
        }
        return false;
    }
}