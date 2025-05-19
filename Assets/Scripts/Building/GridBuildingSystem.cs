using System;
using UnityEngine;

public class GridBuildingSystem : MonoBehaviour
{

    public static GridBuildingSystem Instance { get; private set; }

    [Header("Grid Settings")] public int gridWidth = 50;
    public int gridHeight = 50;
    public float cellSize = 1f;
    public Vector3 gridOrigin = Vector3.zero;

    [Header("Gizmos")] public bool showGrid = true;
    public Color gridColor = Color.white;
    public Color occupiedCellColor = Color.red;

    [Header("Building")] public BuildDataSO currentSelectedObject;
    public LayerMask groundLayer = 1;

    // Grid data structure - stores what's in each cell
    private Building[,] gridData;
    private Building[,] spawnedObjects;


    private Camera playerCamera;

    #region Event


    #endregion

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
    }

    void Start()
    {
        playerCamera = Camera.main;
        InitializeGrid();
    }


    void InitializeGrid()
    {
        gridData = new Building[gridWidth, gridHeight];
        spawnedObjects = new Building[gridWidth, gridHeight];
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        if (currentSelectedObject == null) return;

        Vector3 mousePosition = Input.mousePosition;
        Ray ray = playerCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            Vector2Int gridPosition = WorldToGridPosition(hit.point);

            if (Input.GetMouseButtonDown(0))
            {
                TryBuildObject(gridPosition);
            }

            // Optional: Add right-click to remove buildings
            if (Input.GetMouseButtonDown(1))
            {
                TryRemoveObject(gridPosition);
            }
        }
    }

    public bool TryBuildObject(Vector2Int gridPosition)
    {
        if (!IsValidGridPosition(gridPosition)) return false;
        if (!CanBuildAt(gridPosition, currentSelectedObject)) return false;

        // Check if all cells in the building size are available
        for (int x = 0; x < currentSelectedObject.gridSize.x; x++)
        {
            for (int y = 0; y < currentSelectedObject.gridSize.y; y++)
            {
                Vector2Int checkPos = gridPosition + new Vector2Int(x, y);
                if (!IsValidGridPosition(checkPos))
                {
                    Debug.Log("Cannot build here - out of bounds");
                    return false;
                }

                Building existingBuilding = gridData[checkPos.x, checkPos.y];

                // Check building rules based on type
                if (!CanPlaceOnCell(checkPos, currentSelectedObject, existingBuilding))
                {
                    Debug.Log($"Cannot build {currentSelectedObject.buildID} at grid position {checkPos}");
                    return false;
                }
            }
        }

        // Build the object
        Vector3 worldPosition = GridToWorldPosition(gridPosition);
        GameObject newObject = Instantiate(currentSelectedObject.prefab, worldPosition, Quaternion.identity);

        // Add the appropriate component based on building type
        Building buildingComponent = null;
        switch (currentSelectedObject.buildID)
        {
            case BuildID.Floor:
                buildingComponent = newObject.AddComponent<Floor>();
                break;
            case BuildID.Wall:
                    buildingComponent = newObject.AddComponent<Wall>();
                break;
        }

        // Initialize the building component
        buildingComponent.Initialize(currentSelectedObject);

        // Update grid data for all cells this building occupies
        for (int x = 0; x < currentSelectedObject.gridSize.x; x++)
        {
            for (int y = 0; y < currentSelectedObject.gridSize.y; y++)
            {
                Vector2Int cellPos = gridPosition + new Vector2Int(x, y);
                gridData[cellPos.x, cellPos.y] = buildingComponent;
                spawnedObjects[cellPos.x, cellPos.y] = newObject.GetComponent<Building>();
            }
        }

        Debug.Log($"Successfully built {currentSelectedObject.buildID} at {gridPosition}");
        return true;
    }

    public bool TryRemoveObject(Vector2Int gridPosition)
    {
        if (!IsValidGridPosition(gridPosition)) return false;

        Building buildingToRemove = gridData[gridPosition.x, gridPosition.y];
        if (buildingToRemove == null) return false;

        Building objectToDestroy = spawnedObjects[gridPosition.x, gridPosition.y];

        // Find all cells occupied by this building and clear them
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (spawnedObjects[x, y] == objectToDestroy)
                {
                    gridData[x, y] = null;
                    spawnedObjects[x, y] = null;
                }
            }
        }

        // Destroy the game object
        if (objectToDestroy != null)
        {
            Destroy(objectToDestroy);
        }

        Debug.Log($"Removed building at {gridPosition}");
        return true;
    }

    private bool CanPlaceOnCell(Vector2Int gridPosition, BuildDataSO buildData, Building existingBuilding)
    {
        switch (buildData.buildID)
        {
            case BuildID.Floor:
                
                return existingBuilding == null;

            case BuildID.Wall:
                // Walls can only be placed on floors
                return existingBuilding != null && existingBuilding is Floor;

            default:
                // Default behavior: can only place on empty cells
                return existingBuilding == null;
        }
    }

    bool CanBuildAt(Vector2Int gridPosition, BuildDataSO buildDataSo)
    {
   
        return true;
    }

    bool IsValidGridPosition(Vector2Int gridPosition)
    {
        return gridPosition.x >= 0 && gridPosition.x < gridWidth &&
               gridPosition.y >= 0 && gridPosition.y < gridHeight;
    }

    Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        Vector3 localPosition = worldPosition - gridOrigin;
        int x = Mathf.FloorToInt(localPosition.x / cellSize);
        int z = Mathf.FloorToInt(localPosition.z / cellSize);
        return new Vector2Int(x, z);
    }

    Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        return gridOrigin + new Vector3(gridPosition.x * cellSize, 0, gridPosition.y * cellSize);
    }

    // Helper methods for querying the grid
    public Building GetBuildingAt(Vector2Int gridPosition)
    {
        if (!IsValidGridPosition(gridPosition)) return null;
        return gridData[gridPosition.x, gridPosition.y];
    }
}