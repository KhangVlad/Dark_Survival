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

    private GameObject _previewBuilding;
    private Vector2Int currentPreviewPosition;
    private bool isPlacing = false;
    private bool isDragging = false;
    private bool isEditMode = false;
    public Direction previousDirection;
    private Floor currentHitFloor;
    public Floor[] floors;
    private UIBuildingComfirm uiBuildingConfirm;
    private Camera playerCamera;
    [SerializeField] private GameObject gridGameObject;
    
    public event Action<GameObject,BuildID,Vector2Int> OnPlaceBuilding;

    private void Start()
    {
        playerCamera = Camera.main;
        uiBuildingConfirm = FindObjectOfType<UIBuildingComfirm>();
        UIBuilding.Instance.SwitchEditMode += SwitchEditModeHandler;
        
        // Initialize floors array
        int size = GridSystem.Instance.gridWidth * GridSystem.Instance.gridHeight;
        floors = new Floor[size];
    }

    private void OnDestroy()
    {
        UIBuilding.Instance.SwitchEditMode -= SwitchEditModeHandler;
    }

    private void SwitchEditModeHandler(bool isEditMode)
    {
        this.isEditMode = isEditMode;
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

    private void Update()
    {
        if (isEditMode)
        {
            if (Input.GetMouseButton(0))
            {
                Vector3 mousePos = Input.mousePosition;
                Ray ray = playerCamera.ScreenPointToRay(mousePos);
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
                {
                    Vector2Int gridPosition = GridSystem.Instance.WorldToGridPosition(hit.point);
                    if (GridSystemExtension.IsValidGridPosition(gridPosition, GridSystem.Instance.gridWidth, GridSystem.Instance.gridHeight))
                    {
                        if (GridSystem.Instance.IsCellOccupied(gridPosition))
                        {
                            Debug.Log("Delete Floor at grid position: " + gridPosition);
                            int index = gridPosition.x + gridPosition.y * GridSystem.Instance.gridWidth;
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
                HandleCreateWallPreview();
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

    private void HandleWallDragging()
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = playerCamera.ScreenPointToRay(mousePos);
    
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            return;
        
        Vector2Int gridPosition = GridSystem.Instance.WorldToGridPosition(hit.point);
    
        // Check if grid position is valid
        if (!GridSystemExtension.IsValidGridPosition(gridPosition, GridSystem.Instance.gridWidth, GridSystem.Instance.gridHeight))
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
            GridSystemExtension.GetGridCenterPosition(gridPosition, GridSystem.Instance.gridOrigin, GridSystem.Instance.cellSize)
        );
    
        // Check if this direction already has a wall
        if (currentHitFloor.IsDirectionCovered(direction))
            return;
    
        // Update preview position and direction
        previousDirection = direction;
        UpdatePreviewPositionAndRotation(direction, _previewBuilding);
    
        // Update UI position
        if (uiBuildingConfirm != null && uiBuildingConfirm.gameObject.activeSelf)
        {
            uiBuildingConfirm.UpdatePosition(GridSystem.Instance.GridToWorldPosition(gridPosition));
        }
    }
    
    private Floor GetFloorAtGridPosition(Vector2Int gridPosition)
    {
        if (!GridSystemExtension.IsValidGridPosition(gridPosition, GridSystem.Instance.gridWidth, GridSystem.Instance.gridHeight))
            return null;

        int index = gridPosition.x + gridPosition.y * GridSystem.Instance.gridWidth;
        if (index < 0 || index >= floors.Length)
            return null;

        return floors[index];
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

    // private void HandleCreateWallPreview()
    // {
    //  
    //     bool found = false;
    //     int indexFloor = -1;
    //     for(int i =0; i < floors.Length; i++)
    //     {
    //         if (floors[i].buildID ==BuildID.Floor && floors[i].IsWallAvailable())
    //         {
    //             currentHitFloor = floors[i];
    //             found = true;
    //             indexFloor = i;
    //             break;
    //         }
    //     }
    //     if (!found)
    //     {
    //         CancelBuilding();
    //     }
    //     else
    //     {
    //         Vector2Int gridPosition = GridSystem.Instance.IndexToGridPosition(indexFloor);
    //         currentPreviewPosition = gridPosition;
    //         _previewBuilding = Instantiate(currentSelectedObject.prefab);
    //         previousDirection = currentHitFloor.GetRandomNullDirection();
    //         UpdatePreviewPositionAndRotation(previousDirection, _previewBuilding);
    //         uiBuildingConfirm.UpdatePosition(GridSystem.Instance.GridToWorldPosition(gridPosition));
    //         uiBuildingConfirm.ActiveCanvas(true);
    //     }
    // }
    private void HandleCreateWallPreview()
    {
        bool found = false;
        int indexFloor = -1;
        for(int i = 0; i < floors.Length; i++)
        {
            if (floors[i] != null && floors[i].buildID == BuildID.Floor && floors[i].IsWallAvailable())
            {
                currentHitFloor = floors[i];
                found = true;
                indexFloor = i;
                break;
            }
        }
    
        if (!found)
        {
            CancelBuilding();
        }
        else
        {
            Vector2Int gridPosition = GridSystem.Instance.IndexToGridPosition(indexFloor);
            currentPreviewPosition = gridPosition;
            _previewBuilding = Instantiate(currentSelectedObject.prefab);
            previousDirection = currentHitFloor.GetRandomNullDirection();
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

    private bool TryBuildObject(Vector2Int gridPosition)
    {
        if (!GridSystemExtension.IsValidGridPosition(gridPosition, GridSystem.Instance.gridWidth, GridSystem.Instance.gridHeight)) 
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
                Direction direction = GridSystemExtension.CalculateWallDirection(
                    _previewBuilding.transform.position,
                    GridSystemExtension.GetGridCenterPosition(currentPreviewPosition, GridSystem.Instance.gridOrigin, GridSystem.Instance.cellSize)
                );
                PlaceWall(gridPosition, direction);
                break;
        }

        return true;
    }

    private void PlaceFloor(Vector2Int gridPosition)
    {
        // Vector3 worldPosition = GridSystem.Instance.GridToWorldPosition(gridPosition);
        // GameObject newObject = Instantiate(currentSelectedObject.prefab, worldPosition, Quaternion.identity);
        Floor f = new Floor(BuildID.Floor);
        f.gridPos = gridPosition;
        int index = gridPosition.x + gridPosition.y * GridSystem.Instance.gridWidth;
        floors[index] = f;
    
        // Connect with surrounding floors
        Floor floor1 = GetFloorConnectWith(floors[index], Direction.Top);
        if (floor1 != null)
        {
            // Only check IsHaveWallAtDirection if floor1 is not null
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
        OnPlaceBuilding?.Invoke(currentSelectedObject.prefab, BuildID.Floor, gridPosition);
    }

    private void PlaceWall(Vector2Int gridPosition, Direction direction)
    {
        if (currentHitFloor == null) return;
        // Vector3 wallPosition = GridSystem.Instance.GridToWorldPosition(gridPosition);
        // GameObject wallObject = Instantiate(currentSelectedObject.prefab, wallPosition, Quaternion.identity);
        // wallObject.SetBuildingData(new BuildingData(currentSelectedObject));
        Wall w = new Wall(currentHitFloor, BuildID.Wall);
        currentHitFloor.SetWallWithDirection(direction, w);
        // UpdatePreviewPositionAndRotation(direction, wallObject);
        
        // Connect with adjacent floor
        Floor connectedFloor = GetFloorConnectWith(currentHitFloor, direction);
        if (connectedFloor != null)
        {
            Debug.Log("AAAAA"+BuildingExtension.GetOppositeWallDirection(direction));
            connectedFloor.SetWallWithDirection(BuildingExtension.GetOppositeWallDirection(direction),
                w);
        }
        OnPlaceBuilding?.Invoke(currentSelectedObject.prefab, BuildID.Wall, gridPosition);
    }

    public void DestroyBuilding(Building b)
    {
        if (b is Floor floor)
        {
            if (floor.IsDestroyAble())
            {
                Vector2Int gridPosition = floor.gridPos;
                Debug.Log($"Destroying floor at grid position: {gridPosition}");
                GridSystem.Instance.MarkGridCells(gridPosition, new Vector2Int(1,1), false); // fix vecter 2int to data later
                int index = gridPosition.x + gridPosition.y * GridSystem.Instance.gridWidth;
                if (index >= 0 && index < floors.Length)
                {
                    floors[index] = null;
                }
                BuildingMeshBaker.Instance.DestroyBuildingAtPosition(BuildID.Floor,gridPosition);
               
            }
        }
        else if (b is Wall wall)
        {
            BuildingMeshBaker.Instance.DestroyBuildingAtPosition(BuildID.Wall, wall.attachedFloor[0].gridPos);
        }
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

    private void SetGridMaterialValue(float value)
    {
        if (gridGameObject != null)
        {
            Renderer renderer = gridGameObject.GetComponent<Renderer>();
            Material gridMaterial = renderer.material;
            gridMaterial.SetFloat("_ShowGrid", value);
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

    private Floor GetFloorConnectWith(Floor f, Direction d)
    {
        if (f == null) return null;
    
        Vector2Int gridPosition = f.gridPos;
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

        if (!GridSystemExtension.IsValidGridPosition(checkPos, GridSystem.Instance.gridWidth, GridSystem.Instance.gridHeight))
        {
            return null;
        }

        int index = checkPos.x + checkPos.y * GridSystem.Instance.gridWidth;
        if (index < 0 || index >= floors.Length)
        {
            return null;
        }

        return floors[index];
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

        Floor closestFloor = null;
        float closestDistance = Mathf.Infinity;

        foreach (Floor floor in availableFloors)
        {
            float distance = Vector3.Distance(playerPos, GridSystem.Instance.GridPosToWorldPosition(floor.gridPos));
            if (distance < closestDistance)
            {
                Vector2Int gridPosition = GridSystem.Instance.WorldToGridPosition( GridSystem.Instance.GridPosToWorldPosition(floor.gridPos));

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

            if (GridSystemExtension.IsValidGridPosition(adjacentPos, GridSystem.Instance.gridWidth, GridSystem.Instance.gridHeight) 
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
        return new Vector2Int(Mathf.Clamp(gridPosition.x, 0, GridSystem.Instance.gridWidth - 1),
            Mathf.Clamp(gridPosition.y, 0, GridSystem.Instance.gridHeight - 1));
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
        float cellSize = GridSystem.Instance.cellSize;
        
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