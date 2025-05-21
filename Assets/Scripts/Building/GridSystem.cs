using UnityEngine;

public class GridSystem : MonoBehaviour
{
    #region Singleton

    public static GridSystem Instance { get; private set; }

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

    #endregion

    #region Properties and Fields

    [Header("Grid Settings")] public int gridWidth = 50;
    public int gridHeight = 50;
    public float cellSize = 1f;
    public Vector3 gridOrigin = Vector3.zero;

    [Header("Gizmos")] public bool showGrid = true;
    public Color gridColor = Color.white;
    public Color occupiedCellColor = Color.red;
    public Color unoccupiedCellColor = Color.green;
    [Header("Building")] public BuildDataSO currentSelectedObject;
    public LayerMask groundLayer = 1;
    public LayerMask buildingLayerMask = 1;
    public Building currentBuildingPlaced;

    private Building previewBuilding;
    private Vector2Int currentPreviewPosition;
    private bool isPlacing = false;
    private bool isDragging = false;
    public WallDirection previousDirection;
    private bool[,] gridData;
    //cached floor data
    public Floor[] floorData;
    private UIBuildingComfirm uiBuildingConfirm;
    private Camera playerCamera;

    #endregion

    #region Initialization

    void Start()
    {
        playerCamera = Camera.main;
        InitializeGrid();
        uiBuildingConfirm = FindObjectOfType<UIBuildingComfirm>();
    }

    void InitializeGrid()
    {
        gridData = new bool[gridWidth, gridHeight];
        floorData = new Floor[gridWidth * gridHeight];
    }

    #endregion

    #region Main Update Loop

    void Update()
    {
        if (isPlacing && previewBuilding != null)
        {
            HandlePlacementMode();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isPlacing)
        {
            CancelBuilding();
        }

        if (Input.GetMouseButtonDown(0))
        {
            //world pos to grid pos check is have floor
            Vector3 mousePos = Input.mousePosition;
            Ray ray = playerCamera.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
               //if hit buildingLayerMask 
                if (Physics.Raycast(ray, out RaycastHit hitBuilding, Mathf.Infinity, buildingLayerMask))
                {
                    Vector2Int gridPosition = WorldToGridPosition(hitBuilding.point);
                    Building building = hitBuilding.collider.GetComponent<Building>();
                    if (building is Floor)
                    {
                        //debug this floor is on which grid
                        Debug.Log($"Floor found at {gridPosition}");
                        
                    }
                }
            }
        }
    }

    private void HandlePlacementMode()
    {
        // Handle dragging with mouse click
        if (Input.GetMouseButtonDown(0) && !Utilities.IsPointerOverUI())
        {
            isDragging = true;
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
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

    #endregion

    #region Building Placement

    /// <summary>
    /// Begins the building placement process for a selected building type
    /// </summary>
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

    /// <summary>
    /// Places the building at the current preview position
    /// </summary>
    public void PlaceBuilding()
    {
        if (previewBuilding != null && isPlacing)
        {
            // Try to build at current preview position
            bool success = TryBuildObject(currentPreviewPosition);
            Destroy(previewBuilding.gameObject);

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
                    Debug.Log("No valid adjacent position found for the next building.");
                    // If no valid adjacent position, cancel placing
                    CancelBuilding();
                }
            }
            else
            {
                CancelBuilding();
            }
        }
    }

    /// <summary>
    /// Cancels the current building placement operation
    /// </summary>
    public void CancelBuilding()
    {
        if (previewBuilding != null)
        {
            Destroy(previewBuilding.gameObject);
            previewBuilding = null;
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
        switch (currentSelectedObject.buildID)
        {
            case BuildID.Floor:
                if (!CanPlaceObject(gridPosition, currentSelectedObject.gridSize)) return false;
                MarkGridCells(gridPosition, currentSelectedObject.gridSize, true);
                PlaceBuilding(gridPosition);
                break;
            case BuildID.Wall:
                WallDirection wallDirection = CalculateWallDirection(previewBuilding.transform.position,
                    GetGridCenterPosition(gridPosition));
                PlaceWall(gridPosition, wallDirection);
                break;

            default:
                PlaceBuilding(gridPosition);
                break;
        }

        return true;
    }

    private bool CanPlaceObject(Vector2Int gridPosition, Vector2Int gridSize)
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector2Int checkPos = gridPosition + new Vector2Int(x, y);
                if (!IsValidGridPosition(checkPos) || gridData[checkPos.x, checkPos.y])
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void MarkGridCells(Vector2Int gridPosition, Vector2Int gridSize, bool isOccupied)
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector2Int cellPos = gridPosition + new Vector2Int(x, y);
                gridData[cellPos.x, cellPos.y] = isOccupied;
               
            }
        }
    }
    
    private Floor GetFloorAt(Vector2Int gridPosition)   
    {
        return floorData[gridPosition.x + gridPosition.y * gridWidth];
    }
  

    private void PlaceBuilding(Vector2Int gridPosition)
    {
        Vector3 worldPosition = GridToWorldPosition(gridPosition);
        Building newObject = Instantiate(currentSelectedObject.prefab, worldPosition, Quaternion.identity);
        if (newObject is Floor)
        {
            Debug.Log($"Placing floor at {gridPosition} aaaaa");
            floorData[currentSelectedObject.gridSize.x + currentSelectedObject.gridSize.y * gridWidth] = newObject as Floor;
        }

        currentBuildingPlaced = newObject;
    }

    private void PlaceWall(Vector2Int gridPosition, WallDirection wallDirection)
    {
        Vector3 wallPosition = GridToWorldPosition(gridPosition);
        Building wallObject = Instantiate(currentSelectedObject.prefab, wallPosition, Quaternion.identity);
        UpdatePreviewPositionAndRotation(wallDirection, wallObject);
    }

    #endregion

    #region Preview Management

    private void CreatePreview(Vector2Int gridPosition)
    {
        if (previewBuilding != null)
        {
            Destroy(previewBuilding);
        }

        currentPreviewPosition = gridPosition;
        previewBuilding = Instantiate(currentSelectedObject.prefab);
        UpdatePreviewTransform();
    }

    private void UpdatePreviewTransform()
    {
        if (previewBuilding != null)
        {
            previewBuilding.transform.position = GridToWorldPosition(currentPreviewPosition);
        }
    }

    private void UpdatePreviewPosition()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            Vector2Int gridPosition = WorldToGridPosition(hit.point);
            // Floor f = GetFloorAt(gridPosition);
            // Debug.Log($"Grid Position: {gridPosition}");
            if (currentSelectedObject.buildID == BuildID.Floor)
            {
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
            else if (currentSelectedObject.buildID == BuildID.Wall)
            {
                WallDirection wallDirection = CalculateWallDirection(hit.point, GetGridCenterPosition(gridPosition));
                if (previousDirection == wallDirection) return;
                previousDirection = wallDirection;
                currentPreviewPosition = gridPosition;
                UpdatePreviewPositionAndRotation(wallDirection, previewBuilding);
                // if (f != null)
                // {
                //     Debug.Log($"Floor found at {gridPosition}");
                //    
                //     
                // }
            }
            else
            {
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
    }

    private void UpdatePreviewPositionAndRotation(WallDirection dir, Building preview)
    {
        float rotation = 0f;
        Vector3 offset = Vector3.zero;
        switch (dir)
        {
            case WallDirection.North:
                rotation = 90f;
                offset = new Vector3(cellSize / 2, 0, cellSize);
                break;
            case WallDirection.East:
                rotation = 0;
                offset = new Vector3(cellSize, 0, cellSize / 2);
                break;
            case WallDirection.South:
                rotation = 90f;
                offset = new Vector3(cellSize / 2, 0, 0);
                break;
            case WallDirection.West:
                rotation = 0;
                offset = new Vector3(0, 0, cellSize / 2);
                break;
        }

        preview.transform.position = GridToWorldPosition(currentPreviewPosition) + offset;
        preview.transform.rotation = Quaternion.Euler(0, rotation, 0);
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Returns the closest grid position to the player character
    /// </summary>
    public Vector2Int GetClosestGridPositionFromCharacter()
    {
        Vector3 characterPos = PlayerControl.Instance.transform.position;
        Vector2Int gridPosition = WorldToGridPosition(characterPos);
        return new Vector2Int(Mathf.Clamp(gridPosition.x, 0, gridWidth - 1),
            Mathf.Clamp(gridPosition.y, 0, gridHeight - 1));
    }

    /// <summary>
    /// Toggles the visibility of the grid
    /// </summary>
    public void ToggleGrid()
    {
        showGrid = !showGrid;
        Debug.Log($"Grid visibility: {(showGrid ? "Shown" : "Hidden")}");
    }

    /// <summary>
    /// Converts a world position to a grid position
    /// </summary>
    Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        Vector3 localPosition = worldPosition - gridOrigin;
        int x = Mathf.FloorToInt(localPosition.x / cellSize);
        int z = Mathf.FloorToInt(localPosition.z / cellSize);
        return new Vector2Int(x, z);
    }

    /// <summary>
    /// Converts a grid position to a world position
    /// </summary>
    Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        Vector2Int size = currentSelectedObject.gridSize;
        return gridOrigin + new Vector3(gridPosition.x * cellSize, 0, gridPosition.y * cellSize);
    }

    /// <summary>
    /// Gets the center position of a grid cell
    /// </summary>
    public Vector3 GetGridCenterPosition(Vector2Int gridPosition)
    {
        return gridOrigin + new Vector3(gridPosition.x * cellSize + cellSize / 2, 0,
            gridPosition.y * cellSize + cellSize / 2);
    }

    private WallDirection CalculateWallDirection(Vector3 hitPoint, Vector3 floorCenter)
    {
        Vector2 directionVector = new Vector2(hitPoint.x - floorCenter.x, hitPoint.z - floorCenter.z).normalized;

        float angle = Mathf.Atan2(directionVector.y, directionVector.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        if (angle >= 45f && angle < 135f) return WallDirection.North;
        if (angle >= 135f && angle < 225f) return WallDirection.West;
        if (angle >= 225f && angle < 315f) return WallDirection.South;
        return WallDirection.East;
    }

    private bool IsValidGridPosition(Vector2Int gridPosition)
    {
        return gridPosition.x >= 0 && gridPosition.x < gridWidth &&
               gridPosition.y >= 0 && gridPosition.y < gridHeight;
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
        for (int x = 0; x < currentSelectedObject.gridSize.x; x++)
        {
            for (int y = 0; y < currentSelectedObject.gridSize.y; y++)
            {
                Vector2Int checkPos = position + new Vector2Int(x, y);
                if (!IsValidGridPosition(checkPos) || gridData[checkPos.x, checkPos.y])
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

    #endregion

    #region GUI and Gizmos

    void OnGUI()
    {
        // Create a button in the top right corner to toggle grid visibility
        if (GUI.Button(new Rect(Screen.width - 120, 10, 110, 30), showGrid ? "Hide Grid" : "Show Grid"))
        {
            ToggleGrid();
        }
    }

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
            for (int x = 0; x < gridWidth; x++)
            {
                for (int z = 0; z < gridHeight; z++)
                {
                    Vector3 cellCenter = gridOrigin + new Vector3((x + 0.5f) * cellSize, 0.02f, (z + 0.5f) * cellSize);

                    if (gridData[x, z])
                    {
                        Gizmos.color = occupiedCellColor; // Color for occupied cells
                        Gizmos.DrawCube(cellCenter, new Vector3(cellSize * 0.9f, 0.01f, cellSize * 0.9f));
                    }
                    else
                    {
                        Gizmos.color = unoccupiedCellColor; // Color for unoccupied cells
                        Gizmos.DrawCube(cellCenter, new Vector3(cellSize * 0.9f, 0.01f, cellSize * 0.9f));
                    }
                }
            }
        }
    }

    #endregion
}