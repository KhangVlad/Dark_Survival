using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    #region Singleton
    public static BuildingManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    #endregion

    [Header("Building")] 
    public BuildDataSO currentSelectedObject;
    public LayerMask groundLayer = 1;
    public LayerMask buildingLayerMask = 1;
    [SerializeField] private GameObject gridGameObject;

    private GameObject _previewBuilding;
    private Vector2Int currentPreviewPosition;
    private bool isPlacing = false;
    private bool isDragging = false;
    private bool isEditMode = false;
    public Direction previousDirection;
    private Floor currentHitFloor;
    private UIBuildingComfirm uiBuildingConfirm;
    private Camera playerCamera;

    public event Action OnDone;

    private void Start()
    {
        playerCamera = Camera.main;
        uiBuildingConfirm = FindObjectOfType<UIBuildingComfirm>();
        UIBuilding.Instance.SwitchEditMode += SwitchEditModeHandler;
    }

    private void OnDestroy()
    {
        UIBuilding.Instance.SwitchEditMode -= SwitchEditModeHandler;
    }

    private void SwitchEditModeHandler(bool isEditMode)
    {
        this.isEditMode = isEditMode;
        SetGridMaterialValue(isEditMode ? 1 : 0);
    }

    private void Update()
    {
        if (Utilities.IsPointerOverUIElement()) return;

        if (isEditMode)
        {
            HandleEditMode();
        }
        else
        {
            HandleBuildingMode();
        }
    }

    private void HandleEditMode()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            Ray ray = playerCamera.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, buildingLayerMask))
            {
                if (hit.collider.TryGetComponent(out BuildBehaviour editBuilding))
                {
                    editBuilding.DeleteBuilding();
                    Debug.Log("Clicked on building: " + editBuilding.name);
                }
                else
                {
                    Debug.Log("Clicked on non-building object: " + hit.collider.name);
                }
            }
        }
    }

    private void HandleBuildingMode()
    {
        if (isPlacing && _previewBuilding != null)
        {
            HandlePlacementMode();
        }

        if (Input.GetMouseButton(0))
        {
            if (isDragging)
            {
                if (IsDraggingWall() || IsDraggingDoor())
                {
                    HandleWallOrDoorDragging(currentSelectedObject.buildID);
                }
                else if (IsDraggingFloor())
                {
                    HandleFloorDragging();
                }
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandleBuildingInteraction();
        }
    }

    private void HandleBuildingInteraction()
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = playerCamera.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, buildingLayerMask))
        {
            if (hit.collider.TryGetComponent(out BuildBehaviour editBuilding))
            {
                Debug.Log("Clicked on building: " + editBuilding.name);
                editBuilding.InteractWithBuilding();
            }
        }
    }

    #region Building Preview and Placement

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
            case BuildID.Door:
                HandleCreateWallLikePreview(buildData.buildID);
                break;
        }

        isPlacing = true;
    }

    private void HandlePlacementMode()
    {
        if (Input.GetMouseButtonDown(0) && !Utilities.IsPointerOverUIElement())
        {
            isDragging = true;
            uiBuildingConfirm.ActiveCanvas(false);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            if (uiBuildingConfirm != null)
            {
                uiBuildingConfirm.ActiveCanvas(true);
                uiBuildingConfirm.UpdatePosition(GridSystem.Instance.GridToWorldPosition(currentPreviewPosition));
            }
        }
    }

    private void HandleWallOrDoorDragging(BuildID buildID)
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = playerCamera.ScreenPointToRay(mousePos);

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            return;

        Vector2Int gridPosition = GridSystem.Instance.WorldToGridPosition(hit.point);

        // Check if grid position is valid
        if (!GridSystem.Instance.IsValidGridPosition(gridPosition))
            return;

        // Only continue if there is a floor at this position
        if (!GridSystem.Instance.IsCellOccupied(gridPosition))
            return;

        // Get floor at this position
        currentHitFloor = GetFloorAtGridPosition(gridPosition);
        if (currentHitFloor == null)
            return;

        currentPreviewPosition = gridPosition;

        // Calculate which wall direction based on hit point
        Direction direction = GridSystemExtension.CalculateWallDirection(
            hit.point,
            GridSystem.Instance.GetGridCenterPosition(gridPosition)
        );

        Floor f = GetFloorConnectWith(currentHitFloor, direction);
        if (currentHitFloor.IsDirectionCovered(direction)) return;
        
        // if (f != null && f.IsHaveWallAtDirection(BuildingExtension.GetOppositeWallDirection(direction))) return;
        //
        // previousDirection = direction;
        // UpdatePreviewPositionAndRotation(direction, _previewBuilding);
        //
        // // Update UI position
        // if (uiBuildingConfirm != null && uiBuildingConfirm.gameObject.activeSelf)
        // {
        //     uiBuildingConfirm.UpdatePosition(GridSystem.Instance.GridToWorldPosition(gridPosition));
        // }
        
        //f null is fine 
        previousDirection = direction;
        if (f != null)
        {
            
            if (f.IsHaveWallAtDirection(BuildingExtension.GetOppositeWallDirection(direction)))
            {
                return;
            }
        }
        
        UpdatePreviewPositionAndRotation(direction, _previewBuilding);
        uiBuildingConfirm.UpdatePosition(GridSystem.Instance.GridToWorldPosition(gridPosition));
    }

    private Floor GetFloorAtGridPosition(Vector2Int gridPosition)
    {
        if (!GridSystem.Instance.IsValidGridPosition(gridPosition))
        {
            return null;
        }

        return GridSystem.Instance.GetFloorAt(gridPosition, out var index);
    }

    private void HandleFloorDragging()
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = playerCamera.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            Vector2Int gridPosition = GridSystem.Instance.WorldToGridPosition(hit.point);
            if (GridSystem.Instance.IsCellOccupied(gridPosition)) return;

            if (gridPosition != currentPreviewPosition)
            {
                currentPreviewPosition = gridPosition;
                UpdatePreviewTransform();

                if (uiBuildingConfirm != null && uiBuildingConfirm.gameObject.activeSelf)
                {
                    uiBuildingConfirm.UpdatePosition(GridSystem.Instance.GridToWorldPosition(gridPosition));
                }
            }
        }
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

    private void HandleCreateWallLikePreview(BuildID buildID)
    {
        Floor validFloor = FindValidFloor(out var dir);

        if (validFloor == null)
        {
            CancelBuilding();
        }
        else
        {
            currentHitFloor = validFloor;
            Vector2Int gridPosition = validFloor.gridPos;
            currentPreviewPosition = gridPosition;
            _previewBuilding = Instantiate(currentSelectedObject.prefab);
            previousDirection = dir;
            UpdatePreviewPositionAndRotation(previousDirection, _previewBuilding);
            uiBuildingConfirm.UpdatePosition(GridSystem.Instance.GridToWorldPosition(gridPosition));
            uiBuildingConfirm.ActiveCanvas(true);
        }
    }

    #endregion

    #region Building Placement and Destruction

    public void PlaceBuilding()
    {
        if (_previewBuilding != null && isPlacing)
        {
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
                    case BuildID.Door:
                        HandleCreateWallLikePreview(currentSelectedObject.buildID);
                        break;
                }
            }
            else
            {
                CancelBuilding();
            }
        }
    }

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

        OnDone?.Invoke();
        CanvasController.Instance.SetActiveGameplayCanvas(true);
    }

    private bool TryBuildObject(Vector2Int gridPosition)
    {
        if (!GridSystem.Instance.IsValidGridPosition(gridPosition))
            return false;

        switch (currentSelectedObject.buildID)
        {
            case BuildID.Floor:
                if (!GridSystem.Instance.CanPlaceObject(gridPosition))
                    return false;
                GridSystem.Instance.MarkGridCells(gridPosition, currentSelectedObject.gridSize, true);
                PlaceFloor(gridPosition);
                break;
            case BuildID.Wall:
            case BuildID.Door:
                Direction direction = GridSystemExtension.CalculateWallDirection(
                    _previewBuilding.transform.position,
                    GridSystem.Instance.GetGridCenterPosition(currentPreviewPosition)
                );
                PlaceWallLikeObject(gridPosition, direction, currentSelectedObject.buildID);
                break;
        }

        return true;
    }

    private void PlaceWallLikeObject(Vector2Int gridPosition, Direction direction, BuildID buildID)
    {
        if (currentHitFloor == null) return;
        
        Floor floor = GridSystem.Instance.GetFloorAt(gridPosition, out var index);
        if (floor.IsDirectionCovered(direction)) return;
        
        Floor adjacentFloor = GetFloorConnectWith(floor, direction);
        Direction oppositeDirection = BuildingExtension.GetOppositeWallDirection(direction);
        if (adjacentFloor != null && adjacentFloor.IsHaveWallAtDirection(oppositeDirection)) return;
        
        Vector3 objectPosition = GridSystem.Instance.GridToWorldPosition(gridPosition);
        GameObject newObject = Instantiate(currentSelectedObject.prefab, objectPosition, Quaternion.identity);
        UpdatePreviewPositionAndRotation(direction, newObject);
        
        GridSystem.Instance.SetWallWithDirection(gridPosition, direction, newObject, buildID);
        
        if (buildID == BuildID.Door)
        {
            DoorBehaviour door = EditBuildingFactory.AddEditScripts(BuildID.Door, newObject) as DoorBehaviour;
            door.Init(BuildingExtension.GetWallDirectionIndex(direction), index, buildID, gridPosition);
        }
    }

    private void PlaceFloor(Vector2Int gridPosition)
    {
        Vector3 worldPosition = GridSystem.Instance.GridToWorldPosition(gridPosition);
        GameObject g = Instantiate(currentSelectedObject.prefab, worldPosition, Quaternion.identity);
        Floor f = new Floor(BuildID.Floor);
        f.gridPos = gridPosition;

        GridSystem.Instance.SetFloorAt(gridPosition, f, g);
    }

    #endregion

    #region Helper Methods

    private bool IsDraggingWall()
    {
        return isDragging && currentSelectedObject.buildID == BuildID.Wall;
    }

    private bool IsDraggingFloor()
    {
        return isDragging && currentSelectedObject.buildID == BuildID.Floor;
    }

    private bool IsDraggingDoor()
    {
        return isDragging && currentSelectedObject.buildID == BuildID.Door;
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

    private Floor FindValidFloor(out Direction validDir)
    {
        validDir = Direction.None;

        for (int x = 0; x < GridSystem.Instance.GridWidth; x++)
        {
            for (int y = 0; y < GridSystem.Instance.GridHeight; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Floor floor = GridSystem.Instance.GetFloorAt(pos, out var index);

                if (floor.buildID != BuildID.None && floor.IsWallAvailable())
                {
                    // Check all available directions for this floor
                    foreach (Direction dir in Enum.GetValues(typeof(Direction)))
                    {
                        if (dir == Direction.None || dir == Direction.All)
                            continue;
                        if (floor.IsDirectionCovered(dir))
                            continue;
                        Floor adjacentFloor = GetFloorConnectWith(floor, dir);
                        Direction oppositeDir = BuildingExtension.GetOppositeWallDirection(dir);

                        if (adjacentFloor == null || !adjacentFloor.IsHaveWallAtDirection(oppositeDir))
                        {
                            validDir = dir;
                            return floor;
                        }
                    }
                }
            }
        }

        return null;
    }

    // Cache frequently used vector allocations
    private static readonly Vector2Int[] DirectionOffsets =
    {
        new Vector2Int(0, 1),  // Top
        new Vector2Int(1, 0),  // Right
        new Vector2Int(0, -1), // Bot
        new Vector2Int(-1, 0)  // Left
    };

    private Floor GetFloorConnectWith(Floor f, Direction d)
    {
        if (f == null) return null;

        Vector2Int gridPosition = f.gridPos;

        // Get direction index (0=Top, 1=Right, 2=Bot, 3=Left)
        int dirIndex = -1;
        if (d == Direction.Top) dirIndex = 0;
        else if (d == Direction.Right) dirIndex = 1;
        else if (d == Direction.Bot) dirIndex = 2;
        else if (d == Direction.Left) dirIndex = 3;

        if (dirIndex == -1) return null;

        Vector2Int checkPos = gridPosition + DirectionOffsets[dirIndex];

        if (!GridSystemExtension.IsValidGridPosition(checkPos, GridSystem.Instance.GridWidth,
                GridSystem.Instance.GridHeight))
        {
            return null;
        }

        return GridSystem.Instance.GetFloorAt(checkPos, out _);
    }

    private Floor FindClosestAvailableFloorFromCharacter8Dir(out Vector2Int randomFreePos)
    {
        Vector3 playerPos = PlayerControl.Instance.transform.position;
        randomFreePos = Vector2Int.zero;
        List<Floor> availableFloors = new List<Floor>();

        // Collect all floors from GridSystem
        for (int x = 0; x < GridSystem.Instance.GridWidth; x++)
        {
            for (int y = 0; y < GridSystem.Instance.GridHeight; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Floor floor = GridSystem.Instance.GetFloorAt(pos, out _);
                if (floor != null)
                {
                    availableFloors.Add(floor);
                }
            }
        }

        Floor closestFloor = null;
        float closestDistance = Mathf.Infinity;

        foreach (Floor floor in availableFloors)
        {
            float distance = Vector3.Distance(playerPos, GridSystem.Instance.GridPosToWorldPosition(floor.gridPos));
            if (distance < closestDistance)
            {
                Vector2Int gridPosition = 
                    GridSystem.Instance.WorldToGridPosition(GridSystem.Instance.GridPosToWorldPosition(floor.gridPos));

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

            if (GridSystem.Instance.IsValidGridPosition(adjacentPos)
                && !GridSystem.Instance.IsCellOccupied(adjacentPos))
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

    private Vector2Int GetClosestGridPositionFromCharacter()
    {
        Vector3 characterPos = PlayerControl.Instance.transform.position;
        Vector2Int gridPosition = GridSystem.Instance.WorldToGridPosition(characterPos);
        return new Vector2Int(Mathf.Clamp(gridPosition.x, 0, GridSystem.Instance.GridWidth - 1),
            Mathf.Clamp(gridPosition.y, 0, GridSystem.Instance.GridHeight - 1));
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
        uiBuildingConfirm.UpdatePosition(GridSystem.Instance.GridToWorldPosition(currentPreviewPosition));
    }

    private void UpdatePreviewTransform()
    {
        if (_previewBuilding != null)
        {
            _previewBuilding.transform.position = GridSystem.Instance.GridToWorldPosition(currentPreviewPosition);
        }
    }

    private void UpdatePreviewPositionAndRotation(Direction dir, GameObject preview)
    {
        Vector3 offset = CalculateOffsetAndRotate(dir, out float rotation);
        preview.transform.position = GridSystem.Instance.GridToWorldPosition(currentPreviewPosition) + offset;
        preview.transform.rotation = Quaternion.Euler(0, rotation, 0);
    }

    private Vector3 CalculateOffsetAndRotate(Direction dir, out float rotate)
    {
        float rotation = 0f;
        Vector3 offset = Vector3.zero;
        float cellSize = GridSystem.Instance.CellSize;

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
}