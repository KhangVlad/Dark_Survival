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

    private Building[,] gridData;
    private Building[,] spawnedObjects;

    [Header("Preview")] 
    private GameObject previewObject;
    private Vector2Int currentPreviewPosition;
    private bool isPlacing = false;
    private bool isDragging = false;
    
    private UIBuildingComfirm uiBuildingConfirm;
    
    public Building currentBuildingPlaced;

    private Camera playerCamera;
    public Color previewColor = Color.yellow;
    
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
        uiBuildingConfirm = FindObjectOfType<UIBuildingComfirm>();
        
    }


    void InitializeGrid()
    {
        gridData = new Building[gridWidth, gridHeight];
        spawnedObjects = new Building[gridWidth, gridHeight];
    }

    void Update()
    {
        if (isPlacing && previewObject != null)
        {
            // Handle dragging with mouse click
            if (Input.GetMouseButtonDown(0) && !Utilities.IsPointerOverUI())
            {
                isDragging = true;
            }
            
            if (Input.GetMouseButtonUp(0) && isDragging)
            {
                isDragging = false;
                // Show confirmation UI at the current preview position
                if (uiBuildingConfirm != null)
                {
                    uiBuildingConfirm.ActiveCanvas(true);
                    uiBuildingConfirm.UpdatePosition(GridToWorldPosition(currentPreviewPosition));
                }
            }
            
            if (isDragging || !uiBuildingConfirm.gameObject.activeSelf)
            {
                UpdatePreviewPosition();
            }
        }
        else
        {
            HandleInput();
        }
        
        if (Input.GetKeyDown(KeyCode.Escape) && isPlacing)
        {
            CancelBuilding();
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            //clear grid data
            for (int x = 0; x < gridWidth; x++)
            {
                for (int z = 0; z < gridHeight; z++)
                {
                    TryRemoveObject(new Vector2Int(x, z));
                }
            }
        }
    }
    
    public Vector2Int GetClosestGridPositionFromCharacter()
    {
        Vector3 characterPos = PlayerControl.Instance.transform.position;
        Vector2Int gridPosition = WorldToGridPosition(characterPos);
        return new Vector2Int(Mathf.Clamp(gridPosition.x, 0, gridWidth - 1),
            Mathf.Clamp(gridPosition.y, 0, gridHeight - 1));
    }
    
    // Add a method to toggle grid visibility
    public void ToggleGrid()
    {
        showGrid = !showGrid;
        Debug.Log($"Grid visibility: {(showGrid ? "Shown" : "Hidden")}");
    }

    // Simple UI to toggle grid visibility
    void OnGUI()
    {
        // Create a button in the top right corner to toggle grid visibility
        if (GUI.Button(new Rect(Screen.width - 120, 10, 110, 30), showGrid ? "Hide Grid" : "Show Grid"))
        {
            ToggleGrid();
        }
    }

    // Draw grid gizmos when enabled
    void OnDrawGizmos()
    {
        if (!showGrid) return;

        // Only draw in editor or if application is playing and showGrid is true
        if (!Application.isPlaying && !showGrid) return;

        Gizmos.color = gridColor;

        // Draw horizontal lines
        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 startPos = gridOrigin + new Vector3(x * cellSize, 0.01f, 0);
            Vector3 endPos = gridOrigin + new Vector3(x * cellSize, 0.01f, gridHeight * cellSize);
            Gizmos.DrawLine(startPos, endPos);
        }

        // Draw vertical lines
        for (int z = 0; z <= gridHeight; z++)
        {
            Vector3 startPos = gridOrigin + new Vector3(0, 0.01f, z * cellSize);
            Vector3 endPos = gridOrigin + new Vector3(gridWidth * cellSize, 0.01f, z * cellSize);
            Gizmos.DrawLine(startPos, endPos);
        }

        // Optionally highlight occupied cells
        if (Application.isPlaying && gridData != null)
        {
            Gizmos.color = occupiedCellColor;
            for (int x = 0; x < gridWidth; x++)
            {
                for (int z = 0; z < gridHeight; z++)
                {
                    if (gridData[x, z] != null)
                    {
                        Vector3 cellCenter =
                            gridOrigin + new Vector3((x + 0.5f) * cellSize, 0.02f, (z + 0.5f) * cellSize);
                        Gizmos.DrawCube(cellCenter, new Vector3(cellSize * 0.9f, 0.01f, cellSize * 0.9f));
                    }
                }
            }
        }
    }

    void HandleInput()
    {
        if (currentSelectedObject == null) return;

        Vector3 mousePosition = Input.mousePosition;
        Ray ray = playerCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            Vector2Int gridPosition = WorldToGridPosition(hit.point);

            // if (Input.GetMouseButtonDown(0))
            // {
            //     TryBuildObject(gridPosition);
            // }

            // Optional: Add right-click to remove buildings
            if (Input.GetMouseButtonDown(1))
            {
                TryRemoveObject(gridPosition);
            }
        }
    }
    
    public void StartPlacingBuilding(BuildDataSO buildData)
    {
        currentSelectedObject = buildData;
        Vector2Int position = GetClosestGridPositionFromCharacter();
        
        // If this is a wall, try to find the closest floor
        if (buildData.buildID == BuildID.Wall)
        {
            position = FindClosestFloorPosition(position);
        }
        
        CreatePreview(position);
        isPlacing = true;
        isDragging = true; 
       
    }
    
    private Vector2Int FindClosestFloorPosition(Vector2Int startPosition)
    {
        // Search in increasing radius for a floor
        for (int radius = 0; radius < 5; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    // Only check positions at the current radius
                    if (Mathf.Abs(x) == radius || Mathf.Abs(y) == radius)
                    {
                        Vector2Int checkPos = startPosition + new Vector2Int(x, y);
                        
                        if (IsValidGridPosition(checkPos) && gridData[checkPos.x, checkPos.y] is Floor)
                        {
                            return checkPos;
                        }
                    }
                }
            }
        }
        
        // If no floor found, return the original position
        return startPosition;
    }
    
    private void UpdatePreviewPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = playerCamera.ScreenPointToRay(mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            Vector2Int gridPosition = WorldToGridPosition(hit.point);
            
            // If building type is wall, ensure it's on a floor
            if (currentSelectedObject.buildID == BuildID.Wall)
            {
                if (!IsValidGridPosition(gridPosition) || !(gridData[gridPosition.x, gridPosition.y] is Floor))
                {
                    // Find closest valid floor position
                    gridPosition = FindClosestFloorPosition(gridPosition);
                }
            }
            
            if (gridPosition != currentPreviewPosition)
            {
                currentPreviewPosition = gridPosition;
                UpdatePreviewTransform();
                
                // Update confirmation UI position if it's active
                if (uiBuildingConfirm != null && uiBuildingConfirm.gameObject.activeSelf)
                {
                    uiBuildingConfirm.UpdatePosition(GridToWorldPosition(gridPosition));
                }
            }
        }
    }
    
    private void CreatePreview(Vector2Int gridPosition)
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
        }
        
        currentPreviewPosition = gridPosition;
        previewObject = Instantiate(currentSelectedObject.prefab);
        
        // Make the preview semi-transparent
        SetPreviewTransparency(previewObject, 0.5f);
        
        UpdatePreviewTransform();
    }
    
    private void SetPreviewTransparency(GameObject obj, float alpha)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            foreach (Material material in materials)
            {
                Color color = material.color;
                material.color = new Color(color.r, color.g, color.b, alpha);
                
                // Enable transparency
                material.SetFloat("_Mode", 3); // Transparent mode
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
            }
        }
    }
    
    private void UpdatePreviewTransform()
    {
        if (previewObject != null)
        {
            previewObject.transform.position = GridToWorldPosition(currentPreviewPosition);
        }
    }
    
    public void PlaceBuilding()
    {
        if (previewObject != null && isPlacing)
        {
            // Try to build at current preview position
            bool success = TryBuildObject(currentPreviewPosition);
            Destroy(previewObject);
            
            if (success)
            {
                // Find a valid adjacent position for the next preview
                Vector2Int? nextPosition = FindValidAdjacentPosition(currentBuildingPlaced);
                if (nextPosition.HasValue)
                {
                    CreatePreview(nextPosition.Value);
                    UpdatePreviewTransform();
                    isPlacing = true;
                    
                    // Update the UI confirmation to the new position
                    if (uiBuildingConfirm != null)
                    {
                        uiBuildingConfirm.ActiveCanvas(true);
                        uiBuildingConfirm.UpdatePosition(GridToWorldPosition(nextPosition.Value));
                    }
                }
                else
                {
                    // If no valid adjacent position, cancel placing
                    CancelBuilding();
                }
            }
            else
            {
                Debug.Log("Failed to place building.");
            }
        }
    }

    private Vector2Int? FindValidAdjacentPosition(Building building)
    {
        if (building == null) return null;
        
        Vector2Int currentPosition = WorldToGridPosition(building.transform.position);
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        
        // Shuffle directions for more natural placement patterns
        ShuffleArray(directions);
        
        foreach (Vector2Int direction in directions)
        {
            Vector2Int adjacentPosition = currentPosition + direction;
            
            if (IsValidGridPosition(adjacentPosition) && IsValidBuildingPosition(adjacentPosition))
            {
                return adjacentPosition;
            }
        }
        
        return null;
    }

    private bool IsValidBuildingPosition(Vector2Int position)
    {
        if (currentSelectedObject == null) return false;
        
        // Check if all cells needed for the building are valid
        for (int x = 0; x < currentSelectedObject.gridSize.x; x++)
        {
            for (int y = 0; y < currentSelectedObject.gridSize.y; y++)
            {
                Vector2Int checkPos = position + new Vector2Int(x, y);
                
                if (!IsValidGridPosition(checkPos)) return false;
                
                Building existingBuilding = gridData[checkPos.x, checkPos.y];
                
                // Check building rules based on type
                if (!CanPlaceOnCell(checkPos, currentSelectedObject, existingBuilding))
                {
                    return false;
                }
            }
        }
        
        return true;
    }

    private void ShuffleArray<T>(T[] array)
    {
        // Fisher-Yates shuffle algorithm
        System.Random random = new System.Random();
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            T temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

    private Building TryGetGridNextTpCurrentPlacedBuilding()
    {
        // This method is no longer used but kept for backward compatibility
        Building nextTo = null;
        if (currentBuildingPlaced != null)
        {
            Vector2Int currentPosition = WorldToGridPosition(currentBuildingPlaced.transform.position);
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (Vector2Int direction in directions)
            {
                Vector2Int adjacentPosition = currentPosition + direction;
                if (IsValidGridPosition(adjacentPosition))
                {
                    Building building = gridData[adjacentPosition.x, adjacentPosition.y];
                    if (building != null && building != currentBuildingPlaced)
                    {
                        nextTo = building;
                        break;
                    }
                }
            }
        }
        return nextTo;
    }

    public void CancelBuilding()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }
        
        isPlacing = false;
        isDragging = false;
        
        if (uiBuildingConfirm != null)
        {
            uiBuildingConfirm.ActiveCanvas(false);
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
        buildingComponent.Initialize(currentSelectedObject);
        currentBuildingPlaced = buildingComponent;
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

