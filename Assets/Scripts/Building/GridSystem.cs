// using System.Collections.Generic;
// using UnityEngine;
//
// public class GridSystem : MonoBehaviour
// {
//     #region Singleton
//
//     public static GridSystem Instance { get; private set; }
//
//     private void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//         }
//         else
//         {
//             Destroy(this.gameObject);
//         }
//     }
//
//     #endregion
//
//     #region Properties and Fields
//
//     [Header("Grid Settings")] public int gridWidth = 50;
//     public int gridHeight = 50;
//     public float cellSize = 1f;
//     public Vector3 gridOrigin = Vector3.zero;
//
//     [Header("Gizmos")] public bool showGrid = true;
//     public Color gridColor = Color.white;
//     public Color occupiedCellColor = Color.red;
//     public Color unoccupiedCellColor = Color.green;
//     [Header("Building")] public BuildDataSO currentSelectedObject;
//     public LayerMask groundLayer = 1;
//     public LayerMask buildingLayerMask = 1;
//     public Building currentBuildingPlaced;
//
//     private Building previewBuilding;
//     private Vector2Int currentPreviewPosition;
//     public bool isPlacing = false;
//     public bool isDragging = false;
//     public Direction previousDirection;
//     private bool[,] gridData;
//     private Floor currentHitFloor;
//     private Floor[] floors;
//     private UIBuildingComfirm uiBuildingConfirm;
//     private Camera playerCamera;
//
//
//     [SerializeField] private GameObject gridGameObject; //get material from this game object
//     #endregion
//
//     #region Initialization
//
//     void Start()
//     {
//         playerCamera = Camera.main;
//         InitializeGrid();
//         uiBuildingConfirm = FindObjectOfType<UIBuildingComfirm>();
//     }
//
//     void InitializeGrid()
//     {
//         gridData = new bool[gridWidth, gridHeight];
//         floors = new Floor[gridWidth * gridHeight];
//     }
//
//     #endregion
//
//     #region Main Update Loop
//
//     void Update()
//     {
//         if (Utilities.IsPointerOverUIElement()) return;
//         if (isPlacing && previewBuilding != null)
//         {
//             HandlePlacementMode();
//         }
//
//         if (Input.GetMouseButton(0) && IsDraggingWall())
//         {
//             Vector3 mousePos = Input.mousePosition;
//             Ray ray = playerCamera.ScreenPointToRay(mousePos);
//             if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
//             {
//                 if (Physics.Raycast(ray, out RaycastHit hitBuilding, Mathf.Infinity, buildingLayerMask))
//                 {
//                     Vector2Int gridPosition = WorldToGridPosition(hitBuilding.point);
//                     Building building = hitBuilding.collider.GetComponent<Building>();
//                     if (building is Floor)
//                     {
//                         if (building != currentHitFloor)
//                         {
//                             currentHitFloor = building as Floor;
//                         }
//
//                         currentPreviewPosition = gridPosition;
//                         Direction direction =
//                             CalculateWallDirection(hit.point, GetGridCenterPosition(gridPosition));
//                         if (currentHitFloor.IsDirectionCovered(direction)) return;
//                         previousDirection = direction;
//                         UpdatePreviewPositionAndRotation(direction, previewBuilding);
//                     }
//                 }
//             }
//         }
//
//
//         if (Input.GetMouseButton(0) && IsDraggingFloor())
//         {
//             Vector3 mousePos = Input.mousePosition;
//             Ray ray = playerCamera.ScreenPointToRay(mousePos);
//             if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
//             {
//                 Vector2Int gridPosition = WorldToGridPosition(hit.point);
//                 if (IsCellOccupied(gridPosition, currentSelectedObject.gridSize)) return;
//                 if (gridPosition != currentPreviewPosition)
//                 {
//                     currentPreviewPosition = gridPosition;
//                     UpdatePreviewTransform();
//
//                     // Update confirmation UI position if it's active
//                     if (uiBuildingConfirm != null && uiBuildingConfirm.gameObject.activeSelf)
//                     {
//                         uiBuildingConfirm.UpdatePosition(GridToWorldPosition(gridPosition));
//                     }
//                 }
//             }
//         }
//     }
//
//     private bool IsDraggingWall()
//     {
//         return isDragging && currentSelectedObject.buildID == BuildID.Wall;
//     }
//
//     private bool IsDraggingFloor()
//     {
//         return isDragging && currentSelectedObject.buildID == BuildID.Floor;
//     }
//
//     private void HandlePlacementMode()
//     {
//         // Handle dragging with mouse click
//         if (Input.GetMouseButtonDown(0) && !Utilities.IsPointerOverUIElement())
//         {
//             isDragging = true;
//         }
//
//         if (isDragging)
//         {
//             uiBuildingConfirm.ActiveCanvas(false);
//         }
//
//         if (Input.GetMouseButtonUp(0))
//         {
//             isDragging = false;
//             if (uiBuildingConfirm != null)
//             {
//                 uiBuildingConfirm.ActiveCanvas(true);
//                 uiBuildingConfirm.UpdatePosition(GridToWorldPosition(currentPreviewPosition));
//             }
//         }
//     }
//
//     #endregion
//
//     #region Building Placement
//
//     /// <summary>
//     /// Begins the building placement process for a selected building type
//     /// </summary>
//     public void StartPlacingBuilding(BuildDataSO buildData)
//     {
//         SetGridMaterialValue(1);
//         currentSelectedObject = buildData;
//         switch (buildData.buildID)
//         {
//             case BuildID.Floor:
//                 HandleCreateFloorPreview();
//                 break;
//             case BuildID.Wall:
//                 HandleCreateWallPreview();
//                 break;
//         }
//
//
//         isPlacing = true;
//         // isDragging = true;
//     }
//     private void SetGridMaterialValue(float value)
//     {
//         if (gridGameObject != null)
//         {
//             Renderer renderer = gridGameObject.GetComponent<Renderer>();
//             Material gridMaterial = renderer.material; 
//             gridMaterial.SetFloat("_ShowGrid", value);
//         }
//     }
//     private void HandleCreateWallPreview()
//     {
//         Floor closestFloor = FindClosestAvailableFloorFromCharacter4Dir(out Vector2Int direction);
//         if (closestFloor == null)
//         {
//             CanvasController.Instance.SetActiveGameplayCanvas(true);
//         }
//         else
//         {
//             CreatePreview(direction);
//         }
//     }
//
//
//     private void HandleCreateFloorPreview()
//     {
//         Floor closestFloor = FindClosestAvailableFloorFromCharacter8Dir(out Vector2Int direction);
//         if (closestFloor == null)
//         {
//             Vector2Int characterPos = GetClosestGridPositionFromCharacter();
//             CreatePreview(characterPos);
//         }
//         else
//         {
//             CreatePreview(direction);
//         }
//     }
//
//     private Floor FindClosestAvailableFloorFromCharacter8Dir(out Vector2Int randomFreePos)
//     {
//         Vector3 playerPos = PlayerControl.Instance.transform.position;
//         randomFreePos = Vector2Int.zero;
//         List<Floor> availableFloors = new List<Floor>();
//
//         for (int i = 0; i < floors.Length; i++)
//         {
//             if (floors[i] != null)
//             {
//                 availableFloors.Add(floors[i]);
//             }
//         }
//
//         // Find closest floor with free adjacent positions
//         Floor closestFloor = null;
//         float closestDistance = Mathf.Infinity;
//
//         foreach (Floor floor in availableFloors)
//         {
//             float distance = Vector3.Distance(playerPos, floor.transform.position);
//             if (distance < closestDistance)
//             {
//                 Vector2Int gridPosition = WorldToGridPosition(floor.transform.position);
//
//                 if (HasFreeAdjacent8Position(gridPosition, out Vector2Int direction))
//                 {
//                     closestDistance = distance;
//                     closestFloor = floor;
//                     randomFreePos = direction;
//                 }
//             }
//         }
//
//
//         if (closestFloor != null)
//         {
//             Debug.Log($"Closest floor found at {closestFloor.transform.position} with free adjacent position");
//             return closestFloor;
//         }
//         else
//         {
//             Debug.Log("No floor found with free adjacent positions");
//             return null;
//         }
//     }
//
//     private Floor FindClosestAvailableFloorFromCharacter4Dir(out Vector2Int randomFreePos)
//     {
//         Vector3 playerPos = PlayerControl.Instance.transform.position;
//         randomFreePos = Vector2Int.zero;
//         List<Floor> availableFloors = new List<Floor>();
//
//         for (int i = 0; i < floors.Length; i++)
//         {
//             if (floors[i] != null)
//             {
//                 availableFloors.Add(floors[i]);
//             }
//         }
//
//         // Find closest floor with free adjacent positions
//         Floor closestFloor = null;
//         float closestDistance = Mathf.Infinity;
//
//         foreach (Floor floor in availableFloors)
//         {
//             float distance = Vector3.Distance(playerPos, floor.transform.position);
//             if (distance < closestDistance)
//             {
//                 Vector2Int gridPosition = WorldToGridPosition(floor.transform.position);
//
//                 if (HasFreeAdjacent4Position(gridPosition, out Vector2Int direction))
//                 {
//                     closestDistance = distance;
//                     closestFloor = floor;
//                     randomFreePos = direction;
//                 }
//             }
//         }
//
//         if (closestFloor != null)
//         {
//             Debug.Log($"Closest floor found at {closestFloor.transform.position} with free adjacent position");
//             return closestFloor;
//         }
//         else
//         {
//             Debug.Log("No floor found with free adjacent positions");
//             return null;
//         }
//     }
//
//     private bool HasFreeAdjacent8Position(Vector2Int gridPosition, out Vector2Int randomFreePos)
//     {
//         Vector2Int[] directions =
//         {
//             Vector2Int.up,
//             Vector2Int.up + Vector2Int.right,
//             Vector2Int.right,
//             Vector2Int.down + Vector2Int.right,
//             Vector2Int.down,
//             Vector2Int.down + Vector2Int.left,
//             Vector2Int.left,
//             Vector2Int.up + Vector2Int.left
//         };
//
//         List<Vector2Int> freePositions = new List<Vector2Int>();
//
//         foreach (Vector2Int direction in directions)
//         {
//             Vector2Int adjacentPos = gridPosition + direction;
//
//             if (IsValidGridPosition(adjacentPos) && !gridData[adjacentPos.x, adjacentPos.y])
//             {
//                 freePositions.Add(adjacentPos);
//             }
//         }
//
//         if (freePositions.Count > 0)
//         {
//             randomFreePos = freePositions[UnityEngine.Random.Range(0, freePositions.Count)];
//             return true;
//         }
//
//         randomFreePos = default; // hoặc Vector2Int.zero
//         return false;
//     }
//
//     private bool HasFreeAdjacent4Position(Vector2Int gridPosition, out Vector2Int randomFreePos)
//     {
//         Vector2Int[] directions =
//         {
//             Vector2Int.up,
//             Vector2Int.right,
//             Vector2Int.down,
//             Vector2Int.left
//         };
//
//         List<Vector2Int> freePositions = new List<Vector2Int>();
//
//         foreach (Vector2Int direction in directions)
//         {
//             Vector2Int adjacentPos = gridPosition + direction;
//
//             if (IsValidGridPosition(adjacentPos) && !gridData[adjacentPos.x, adjacentPos.y])
//             {
//                 freePositions.Add(adjacentPos);
//             }
//         }
//
//         if (freePositions.Count > 0)
//         {
//             randomFreePos = freePositions[UnityEngine.Random.Range(0, freePositions.Count)];
//             return true;
//         }
//
//         randomFreePos = default; // hoặc Vector2Int.zero
//         return false;
//     }
//
//
//     public void PlaceBuilding()
//     {
//         if (previewBuilding != null && isPlacing)
//         {
//             // Try to build at current preview position
//             bool success = TryBuildObject(currentPreviewPosition);
//             Destroy(previewBuilding.gameObject);
//
//             if (success)
//             {
//                 // Find a valid adjacent position for the next preview
//                 Vector2Int? nextPosition = FindValidAdjacentPosition(currentBuildingPlaced);
//                 if (nextPosition.HasValue)
//                 {
//                     CreatePreview(nextPosition.Value);
//                     UpdatePreviewTransform();
//                     isPlacing = true;
//
//                     // Update the UI confirmation to the new position
//                     if (uiBuildingConfirm != null)
//                     {
//                         uiBuildingConfirm.ActiveCanvas(true);
//                         uiBuildingConfirm.UpdatePosition(GridToWorldPosition(nextPosition.Value));
//                     }
//                 }
//                 else
//                 {
//                     Debug.Log("No valid adjacent position found for the next building.");
//                     // If no valid adjacent position, cancel placing
//                     CancelBuilding();
//                 }
//             }
//             else
//             {
//                 CancelBuilding();
//             }
//         }
//     }
//
//     private Floor GetFloorConnectWith(Floor f, Direction d)
//     {
//         Vector2Int gridPosition = WorldToGridPosition(f.transform.position);
//         Vector2Int checkPos = gridPosition;
//
//         switch (d)
//         {
//             case Direction.Top:
//                 checkPos += Vector2Int.up;
//                 break;
//             case Direction.Right:
//                 checkPos += Vector2Int.right;
//                 break;
//             case Direction.Bot:
//                 checkPos += Vector2Int.down;
//                 break;
//             case Direction.Left:
//                 checkPos += Vector2Int.left;
//                 break;
//         }
//
//         // Check if the position is valid before accessing the array
//         if (!IsValidGridPosition(checkPos))
//         {
//             Debug.Log($"Position {checkPos} is outside the grid bounds");
//             return null;
//         }
//
//         int index = checkPos.x + checkPos.y * gridWidth;
//
//         // Verify index is within array bounds
//         if (index < 0 || index >= floors.Length)
//         {
//             Debug.Log($"Index {index} is outside the floors array bounds");
//             return null;
//         }
//
//         Floor checkFloor = floors[index];
//
//         if (checkFloor != null)
//         {
//             Debug.Log($"Found floor at {checkPos} +index {index}");
//             return checkFloor;
//         }
//         else
//         {
//             Debug.Log($"No floor at {checkPos} +index {index}");
//             return null;
//         }
//     }
//
//     /// <summary>
//     /// Cancels the current building placement operation
//     /// </summary>
//     public void CancelBuilding()
//     {
//         SetGridMaterialValue(0);
//         if (previewBuilding != null)
//         {
//             Destroy(previewBuilding.gameObject);
//             previewBuilding = null;
//         }
//
//         isPlacing = false;
//         isDragging = false;
//
//         if (uiBuildingConfirm != null)
//         {
//             uiBuildingConfirm.ActiveCanvas(false);
//         }
//
//         CanvasController.Instance.SetActiveGameplayCanvas(true);
//         Debug.Log("Building placement cancelled");
//     }
//
//
//     public bool TryBuildObject(Vector2Int gridPosition)
//     {
//         if (!IsValidGridPosition(gridPosition)) return false;
//         switch (currentSelectedObject.buildID)
//         {
//             case BuildID.Floor:
//                 if (!CanPlaceObject(gridPosition, currentSelectedObject.gridSize)) return false;
//                 MarkGridCells(gridPosition, currentSelectedObject.gridSize, true);
//                 PlaceFloor(gridPosition);
//                 break;
//             case BuildID.Wall:
//                 Direction direction = CalculateWallDirection(previewBuilding.transform.position,
//                     GetGridCenterPosition(gridPosition));
//                 PlaceWall(gridPosition, direction);
//                 break;
//         }
//
//         return true;
//     }
//
//     private bool CanPlaceObject(Vector2Int gridPosition, Vector2Int gridSize)
//     {
//         for (int x = 0; x < gridSize.x; x++)
//         {
//             for (int y = 0; y < gridSize.y; y++)
//             {
//                 Vector2Int checkPos = gridPosition + new Vector2Int(x, y);
//                 if (!IsValidGridPosition(checkPos) || gridData[checkPos.x, checkPos.y])
//                 {
//                     return false;
//                 }
//             }
//         }
//
//         return true;
//     }
//
//     private void MarkGridCells(Vector2Int gridPosition, Vector2Int gridSize, bool isOccupied)
//     {
//         for (int x = 0; x < gridSize.x; x++)
//         {
//             for (int y = 0; y < gridSize.y; y++)
//             {
//                 Vector2Int cellPos = gridPosition + new Vector2Int(x, y);
//                 gridData[cellPos.x, cellPos.y] = isOccupied;
//             }
//         }
//     }
//
//     private bool IsCellOccupied(Vector2Int gridPosition, Vector2Int gridSize)
//     {
//         for (int x = 0; x < gridSize.x; x++)
//         {
//             for (int y = 0; y < gridSize.y; y++)
//             {
//                 Vector2Int checkPos = gridPosition + new Vector2Int(x, y);
//                 if (!IsValidGridPosition(checkPos) || gridData[checkPos.x, checkPos.y])
//                 {
//                     return true;
//                 }
//             }
//         }
//
//         return false;
//     }
//
//     private void PlaceFloor(Vector2Int gridPosition)
//     {
//         Vector3 worldPosition = GridToWorldPosition(gridPosition);
//         Building newObject = Instantiate(currentSelectedObject.prefab, worldPosition, Quaternion.identity);
//
//         // Calculate correct index based on the grid position where the floor is placed
//         int index = gridPosition.x + gridPosition.y * gridWidth;
//
//         // Store the floor in the correct position in the array
//         floors[index] = newObject as Floor;
//         currentBuildingPlaced = newObject;
//     }
//
//
//     private void PlaceWall(Vector2Int gridPosition, Direction direction)
//     {
//         if (currentHitFloor == null) return;
//         Vector3 wallPosition = GridToWorldPosition(gridPosition);
//         Building wallObject = Instantiate(currentSelectedObject.prefab, wallPosition, Quaternion.identity);
//         currentHitFloor.SetWallWithDirection(direction, wallObject as Wall);
//         UpdatePreviewPositionAndRotation(direction, wallObject);
//         //get connected floor at wall direction
//         Floor connectedFloor = GetFloorConnectWith(currentHitFloor, direction);
//         if (connectedFloor != null)
//         {
//             // Set the wall on the connected floor
//             connectedFloor.SetWallWithDirection(BuildingExtension.GetOppositeWallDirection(direction),
//                 wallObject as Wall);
//         }
//     }
//
//     #endregion
//
//     #region Preview Management
//
//     private void CreatePreview(Vector2Int gridPosition)
//     {
//         if (previewBuilding != null)
//         {
//             Destroy(previewBuilding);
//         }
//
//         currentPreviewPosition = gridPosition;
//         previewBuilding = Instantiate(currentSelectedObject.prefab);
//         UpdatePreviewTransform();
//         Debug.Log($"Preview created at grid position: {currentPreviewPosition}");
//         uiBuildingConfirm.ActiveCanvas(true);
//         uiBuildingConfirm.UpdatePosition(GridToWorldPosition(currentPreviewPosition));
//         Debug.Log($"Preview position updated to: {GridToWorldPosition(currentPreviewPosition)}");
//     }
//
//     private void UpdatePreviewTransform()
//     {
//         if (previewBuilding != null)
//         {
//             previewBuilding.transform.position = GridToWorldPosition(currentPreviewPosition);
//         }
//     }
//
//
//     private void UpdatePreviewPositionAndRotation(Direction dir, Building preview)
//     {
//         float rotation = 0f;
//         Vector3 offset = Vector3.zero;
//         switch (dir)
//         {
//             case Direction.Top:
//                 rotation = 90f;
//                 offset = new Vector3(cellSize / 2, 0, cellSize);
//                 break;
//             case Direction.Right:
//                 rotation = 0;
//                 offset = new Vector3(cellSize, 0, cellSize / 2);
//                 break;
//             case Direction.Bot:
//                 rotation = 90f;
//                 offset = new Vector3(cellSize / 2, 0, 0);
//                 break;
//             case Direction.Left:
//                 rotation = 0;
//                 offset = new Vector3(0, 0, cellSize / 2);
//                 break;
//         }
//
//         preview.transform.position = GridToWorldPosition(currentPreviewPosition) + offset;
//         preview.transform.rotation = Quaternion.Euler(0, rotation, 0);
//     }
//
//     #endregion
//
//     #region Utility Methods
//
//     public Vector2Int GetClosestGridPositionFromCharacter()
//     {
//         Vector3 characterPos = PlayerControl.Instance.transform.position;
//         Vector2Int gridPosition = WorldToGridPosition(characterPos);
//         return new Vector2Int(Mathf.Clamp(gridPosition.x, 0, gridWidth - 1),
//             Mathf.Clamp(gridPosition.y, 0, gridHeight - 1));
//     }
//
//     public Vector2Int WorldToGridPosition(Vector3 worldPosition)
//     {
//         Vector3 localPosition = worldPosition - gridOrigin;
//         int x = Mathf.FloorToInt(localPosition.x / cellSize);
//         int z = Mathf.FloorToInt(localPosition.z / cellSize);
//         return new Vector2Int(x, z);
//     }
//
//     Vector3 GridToWorldPosition(Vector2Int gridPosition)
//     {
//         return gridOrigin + new Vector3(gridPosition.x * cellSize, 0, gridPosition.y * cellSize);
//     }
//
//     public Vector3 GetGridCenterPosition(Vector2Int gridPosition)
//     {
//         return gridOrigin + new Vector3(gridPosition.x * cellSize + cellSize / 2, 0,
//             gridPosition.y * cellSize + cellSize / 2);
//     }
//
//     private Direction CalculateWallDirection(Vector3 hitPoint, Vector3 floorCenter)
//     {
//         Vector2 directionVector = new Vector2(hitPoint.x - floorCenter.x, hitPoint.z - floorCenter.z).normalized;
//         float angle = Mathf.Atan2(directionVector.y, directionVector.x) * Mathf.Rad2Deg;
//         if (angle < 0) angle += 360f;
//         if (angle >= 45f && angle < 135f) return Direction.Top;
//         if (angle >= 135f && angle < 225f) return Direction.Left;
//         if (angle >= 225f && angle < 315f) return Direction.Bot;
//         return Direction.Right;
//     }
//
//     public bool IsValidGridPosition(Vector2Int gridPosition)
//     {
//         return gridPosition.x >= 0 && gridPosition.x < gridWidth &&
//                gridPosition.y >= 0 && gridPosition.y < gridHeight;
//     }
//
//
//     private Vector2Int? FindValidAdjacentPosition(Building building)
//     {
//         if (building == null) return null;
//
//         Vector2Int currentPosition = WorldToGridPosition(building.transform.position);
//         Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
//
//         // Shuffle directions for more natural placement patterns
//         ShuffleArray(directions);
//
//         foreach (Vector2Int direction in directions)
//         {
//             Vector2Int adjacentPosition = currentPosition + direction;
//
//             if (IsValidGridPosition(adjacentPosition) && IsValidBuildingPosition(adjacentPosition))
//             {
//                 return adjacentPosition;
//             }
//         }
//
//         return null;
//     }
//
//
//     private bool IsValidBuildingPosition(Vector2Int position)
//     {
//         for (int x = 0; x < currentSelectedObject.gridSize.x; x++)
//         {
//             for (int y = 0; y < currentSelectedObject.gridSize.y; y++)
//             {
//                 Vector2Int checkPos = position + new Vector2Int(x, y);
//                 if (!IsValidGridPosition(checkPos) || gridData[checkPos.x, checkPos.y])
//                 {
//                     return false;
//                 }
//             }
//         }
//
//         return true;
//     }
//
//     private void ShuffleArray<T>(T[] array)
//     {
//         // Fisher-Yates shuffle algorithm
//         System.Random random = new System.Random();
//         for (int i = array.Length - 1; i > 0; i--)
//         {
//             int j = random.Next(0, i + 1);
//             T temp = array[i];
//             array[i] = array[j];
//             array[j] = temp;
//         }
//     }
//
//     #endregion
//
//     #region GUI and Gizmos
//
// #if UNITY_EDITOR
//
//     void OnDrawGizmos()
//     {
//         if (!showGrid) return;
//
//         // Only draw in editor or if application is playing and showGrid is true
//         if (!Application.isPlaying && !showGrid) return;
//
//         Gizmos.color = gridColor;
//
//         // Draw horizontal lines
//         for (int x = 0; x <= gridWidth; x++)
//         {
//             Vector3 startPos = gridOrigin + new Vector3(x * cellSize, 0.01f, 0);
//             Vector3 endPos = gridOrigin + new Vector3(x * cellSize, 0.01f, gridHeight * cellSize);
//             Gizmos.DrawLine(startPos, endPos);
//         }
//
//         // Draw vertical lines
//         for (int z = 0; z <= gridHeight; z++)
//         {
//             Vector3 startPos = gridOrigin + new Vector3(0, 0.01f, z * cellSize);
//             Vector3 endPos = gridOrigin + new Vector3(gridWidth * cellSize, 0.01f, z * cellSize);
//             Gizmos.DrawLine(startPos, endPos);
//         }
//
//
//         // Optionally highlight occupied cells
//         if (Application.isPlaying && gridData != null)
//         {
//             for (int x = 0; x < gridWidth; x++)
//             {
//                 for (int z = 0; z < gridHeight; z++)
//                 {
//                     Vector3 cellCenter = gridOrigin + new Vector3((x + 0.5f) * cellSize, 0.02f, (z + 0.5f) * cellSize);
//
//                     if (gridData[x, z])
//                     {
//                         Gizmos.color = occupiedCellColor; // Color for occupied cells
//                         Gizmos.DrawCube(cellCenter, new Vector3(cellSize * 0.9f, 0.01f, cellSize * 0.9f));
//                     }
//                     else
//                     {
//                         Gizmos.color = unoccupiedCellColor; // Color for unoccupied cells
//                         Gizmos.DrawCube(cellCenter, new Vector3(cellSize * 0.9f, 0.01f, cellSize * 0.9f));
//                     }
//                 }
//             }
//         }
//     }
// #endif
//
//     #endregion
// }
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

    [Header("Grid Settings")] 
    public int gridWidth = 50;
    public int gridHeight = 50;
    public float cellSize = 1f;
    public Vector3 gridOrigin = Vector3.zero;

    [Header("Gizmos")] 
    public bool showGrid = true;
    public Color gridColor = Color.white;
    public Color occupiedCellColor = Color.red;
    public Color unoccupiedCellColor = Color.green;
    
    [Header("Building")] 
    public BuildDataSO currentSelectedObject;
    public LayerMask groundLayer = 1;
    public LayerMask buildingLayerMask = 1;
    public Building currentBuildingPlaced;

    private Building previewBuilding;
    private Vector2Int currentPreviewPosition;
    public bool isPlacing = false;
    public bool isDragging = false;
    public Direction previousDirection;
    private bool[,] gridData;
    private Floor currentHitFloor;
    private Floor[] floors;
    private UIBuildingComfirm uiBuildingConfirm;
    private Camera playerCamera;

    [SerializeField] private GameObject gridGameObject; //get material from this game object
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
        floors = new Floor[gridWidth * gridHeight];
    }

    #endregion

    #region Main Update Loop

    void Update()
    {
        if (Utilities.IsPointerOverUIElement()) return;
        
        if (isPlacing && previewBuilding != null)
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
                    UpdatePreviewPositionAndRotation(direction, previewBuilding);
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
        // Handle dragging with mouse click
        if (Input.GetMouseButtonDown(0) && !Utilities.IsPointerOverUIElement())
        {
            isDragging = true;
        }

        if (isDragging)
        {
            uiBuildingConfirm.ActiveCanvas(false);
        }

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

    /// <summary>
    /// Begins the building placement process for a selected building type
    /// </summary>
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
        Floor closestFloor = FindClosestAvailableFloorFromCharacter4Dir(out Vector2Int direction);
        if (closestFloor == null)
        {
            CanvasController.Instance.SetActiveGameplayCanvas(true);
        }
        else
        {
            CreatePreview(direction);
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

    private Floor FindClosestAvailableFloorFromCharacter4Dir(out Vector2Int randomFreePos)
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

                if (HasFreeAdjacent4Position(gridPosition, out Vector2Int direction))
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

    private bool HasFreeAdjacent4Position(Vector2Int gridPosition, out Vector2Int randomFreePos)
    {
        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
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
                    previewBuilding.transform.position,
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

        // Calculate correct index based on the grid position where the floor is placed
        int index = gridPosition.x + gridPosition.y * gridWidth;

        // Store the floor in the correct position in the array
        floors[index] = newObject as Floor;
        currentBuildingPlaced = newObject;
    }

    private void PlaceWall(Vector2Int gridPosition, Direction direction)
    {
        if (currentHitFloor == null) return;
        Vector3 wallPosition = GridToWorldPosition(gridPosition);
        Building wallObject = Instantiate(currentSelectedObject.prefab, wallPosition, Quaternion.identity);
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
        if (previewBuilding != null)
        {
            Destroy(previewBuilding);
        }

        currentPreviewPosition = gridPosition;
        previewBuilding = Instantiate(currentSelectedObject.prefab);
        UpdatePreviewTransform();
        
        uiBuildingConfirm.ActiveCanvas(true);
        uiBuildingConfirm.UpdatePosition(GridToWorldPosition(currentPreviewPosition));
    }

    private void UpdatePreviewTransform()
    {
        if (previewBuilding != null)
        {
            previewBuilding.transform.position = GridToWorldPosition(currentPreviewPosition);
        }
    }

    private void UpdatePreviewPositionAndRotation(Direction dir, Building preview)
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

        preview.transform.position = GridToWorldPosition(currentPreviewPosition) + offset;
        preview.transform.rotation = Quaternion.Euler(0, rotation, 0);
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

    private Vector2Int? FindValidAdjacentPosition(Building building)
    {
        if (building == null) return null;

        Vector2Int currentPosition = WorldToGridPosition(building.transform.position);
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        // Shuffle directions for more natural placement patterns
        GridSystemExtension.ShuffleArray(directions);

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