using System;
using System.Collections.Generic;
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

    private Building _previewBuilding;
    private Vector2Int currentPreviewPosition;
    public bool isPlacing = false;
    public bool isDragging = false;
    public bool IsEditMode = false;
    public Direction previousDirection;
    private bool[,] gridData;
    private Floor currentHitFloor;
    public Floor[] floors;
    private UIBuildingComfirm uiBuildingConfirm;
    private Camera playerCamera;
    [SerializeField] private GameObject gridGameObject;
// Add these events to the GridSystem class
    public event System.Action<Building> OnBuildingPlaced;
    public event System.Action<Building> OnBuildingRemoved;
    #endregion

    #region Initialization

    void Start()
    {
        playerCamera = Camera.main;
        InitializeGrid();
        uiBuildingConfirm = FindObjectOfType<UIBuildingComfirm>();
        UIBuilding.Instance.SwitchEditMode += SwitchEditModeHandler;
    }

    private void OnDestroy()
    {
        UIBuilding.Instance.SwitchEditMode -= SwitchEditModeHandler;
    }

    private void SwitchEditModeHandler(bool isEditMode)
    {
        IsEditMode = isEditMode;
        if (isEditMode)
        {
            SetGridMaterialValue(1);
            CanvasController.Instance.SetActiveGameplayCanvas(false);
        }
        else
        {
            SetGridMaterialValue(0);
            CanvasController.Instance.SetActiveGameplayCanvas(true);
        }
    }

    void InitializeGrid()
    {
        gridData = new bool[gridWidth, gridHeight];
        floors = new Floor[gridWidth * gridHeight];
    }

    #endregion

    #region Main Update Loop

    void Update()
    {
        if (IsEditMode)
        {
            // if (Input.GetMouseButtonDown(0) && !Utilities.IsPointerOverUIElement())
            // {
            //     Vector3 mousePos = Input.mousePosition;
            //     Ray ray = playerCamera.ScreenPointToRay(mousePos);
            //     if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, buildingLayerMask))
            //     {
            //         if (hit.collider.TryGetComponent(out Building b))
            //         {
            //             DestroyBuilding(b);
            //         }
            //     }
            // }
            
            //debug grid pos by mouse 
            if (Input.GetMouseButton(0))
            {
                //if cell is occupied by floor debug not use ray , use grid 
                Vector3 mousePos = Input.mousePosition;
                Ray ray = playerCamera.ScreenPointToRay(mousePos);
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
                {
                    Vector2Int gridPosition = WorldToGridPosition(hit.point);
                    if (IsValidGridPosition(gridPosition))
                    {
                        if (gridData[gridPosition.x, gridPosition.y])
                        {
                          Debug.Log("Delete Floor at    grid position: " + gridPosition);
                            int index = gridPosition.x + gridPosition.y * gridWidth;
                            if (index >= 0 && index < floors.Length && floors[index] != null)
                            {
                                DestroyBuilding(floors[index]);
                                floors[index] = null; 
                            }
                        }
                        
                    }
                }
            }
        }
        else
        {
            if (Utilities.IsPointerOverUIElement()) return;
            if (isPlacing && _previewBuilding != null)
            {
                HandlePlacementMode();
            }

            if (Input.GetMouseButton(0) && IsDraggingWall())
            {
                HandleWallDragging();
            }

            if (Input.GetMouseButton(0) && IsDraggingFloor())
            {
                HandleFloorDragging();
            }
        }
    }

    private void DestroyBuilding(Building b)
    {
        if (b is Floor floor)
        {
            if (floor.IsDestroyAble())
            {
                // Destroy floor and free grid cells
                Vector2Int gridPosition = WorldToGridPosition(floor.transform.position);
                Debug.Log($"Destroying floor at grid position: {gridPosition}");
                MarkGridCells(gridPosition, floor.buildingData.gridSize, false);
                int index = gridPosition.x + gridPosition.y * gridWidth;
                if (index >= 0 && index < floors.Length)
                {
                    floors[index] = null; // Remove the floor from the array
                }
                OnBuildingRemoved?.Invoke(b);
                Destroy(floor.gameObject);
            }
        }
        else if (b is Wall wall)
        {
            Destroy( wall.gameObject);
        }
    }

    private void HandleWallDragging()
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = playerCamera.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            if (Physics.Raycast(ray, out RaycastHit hitBuilding, Mathf.Infinity, buildingLayerMask))
            {
                Vector2Int gridPosition = WorldToGridPosition(hitBuilding.point);
                Building building = hitBuilding.collider.GetComponent<Building>();
                if (building is Floor)
                {
                    if (building != currentHitFloor)
                    {
                        currentHitFloor = building as Floor;
                    }

                    currentPreviewPosition = gridPosition;
                    Direction direction = GridSystemExtension.CalculateWallDirection(
                        hit.point,
                        GridSystemExtension.GetGridCenterPosition(gridPosition, gridOrigin, cellSize)
                    );

                    if (currentHitFloor.IsDirectionCovered(direction)) return;
                    previousDirection = direction;
                    UpdatePreviewPositionAndRotation(direction, _previewBuilding);
                }
            }
        }
    }

    private void HandleFloorDragging()
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = playerCamera.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            Vector2Int gridPosition = WorldToGridPosition(hit.point);
            if (IsCellOccupied(gridPosition, currentSelectedObject.gridSize)) return;

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

    private bool IsDraggingWall()
    {
        return isDragging && currentSelectedObject.buildID == BuildID.Wall;
    }

    private bool IsDraggingFloor()
    {
        return isDragging && currentSelectedObject.buildID == BuildID.Floor;
    }

    private void HandlePlacementMode()
    {
        if (Input.GetMouseButtonDown(0) && !Utilities.IsPointerOverUIElement())
        {
            isDragging = true;
            uiBuildingConfirm.ActiveCanvas(false);
        }

        // if (isDragging)
        // {
        //     uiBuildingConfirm.ActiveCanvas(false);
        // }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            if (uiBuildingConfirm != null)
            {
                uiBuildingConfirm.ActiveCanvas(true);
                uiBuildingConfirm.UpdatePosition(GridToWorldPosition(currentPreviewPosition));
            }
        }
    }

    #endregion

    #region Building Placement

    public void StartPlacingBuilding(BuildDataSO buildData)
    {
        SetGridMaterialValue(1);
        currentSelectedObject = buildData;

        switch (buildData.buildID)
        {
            case BuildID.Floor:
                HandleCreateFloorPreview();
                break;
            case BuildID.Wall:
                HandleCreateWallPreview();
                break;
        }

        isPlacing = true;
    }

    private void SetGridMaterialValue(float value)
    {
        if (gridGameObject != null)
        {
            Renderer renderer = gridGameObject.GetComponent<Renderer>();
            Material gridMaterial = renderer.material;
            gridMaterial.SetFloat("_ShowGrid", value);
        }
    }

    private void HandleCreateWallPreview()
    {
        Floor validFloor = FindValidFloor();
        currentHitFloor = validFloor;   
        if (validFloor == null)
        {
            CancelBuilding();
           
        }
        else
        {
            Vector2Int gridPosition = WorldToGridPosition(validFloor.transform.position);
            currentPreviewPosition = gridPosition;
            _previewBuilding = Instantiate(currentSelectedObject.prefab);
            previousDirection = validFloor.GetRandomNullDirection();
            UpdatePreviewPositionAndRotation(previousDirection, _previewBuilding);
            uiBuildingConfirm.UpdatePosition(GridToWorldPosition(gridPosition));
            uiBuildingConfirm.ActiveCanvas(true);
        }
    }

    private Floor FindValidFloor()
    {
        Floor validFloor = null;
        for (int i = 0; i < floors.Length; i++)
        {
            if (floors[i] != null && floors[i].IsWallAvailable())
            {
                validFloor = floors[i];
                if (validFloor.IsWallAvailable())
                {
                    break;
                }
            }
        }

        return validFloor;
    }


    private void HandleCreateFloorPreview()
    {
        Floor closestFloor = FindClosestAvailableFloorFromCharacter8Dir(out Vector2Int direction);
        if (closestFloor == null)
        {
            Vector2Int characterPos = GetClosestGridPositionFromCharacter();
            CreatePreview(characterPos);
        }
        else
        {
            CreatePreview(direction);
        }
    }

    private Floor FindClosestAvailableFloorFromCharacter8Dir(out Vector2Int randomFreePos)
    {
        Vector3 playerPos = PlayerControl.Instance.transform.position;
        randomFreePos = Vector2Int.zero;
        List<Floor> availableFloors = new List<Floor>();

        for (int i = 0; i < floors.Length; i++)
        {
            if (floors[i] != null)
            {
                availableFloors.Add(floors[i]);
            }
        }

        // Find closest floor with free adjacent positions
        Floor closestFloor = null;
        float closestDistance = Mathf.Infinity;

        foreach (Floor floor in availableFloors)
        {
            float distance = Vector3.Distance(playerPos, floor.transform.position);
            if (distance < closestDistance)
            {
                Vector2Int gridPosition = WorldToGridPosition(floor.transform.position);

                if (HasFreeAdjacent8Position(gridPosition, out Vector2Int direction))
                {
                    closestDistance = distance;
                    closestFloor = floor;
                    randomFreePos = direction;
                }
            }
        }

        return closestFloor;
    }


    private bool HasFreeAdjacent8Position(Vector2Int gridPosition, out Vector2Int randomFreePos)
    {
        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.up + Vector2Int.right,
            Vector2Int.right,
            Vector2Int.down + Vector2Int.right,
            Vector2Int.down,
            Vector2Int.down + Vector2Int.left,
            Vector2Int.left,
            Vector2Int.up + Vector2Int.left
        };

        List<Vector2Int> freePositions = new List<Vector2Int>();

        foreach (Vector2Int direction in directions)
        {
            Vector2Int adjacentPos = gridPosition + direction;

            if (IsValidGridPosition(adjacentPos) && !gridData[adjacentPos.x, adjacentPos.y])
            {
                freePositions.Add(adjacentPos);
            }
        }

        if (freePositions.Count > 0)
        {
            randomFreePos = freePositions[UnityEngine.Random.Range(0, freePositions.Count)];
            return true;
        }

        randomFreePos = default;
        return false;
    }


    public void PlaceBuilding()
    {
        if (_previewBuilding != null && isPlacing)
        {
            // Try to build at current preview position
            bool success = TryBuildObject(currentPreviewPosition);
            Destroy(_previewBuilding.gameObject);

            if (success)
            {
                switch (currentSelectedObject.buildID)
                {
                    case BuildID.Floor:
                        HandleCreateFloorPreview();
                        break;
                    case BuildID.Wall:
                        HandleCreateWallPreview();
                        break;
                }
            }
            else
            {
                
                CancelBuilding();
            }
        }
    }

    private Floor GetFloorConnectWith(Floor f, Direction d)
    {
        Vector2Int gridPosition = WorldToGridPosition(f.transform.position);
        Vector2Int checkPos = gridPosition;

        switch (d)
        {
            case Direction.Top:
                checkPos += Vector2Int.up;
                break;
            case Direction.Right:
                checkPos += Vector2Int.right;
                break;
            case Direction.Bot:
                checkPos += Vector2Int.down;
                break;
            case Direction.Left:
                checkPos += Vector2Int.left;
                break;
        }

        // Check if the position is valid before accessing the array
        if (!IsValidGridPosition(checkPos))
        {
            return null;
        }

        int index = checkPos.x + checkPos.y * gridWidth;

        // Verify index is within array bounds
        if (index < 0 || index >= floors.Length)
        {
            return null;
        }

        return floors[index];
    }

    /// <summary>
    /// Cancels the current building placement operation
    /// </summary>
    public void CancelBuilding()
    {
        SetGridMaterialValue(0);
        if (_previewBuilding != null)
        {
            Destroy(_previewBuilding.gameObject);
            _previewBuilding = null;
        }

        isPlacing = false;
        isDragging = false;

        if (uiBuildingConfirm != null)
        {
            uiBuildingConfirm.ActiveCanvas(false);
        }

        CanvasController.Instance.SetActiveGameplayCanvas(true);
    }

    public bool TryBuildObject(Vector2Int gridPosition)
    {
        if (!IsValidGridPosition(gridPosition)) return false;

        switch (currentSelectedObject.buildID)
        {
            case BuildID.Floor:
                if (!CanPlaceObject(gridPosition, currentSelectedObject.gridSize)) return false;
                MarkGridCells(gridPosition, currentSelectedObject.gridSize, true);
                PlaceFloor(gridPosition);
                break;
            case BuildID.Wall:
                Direction direction = GridSystemExtension.CalculateWallDirection(
                    _previewBuilding.transform.position,
                    GridSystemExtension.GetGridCenterPosition(currentPreviewPosition, gridOrigin, cellSize)
                );
                PlaceWall(gridPosition, direction);
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

    private bool IsCellOccupied(Vector2Int gridPosition, Vector2Int gridSize)
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector2Int checkPos = gridPosition + new Vector2Int(x, y);
                if (!IsValidGridPosition(checkPos) || gridData[checkPos.x, checkPos.y])
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void PlaceFloor(Vector2Int gridPosition)
    {
        Vector3 worldPosition = GridToWorldPosition(gridPosition);
        Building newObject = Instantiate(currentSelectedObject.prefab, worldPosition, Quaternion.identity);
        newObject.SetBuildingData(new BuildingData(currentSelectedObject));
        int index = gridPosition.x + gridPosition.y * gridWidth;
        floors[index] = newObject as Floor;
        //find 4 floor direction next to this grid
        Floor floor1 = GetFloorConnectWith(floors[index], Direction.Top);
        if (floor1 != null)
        {
            if (floor1.IsHaveWallAtDirection(Direction.Bot))
            {
                floors[index].SetWallWithDirection(Direction.Top, floor1.GetWallAtDirection(Direction.Bot));
            }
        }
        Floor floor2 = GetFloorConnectWith(floors[index], Direction.Right);
        if (floor2 != null)
        {
            if (floor2.IsHaveWallAtDirection(Direction.Left))
            {
                floors[index].SetWallWithDirection(Direction.Right, floor2.GetWallAtDirection(Direction.Left));
            }
        }
        Floor floor3 = GetFloorConnectWith(floors[index], Direction.Bot);
        if (floor3 != null)
        {
            if (floor3.IsHaveWallAtDirection(Direction.Top))
            {
                floors[index].SetWallWithDirection(Direction.Bot, floor3.GetWallAtDirection(Direction.Top));
            }
        }
        Floor floor4 = GetFloorConnectWith(floors[index], Direction.Left);
        if (floor4 != null)
        {
            if (floor4.IsHaveWallAtDirection(Direction.Right))
            {
                floors[index].SetWallWithDirection(Direction.Left, floor4.GetWallAtDirection(Direction.Right));
            }
        }
        OnBuildingPlaced?.Invoke(newObject);
    }

    private void PlaceWall(Vector2Int gridPosition, Direction direction)
    {
        if (currentHitFloor == null) return;
        Vector3 wallPosition = GridToWorldPosition(gridPosition);
        Building wallObject = Instantiate(currentSelectedObject.prefab, wallPosition, Quaternion.identity);
        wallObject.SetBuildingData(new BuildingData(currentSelectedObject));
        currentHitFloor.SetWallWithDirection(direction, wallObject as Wall);
        UpdatePreviewPositionAndRotation(direction, wallObject);

        //get connected floor at wall direction
        Floor connectedFloor = GetFloorConnectWith(currentHitFloor, direction);
        if (connectedFloor != null)
        {
            // Set the wall on the connected floor
            connectedFloor.SetWallWithDirection(BuildingExtension.GetOppositeWallDirection(direction),
                wallObject as Wall);
        }
    }

    #endregion

    #region Preview Management

    private void CreatePreview(Vector2Int gridPosition)
    {
        if (_previewBuilding != null)
        {
            Destroy(_previewBuilding);
        }

        currentPreviewPosition = gridPosition;
        _previewBuilding = Instantiate(currentSelectedObject.prefab);
        UpdatePreviewTransform();
        uiBuildingConfirm.ActiveCanvas(true);
        uiBuildingConfirm.UpdatePosition(GridToWorldPosition(currentPreviewPosition));
    }

    private void UpdatePreviewTransform()
    {
        if (_previewBuilding != null)
        {
            _previewBuilding.transform.position = GridToWorldPosition(currentPreviewPosition);
        }
    }

    private void UpdatePreviewPositionAndRotation(Direction dir, Building preview) //adjust postion on hit floor 
    {
        Vector3 offset = CalculateOffsetAndRotate(dir, out float rotation);
        preview.transform.position = GridToWorldPosition(currentPreviewPosition) + offset;
        preview.transform.rotation = Quaternion.Euler(0, rotation, 0);
    }

 

    private Vector3 CalculateOffsetAndRotate(Direction dir, out float rotate)
    {
        float rotation = 0f;
        Vector3 offset = Vector3.zero;
        switch (dir)
        {
            case Direction.Top:
                rotation = 90f;
                offset = new Vector3(cellSize / 2, 0, cellSize);
                break;
            case Direction.Right:
                rotation = 0;
                offset = new Vector3(cellSize, 0, cellSize / 2);
                break;
            case Direction.Bot:
                rotation = 90f;
                offset = new Vector3(cellSize / 2, 0, 0);
                break;
            case Direction.Left:
                rotation = 0;
                offset = new Vector3(0, 0, cellSize / 2);
                break;
        }

        rotate = rotation;
        return offset;
    }

    #endregion

    #region Utility Methods

    public Vector2Int GetClosestGridPositionFromCharacter()
    {
        Vector3 characterPos = PlayerControl.Instance.transform.position;
        Vector2Int gridPosition = WorldToGridPosition(characterPos);
        return new Vector2Int(Mathf.Clamp(gridPosition.x, 0, gridWidth - 1),
            Mathf.Clamp(gridPosition.y, 0, gridHeight - 1));
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        return GridSystemExtension.WorldToGridPosition(worldPosition, gridOrigin, cellSize);
    }

    Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        return GridSystemExtension.GridToWorldPosition(gridPosition, gridOrigin, cellSize);
    }

    private bool IsValidGridPosition(Vector2Int gridPosition)
    {
        return GridSystemExtension.IsValidGridPosition(gridPosition, gridWidth, gridHeight);
    }

    #endregion

    #region GUI and Gizmos

#if UNITY_EDITOR
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
#endif

    #endregion
}