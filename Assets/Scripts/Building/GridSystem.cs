using System;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GridSystem : MonoBehaviour
{
    #region Singleton

    public static GridSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        InitializeGrid();
    }

    private void Start()
    {
        BuildingManager.Instance.OnDone += HandleBakeMesh;
        UIBuilding.Instance.SwitchEditMode += OnEditMode;

        // Load saved world data
        WorldData worldData = SaveManager.Instance.LoadWorldData();
        if (worldData != null)
        {
            LoadWorldData(worldData);
        }
    }

    private void OnEditMode(bool isEditMode)
    {
        if (isEditMode)
        {
            ClearChunks();
            RecreateWorld();
        }
        else
        {
            HandleBakeMesh();
        }
    }

    private void ClearChunks()
    {
        foreach (var chunk in chunks)
        {
            ClearChunkObjects(chunk.floorsParent);
            ClearChunkObjects(chunk.wallsParents);
        }
    }

    private void ClearChunkObjects(GameObject parent)
    {
        if (parent != null)
        {
            foreach (Transform child in parent.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void OnDestroy()
    {
        if (BuildingManager.Instance != null)
            BuildingManager.Instance.OnDone -= HandleBakeMesh;

        if (UIBuilding.Instance != null)
            UIBuilding.Instance.SwitchEditMode -= OnEditMode;
    }

    #endregion

    #region Properties and Fields

    [Header("Grid Settings")]
    [SerializeField]
    private int gridWidth = 50;

    public int GridSizeX => gridWidth;
    public int GridHeight => gridHeight;

    [SerializeField] private int gridHeight = 50;
    public int GridWidth => gridWidth;
    [SerializeField] private float cellSize = 1f;
    public float CellSize => cellSize;
    [SerializeField] private Vector3 gridOrigin = Vector3.zero;

    [Header("Chunk Settings")] public int chunkCountX = 3;
    [SerializeField] private int chunkCountY = 3;
    private int chunkSizeX;
    private int chunkSizeY;

    public List<Chunk> chunks = new List<Chunk>();
    private Dictionary<Vector2Int, int> chunkLookup = new Dictionary<Vector2Int, int>();

    [Header("Chunk Gizmos")] public bool showChunks = true;
    public Color chunkGizmoColor = new Color(0, 0.5f, 1f, 0.3f);

    public event Action<Vector2Int, bool> OnGridCellChanged;

    #endregion

    private void InitializeGrid()
    {
        chunkSizeX = Mathf.CeilToInt((float)gridWidth / chunkCountX);
        chunkSizeY = Mathf.CeilToInt((float)gridHeight / chunkCountY);
        chunks.Clear();
        chunkLookup.Clear();

        for (int y = 0; y < chunkCountY; y++)
        {
            for (int x = 0; x < chunkCountX; x++)
            {
                Vector2Int chunkCoord = new Vector2Int(x, y);
                Chunk newChunk = new Chunk(chunkCoord, chunkSizeX, chunkSizeY);
                int index = chunks.Count;
                chunks.Add(newChunk);
                chunkLookup[chunkCoord] = index;
            }
        }
    }

    #region Grid Cell Management

    public bool IsCellOccupied(Vector2Int gridPosition)
    {
        if (TryGetChunk(gridPosition, out Chunk chunk, out Vector2Int localPos))
        {
            return chunk.IsCellOccupied(localPos);
        }

        return true;
    }


    public T GetEntityById<T>(Vector2Int gridPosition, out int index) where T : Entity
    {
        if (TryGetChunk(gridPosition, out Chunk chunk, out Vector2Int localPos))
        {
            index = localPos.x + localPos.y * chunkSizeX;
            return chunk.GetCell(localPos) as T;
        }

        index = -1;
        return null;
    }

    public void SetCellInChunk(Vector2Int worldPos, Entity e, GameObject g)
    {
        if (TryGetChunk(worldPos, out Chunk chunk, out Vector2Int localPos))
        {
            chunk.SetCell(localPos, e, g);
        }
    }

    public bool CanPlaceObject(Vector2Int gridPosition)
    {
        return !IsCellOccupied(gridPosition);
    }

    public void SetWallWithDirection(Vector2Int gridPosition, Direction direction, GameObject g, EntityID id)
    {
        if (TryGetChunk(gridPosition, out Chunk chunk, out Vector2Int localPos))
        {
            chunk.SetWallWithDirection(localPos, direction, g, id);
        }
    }

    public void SetWallData(Vector2Int gridPosition, Direction direction, EntityID id)
    {
        if (TryGetChunk(gridPosition, out Chunk chunk, out Vector2Int localPos))
        {
            chunk.SetWallData(localPos, direction, id);
        }
    }

    public void SetFloorData(Vector2Int gridPosition, EntityID id)
    {
        if (TryGetChunk(gridPosition, out Chunk chunk, out Vector2Int localPos))
        {
            chunk.SetFloorData(localPos, id);
        }
    }

    public void MarkGridCells(Vector2Int gridPosition, Vector2Int gridSize, bool isOccupied)
    {
        // Notify any listeners that cells are changing state
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector2Int cellPos = gridPosition + new Vector2Int(x, y);
                if (TryGetChunk(cellPos, out Chunk chunk, out Vector2Int localPos))
                {
                    // You might need to update something in the chunk data here
                    // For example, mark the cell as occupied or update its state
                    OnGridCellChanged?.Invoke(cellPos, isOccupied);
                }
            }
        }
    }

    public Vector3 GetWorldPositionFromChunk(Vector2Int chunkCoord, Vector2Int localPos)
    {
        if (chunkLookup.TryGetValue(chunkCoord, out int index) && index >= 0 && index < chunks.Count)
        {
            Chunk chunk = chunks[index];
            return GridSystemExtension.GridToWorldPosition(
                new Vector2Int(chunkCoord.x * chunkSizeX + localPos.x, chunkCoord.y * chunkSizeY + localPos.y),
                gridOrigin, cellSize);
        }

        return Vector3.zero;
    }

    #endregion

    #region Grid Coordinate Conversion

    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        Vector3 localPosition = worldPosition - gridOrigin;
        int x = Mathf.FloorToInt(localPosition.x / cellSize);
        int z = Mathf.FloorToInt(localPosition.z / cellSize);
        return new Vector2Int(x, z);
    }

    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        return GridSystemExtension.GridToWorldPosition(gridPosition, gridOrigin, cellSize);
    }

    public Vector3 GridPosToWorldPosition(Vector2Int gridPosition)
    {
        return gridOrigin + new Vector3(gridPosition.x * cellSize, 0, gridPosition.y * cellSize);
    }

    public bool IsValidGridPosition(Vector2Int gridPosition)
    {
        return GridSystemExtension.IsValidGridPosition(gridPosition, gridWidth, gridHeight);
    }

    public Vector3 GetGridCenterPosition(Vector2Int gridPosition)
    {
        return GridSystemExtension.GetGridCenterPosition(gridPosition, gridOrigin, cellSize);
    }

    #endregion

    #region Chunk Management

    private bool TryGetChunk(Vector2Int gridPos, out Chunk chunk, out Vector2Int localPos)
    {
        if (!IsValidGridPosition(gridPos))
        {
            chunk = null;
            localPos = Vector2Int.zero;
            return false;
        }

        Vector2Int chunkCoord = new Vector2Int(gridPos.x / chunkSizeX, gridPos.y / chunkSizeY);
        localPos = new Vector2Int(gridPos.x % chunkSizeX, gridPos.y % chunkSizeY);

        if (chunkLookup.TryGetValue(chunkCoord, out int index) && index >= 0 && index < chunks.Count)
        {
            chunk = chunks[index];
            return true;
        }

        chunk = null;
        return false;
    }

    private int GetChunkIndex(Vector2Int chunkCoord)
    {
        if (chunkCoord.x < 0 || chunkCoord.x >= chunkCountX ||
            chunkCoord.y < 0 || chunkCoord.y >= chunkCountY)
            return -1;

        if (chunkLookup.TryGetValue(chunkCoord, out int index))
        {
            return index;
        }

        return -1;
    }

    #endregion

    #region Mesh Handling

    private void HandleBakeMesh()
    {
        for (int i = 0; i < chunks.Count; i++)
        {
            if (chunks[i].floorsParent != null && chunks[i].NeedRebuild)
            {
                CombineMeshes(chunks[i].floorsParent);
                CombineMeshes(chunks[i].wallsParents);
                chunks[i].NeedRebuild = false;
            }
        }
    }

    private void CombineMeshes(GameObject parent)
    {
        if (parent == null) return;

        MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>();
        if (meshFilters.Length == 0) return;

        // Find a valid material
        Material material = null;
        foreach (MeshRenderer renderer in parent.GetComponentsInChildren<MeshRenderer>())
        {
            if (renderer.sharedMaterial != null)
            {
                material = renderer.sharedMaterial;
                break;
            }
        }

        if (material == null) return;

        List<CombineInstance> validInstances = new List<CombineInstance>();

        foreach (MeshFilter mf in meshFilters)
        {
            if (mf.sharedMesh == null) continue;

            CombineInstance instance = new CombineInstance
            {
                mesh = mf.sharedMesh,
                transform = mf.transform.localToWorldMatrix
            };
            validInstances.Add(instance);
        }

        if (validInstances.Count == 0) return;

        // Create combined mesh object
        GameObject combinedObject = new GameObject("CombinedMesh");
        combinedObject.transform.SetParent(parent.transform);
        combinedObject.transform.localPosition = Vector3.zero;
        combinedObject.transform.localRotation = Quaternion.identity;
        combinedObject.transform.localScale = Vector3.one;

        MeshFilter combinedMeshFilter = combinedObject.AddComponent<MeshFilter>();
        MeshRenderer combinedRenderer = combinedObject.AddComponent<MeshRenderer>();

        Mesh combinedMesh = new Mesh
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
        combinedMesh.CombineMeshes(validInstances.ToArray(), true, true);

        if (combinedMesh.vertexCount == 0) return;

        combinedMeshFilter.sharedMesh = combinedMesh;
        combinedRenderer.sharedMaterial = material;
        combinedRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        combinedRenderer.receiveShadows = false;

        // Remove old children
        List<GameObject> childrenToDestroy = new List<GameObject>();
        foreach (Transform child in parent.transform)
        {
            if (child.gameObject != combinedObject)
            {
                childrenToDestroy.Add(child.gameObject);
            }
        }

        foreach (GameObject child in childrenToDestroy)
        {
            DestroyImmediate(child);
        }

        // Add collider
        MeshCollider collider = combinedObject.AddComponent<MeshCollider>();
        collider.sharedMesh = combinedMesh;
    }

    #endregion

    #region World Recreation

    [ContextMenu("Re Create")]
    public void RecreateWorld()
    {
        foreach (Chunk chunk in chunks)
        {
            RecreateChunk(chunk);
        }
    }

    private void RecreateChunk(Chunk chunk)
    {
        EnsureChunkParents(chunk);
        ClearChunkObjects(chunk.floorsParent);
        ClearChunkObjects(chunk.wallsParents);
        ClearChunkObjects(chunk.doorsParent);
        ClearChunkObjects(chunk.resourcesParent);

        // Loop through all cells in the chunk
        for (int x = 0; x < chunkSizeX; x++)
        {
            for (int y = 0; y < chunkSizeY; y++)
            {
                Vector2Int localPos = new Vector2Int(x, y);
                Entity floor = chunk.GetCell(localPos);

                if (floor != null && floor.entityID != EntityID.None)
                {
                    RecreateEntity(chunk, localPos, floor);
                }
            }
        }
    }

    private void EnsureChunkParents(Chunk chunk)
    {
        if (chunk.floorsParent == null)
        {
            chunk.floorsParent = new GameObject($"Floor_{chunk.chunkCoord.x}_{chunk.chunkCoord.y}");
            chunk.floorsParent.transform.SetParent(transform);
        }

        if (chunk.wallsParents == null)
        {
            chunk.wallsParents = new GameObject($"Wall_{chunk.chunkCoord.x}_{chunk.chunkCoord.y}");
            chunk.wallsParents.transform.SetParent(transform);
        }

        if (chunk.doorsParent == null)
        {
            chunk.doorsParent = new GameObject($"Door_{chunk.chunkCoord.x}_{chunk.chunkCoord.y}");
            chunk.doorsParent.transform.SetParent(transform);
        }
    }

    private void RecreateEntity(Chunk chunk, Vector2Int localPos, Entity entity)
    {
        Vector2Int globalPos = chunk.chunkCoord * new Vector2Int(chunkSizeX, chunkSizeY) + localPos;
        Vector3 worldPos = GridToWorldPosition(globalPos);
        if (entity.entityID.IsEntityBuilding())
        {
            GameObject floorObj = Instantiate(
                GameManager.Instance.GetBuildingDataByID(entity.entityID).prefab,
                worldPos,
                Quaternion.identity);

            FloorBehaviour floorBehaviour =
                EditBuildingFactory.AddEditScripts(entity.entityID, floorObj) as FloorBehaviour;
            floorBehaviour.SetBuildID(entity.entityID);
            floorObj.transform.SetParent(chunk.floorsParent.transform);

            int floorIndex = localPos.x + localPos.y * chunkSizeX;
            floorBehaviour.Init(entity.entityID, globalPos);

            // Create walls and doors
            RecreateWallsAndDoors(chunk, entity, floorIndex, globalPos, worldPos);
        }
        else
        {
            GameObject resourceObj = Instantiate(
                GameResourceManager.Instance.GetResourcePrefabByID(entity.entityID),
                worldPos + new Vector3(0.5f, 0, 0.5f), // Center the resource in the cell
                Quaternion.identity);
            resourceObj.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0);
            resourceObj.transform.SetParent(chunk.resourcesParent.transform);
            ResourceNode resourceNode = new ResourceNode(entity.entityID);
            resourceNode.gridPos = globalPos;
            ResourceFactory.CreateResourceBehavior(resourceNode, resourceObj);
            
            chunk.SetCell(localPos, resourceNode, resourceObj);
        }
    }

    private void RecreateWallsAndDoors(Chunk chunk, Entity entity, int floorIndex, Vector2Int globalPos,
        Vector3 worldPos)
    {
        switch (entity.entityID)
        {
            case EntityID.Floor:
                Floor floor = entity as Floor;
                if (floor == null) return;

                for (int i = 0; i < floor.buildingWithDirection.Length; i++)
                {
                    BuildingWithDirection building = floor.buildingWithDirection[i];
                    if (building.ID == EntityID.None) continue;

                    Direction direction = (Direction)(1 << i);
                    Vector3 offset = CalculateOffsetAndRotate(direction, out float rotate);

                    switch (building.ID)
                    {
                        case EntityID.Wall:
                            Debug.Log($"Creating wall at {globalPos} with direction {direction}");
                            CreateWall(chunk, building.ID, i, floorIndex, globalPos, worldPos, offset, rotate);
                            break;
                        case EntityID.Door:
                            CreateDoor(chunk, building.ID, i, floorIndex, globalPos, worldPos, offset, rotate);
                            break;
                    }
                }

                break;
        }
    }

    private void CreateWall(Chunk chunk, EntityID buildID, int directionIndex, int floorIndex,
        Vector2Int globalPos, Vector3 worldPos, Vector3 offset, float rotation)
    {
        GameObject wallObj = Instantiate(
            GameManager.Instance.GetBuildingDataByID(buildID).prefab,
            worldPos + offset,
            Quaternion.Euler(0, rotation, 0),
            chunk.wallsParents.transform);

        WallBehaviour wallBehaviour = EditBuildingFactory.AddEditScripts(buildID, wallObj) as WallBehaviour;
        wallBehaviour.SetBuildID(buildID);
        wallBehaviour.Init(directionIndex, floorIndex, buildID, globalPos);
    }

    private void CreateDoor(Chunk chunk, EntityID buildID, int directionIndex, int floorIndex,
        Vector2Int globalPos, Vector3 worldPos, Vector3 offset, float rotation)
    {
        GameObject doorObj = Instantiate(
            GameManager.Instance.GetBuildingDataByID(buildID).prefab,
            worldPos + offset,
            Quaternion.Euler(0, rotation, 0),
            chunk.doorsParent.transform);

        DoorBehaviour doorBehaviour = EditBuildingFactory.AddEditScripts(buildID, doorObj) as DoorBehaviour;
        doorBehaviour.SetBuildID(buildID);
        doorBehaviour.Init(directionIndex, floorIndex, buildID, globalPos);
    }

    #endregion

    #region Data Loading and Saving


    private void LoadWorldData(WorldData worldData)
    {
        ClearChunks();
        // Apply saved data
        foreach (var chunkData in worldData.chunkData)
        {
            int chunkIndex = GetChunkIndex(chunkData.chunkCoord);
            if (chunkIndex >= 0 && chunkIndex < chunks.Count)
            {
                Chunk chunk = chunks[chunkIndex];
                foreach (var entityData in chunkData.entities)
                {
                    Vector2Int localPos = new Vector2Int(entityData.localPos.x, entityData.localPos.y);
                    if (!entityData.entityID.IsEntityBuilding())
                    {
                        ResourceNode a = EntitiesExtention.CreateEntityByID<ResourceNode>(entityData.entityID);
                        chunk.chunkData[localPos.x, localPos.y] = a;
                    }
                    else
                    {
                        Floor floor = EntitiesExtention.CreateEntityByID<Floor>(entityData.entityID);
                        if (entityData.walls != null)
                        {
                            foreach (var wallData in entityData.walls)
                            {
                                Direction direction = (Direction)(1 << wallData.directionIndex);
                                floor.SetWall(direction, wallData.entityID);
                            }
                        }
                        chunk.chunkData[localPos.x, localPos.y] = floor;
                    }
                    chunk.NeedRebuild = true;
                }
            }
        }

        RecreateWorld();
        HandleBakeMesh();
    }

    #endregion

    #region Helper Methods

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

    #region Gizmo Drawing

    private void OnDrawGizmos()
    {
        if (!showChunks || chunks == null || chunks.Count == 0) return;

        foreach (var chunk in chunks)
        {
            DrawChunkGizmo(chunk);
        }
    }

    private void DrawChunkGizmo(Chunk chunk)
    {
        Vector2Int chunkCoord = chunk.chunkCoord;

        Vector3 chunkOrigin = gridOrigin + new Vector3(
            chunkCoord.x * chunkSizeX * cellSize, 0,
            chunkCoord.y * chunkSizeY * cellSize);

        Vector3 size = new Vector3(chunkSizeX * cellSize, 0.1f, chunkSizeY * cellSize);
        Vector3 center = chunkOrigin + size / 2f;

        Gizmos.color = new Color(chunkGizmoColor.r, chunkGizmoColor.g, chunkGizmoColor.b, 0.1f);
        Gizmos.DrawCube(center, size * 0.98f);

        Gizmos.color = chunkGizmoColor;
        Gizmos.DrawWireCube(center, size);

#if UNITY_EDITOR
        GUIStyle labelStyle = new GUIStyle
        {
            normal = new GUIStyleState { textColor = Color.white },
            fontSize = 20,
            alignment = TextAnchor.MiddleCenter
        };

        Handles.Label(center + Vector3.up * 0.5f, $"Chunk {chunkCoord.x},{chunkCoord.y}", labelStyle);
#endif
    }

    #endregion
}