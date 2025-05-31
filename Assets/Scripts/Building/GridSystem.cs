using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
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
    }


    private void OnEditMode(bool b)
    {
        if (b)
        {
            foreach (var chunk in chunks)
            {
                if (chunk.floorsParent != null)
                {
                    foreach (Transform child in chunk.floorsParent.transform)
                    {
                        Destroy(child.gameObject);
                    }
                }

                if (chunk.wallsParents != null)
                {
                    foreach (Transform child in chunk.wallsParents.transform)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }

            RecreateWorld();
        }
        else
        {
            HandleBakeMesh();
        }
    }

    private void OnDestroy()
    {
        BuildingManager.Instance.OnDone -= HandleBakeMesh;
        UIBuilding.Instance.SwitchEditMode -= OnEditMode;
    }

    #endregion

    #region Properties and Fields

    [Header("Grid Settings")] public int gridWidth = 50;
    public int gridHeight = 50;
    public float cellSize = 1f;
    public Vector3 gridOrigin = Vector3.zero;

    [Header("Chunk Settings")] [Header("Chunk Settings")]
    public int chunkCountX = 3;

    public int chunkCountY = 3;
    private int chunkSizeX;

    private int chunkSizeY;

    // private Dictionary<Vector2Int, Chunk> chunks = new();
    private Dictionary<Vector2Int, Chunk> chunkDictionary = new Dictionary<Vector2Int, Chunk>();


    public List<Chunk> chunks = new();
    [Header("Chunk Gizmos")] public bool showChunks = true;
    public Color chunkGizmoColor = new Color(0, 0.5f, 1f, 0.3f);
    public event Action<Vector2Int, bool> OnGridCellChanged;

    #endregion

    private void InitializeGrid()
    {
        chunkSizeX = Mathf.CeilToInt((float)gridWidth / chunkCountX);
        chunkSizeY = Mathf.CeilToInt((float)gridHeight / chunkCountY);
        chunks.Clear();

        for (int y = 0; y < chunkCountY; y++)
        {
            for (int x = 0; x < chunkCountX; x++)
            {
                Vector2Int chunkCoord = new Vector2Int(x, y);
                Chunk newChunk = new Chunk(chunkCoord, chunkSizeX, chunkSizeY);
                chunks.Add(newChunk);
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


    public Floor GetFloorAt(Vector2Int gridPosition)
    {
        if (TryGetChunk(gridPosition, out Chunk chunk, out Vector2Int localPos))
        {
            return chunk.GetCell(localPos);
        }

        return null;
    }

    public void SetFloorAt(Vector2Int gridPosition, Floor floor, GameObject g)
    {
        if (TryGetChunk(gridPosition, out Chunk chunk, out Vector2Int localPos))
        {
            chunk.SetCell(localPos, floor, g);
            OnGridCellChanged?.Invoke(gridPosition, floor != null);
        }
    }

    public bool CanPlaceObject(Vector2Int gridPosition)
    {
        return !IsCellOccupied(gridPosition);
    }

    public Vector3 GridPosToWorldPosition(Vector2Int gridPosition)
    {
        return gridOrigin + new Vector3(gridPosition.x * cellSize, 0, gridPosition.y * cellSize);
    }

    public Vector2Int IndexToGridPosition(int index)
    {
        if (index < 0 || index >= gridWidth * gridHeight)
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of bounds of the grid.");

        int x = index % gridWidth;
        int y = index / gridWidth;
        return new Vector2Int(x, y);
    }


    public void MarkGridCells(Vector2Int gridPosition, Vector2Int gridSize, bool isOccupied)
    {
        // This is now deprecated but kept for compatibility
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector2Int cellPos = gridPosition + new Vector2Int(x, y);
                if (TryGetChunk(cellPos, out Chunk chunk, out Vector2Int localPos))
                {
                    // if (!isOccupied)
                    // {
                    //     chunk.SetCell(localPos, null);
                    // }
                }
            }
        }
    }

    #endregion

    #region Grid Coordinate Conversion

    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        //debug which chunk is being used
        Vector3 localPosition = worldPosition - gridOrigin;
        int x = Mathf.FloorToInt(localPosition.x / cellSize);
        int z = Mathf.FloorToInt(localPosition.z / cellSize);
        return new Vector2Int(x, z);
    }

    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        return GridSystemExtension.GridToWorldPosition(gridPosition, gridOrigin, cellSize);
    }

    public bool IsValidGridPosition(Vector2Int gridPosition)
    {
        return GridSystemExtension.IsValidGridPosition(gridPosition, gridWidth, gridHeight);
    }

    public void SetWallWithDirection(Vector2Int gridPosition, Direction direction, GameObject g,BuildID id)
    {
        if (TryGetChunk(gridPosition, out Chunk chunk, out Vector2Int localPos))
        {
            chunk.SetWallWithDirection(localPos, direction, g,id);
        }
    }


    private bool TryGetChunk(Vector2Int gridPos, out Chunk chunk, out Vector2Int localPos) //list instead of dictionary
    {
        if (!IsValidGridPosition(gridPos))
        {
            chunk = null;
            localPos = Vector2Int.zero;
            return false;
        }

        Vector2Int chunkCoord = new Vector2Int(gridPos.x / chunkSizeX, gridPos.y / chunkSizeY);
        localPos = new Vector2Int(gridPos.x % chunkSizeX, gridPos.y % chunkSizeY);

        int index = GetChunkIndex(chunkCoord);
        if (index >= 0 && index < chunks.Count)
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

        return chunkCoord.y * chunkCountX + chunkCoord.x;
    }


    private void OnDrawGizmos()
    {
        if (!showChunks || chunks == null) return;
        foreach (var chunk in chunks)
        {
            Vector2Int chunkCoord = chunk.chunkCoord;

            Vector3 chunkOrigin = gridOrigin + new Vector3(chunkCoord.x * chunkSizeX * cellSize, 0,
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
    }

    #endregion

    #region Recreate GameObject Base On Chunk Data

    [ContextMenu("Re Create")]
    public void RecreateWorld()
    {
        foreach (Chunk chunk in chunks)
        {
            if (chunk.floorsParent == null)
            {
                chunk.floorsParent = new GameObject($"Floor_{chunk.chunkCoord.x}_{chunk.chunkCoord.y}");
                chunk.floorsParent.transform.SetParent(transform);
            }

            foreach (Transform child in chunk.floorsParent.transform)
            {
                Destroy(child.gameObject);
            }

            // Loop through all cells in the chunk
            for (int x = 0; x < chunkSizeX; x++)
            {
                for (int y = 0; y < chunkSizeY; y++)
                {
                    Vector2Int localPos = new Vector2Int(x, y);
                    Floor floor = chunk.GetCell(localPos);


                    if (floor != null && floor.buildID != BuildID.None)
                    {
                        Vector2Int globalPos = chunk.chunkCoord * new Vector2Int(chunkSizeX, chunkSizeY) + localPos;
                        Vector3 worldPos = GridToWorldPosition(globalPos);
                        GameObject floorObj =
                            Instantiate(GameManager.Instance.GetBuildingDataByID(floor.buildID).prefab, worldPos,
                                Quaternion.identity);
                        EditBuilding editBuilding = floorObj.AddComponent<EditBuilding>();
                        editBuilding.SetBuildID(floor.buildID);
                        floorObj.transform.SetParent(chunk.floorsParent.transform);

                        for (int dirInt = 0; dirInt < 4; dirInt++)
                        {
                            Direction direction = (Direction)(1 << dirInt); // Convert to actual Direction enum values
                            if (floor.IsHaveWallAtDirection(direction))
                            {
                                Vector3 offset = CalculateOffsetAndRotate(direction, out float rotate);
                                GameObject wallObj = Instantiate(
                                    GameManager.Instance.GetBuildingDataByID(BuildID.Wall).prefab,
                                    worldPos + offset, // Position adjusted with offset
                                    Quaternion.Euler(0, rotate, 0), // Rotation based on direction
                                    chunk.floorsParent.transform); // Parent to the chunk
                                wallObj.transform.SetParent(chunk.wallsParents.transform);
                                EditBuilding e = wallObj.AddComponent<EditBuilding>();
                                e.SetBuildID(BuildID.Wall);
                            }
                        }
                    }
                }
            }
        }

        Debug.Log("World recreation completed.");
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

    private void HandleBakeMesh()
    {
        for (int i = 0; i < chunks.Count; i++)
        {
            // Process floors
            if (chunks[i].floorsParent != null)
            {
                if (chunks[i].NeedRebuild)
                {
                    CombineMeshes(chunks[i].floorsParent);
                    CombineMeshes(chunks[i].wallsParents);
                }
                else
                {
                    Debug.Log($"Chunk {chunks[i].chunkCoord} does not need rebuild.");
                }

                chunks[i].NeedRebuild = false;
            }
        }
    }

    private void CombineMeshes(GameObject parent)
    {
        if (parent == null)
        {
            return;
        }

        // Get all child mesh filters
        MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>();
        if (meshFilters.Length == 0)
        {
            return;
        }

        Material material = null;
        foreach (MeshRenderer renderer in parent.GetComponentsInChildren<MeshRenderer>())
        {
            if (renderer.sharedMaterial != null)
            {
                material = renderer.sharedMaterial;
                break;
            }
        }

        if (material == null)
        {
            return;
        }

        List<CombineInstance> validInstances = new List<CombineInstance>();

        // Fill the combine instances
        foreach (MeshFilter mf in meshFilters)
        {
            if (mf.sharedMesh == null)
                continue;

            CombineInstance instance = new CombineInstance
            {
                mesh = mf.sharedMesh,
                transform = mf.transform.localToWorldMatrix
            };
            validInstances.Add(instance);
        }

        if (validInstances.Count == 0)
        {
            return;
        }

        // Create new GameObject inside parent
        GameObject combinedObject = new GameObject("CombinedMesh");
        combinedObject.transform.SetParent(parent.transform);
        combinedObject.transform.localPosition = Vector3.zero;
        combinedObject.transform.localRotation = Quaternion.identity;
        combinedObject.transform.localScale = Vector3.one;

        MeshFilter combinedMeshFilter = combinedObject.AddComponent<MeshFilter>();
        MeshRenderer combinedRenderer = combinedObject.AddComponent<MeshRenderer>();

        // Combine meshes
        Mesh combinedMesh = new Mesh
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
        combinedMesh.CombineMeshes(validInstances.ToArray(), true, true);

        if (combinedMesh.vertexCount == 0)
        {
            return;
        }

        combinedMeshFilter.sharedMesh = combinedMesh;
        combinedRenderer.sharedMaterial = material;
        //cast shadow off, receive shadow off
        combinedRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        combinedRenderer.receiveShadows = false;

        // Destroy old children
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

        // Add MeshCollider
        MeshCollider collider = combinedObject.AddComponent<MeshCollider>();
        collider.sharedMesh = combinedMesh;
    }


    [Serializable]
    public class Chunk
    {
        public Vector2Int chunkCoord;
        public Floor[,] data;
        public Floor[] floors;
        public GameObject floorsParent;
        public GameObject wallsParents;
        public bool NeedRebuild = false;
        //mesh bake setting 

        public Chunk(Vector2Int coord, int sizeX, int sizeY)
        {
            floorsParent = new GameObject($"Floor_{coord.x}_{coord.y}");
            wallsParents = new GameObject($"Wall_{coord.x}_{coord.y}");
            chunkCoord = coord;
            data = new Floor[sizeX, sizeY];
            floors = new Floor[sizeX * sizeY];
        }

        public Floor GetCell(Vector2Int localPos)
        {
            return data[localPos.x, localPos.y];
        }

        public void SetCell(Vector2Int localPos, Floor floor, GameObject g)
        {
            data[localPos.x, localPos.y] = floor;
            int index = localPos.x + localPos.y * 5; //assuming chunk size is 5x5
            floors[index] = floor;
            g.transform.SetParent(floorsParent.transform);
            NeedRebuild = true;
        }

        public bool IsCellOccupied(Vector2Int localPos)
        {
            return data[localPos.x, localPos.y] != null;
        }

        public void SetWallWithDirection(Vector2Int localPos, Direction d, GameObject g,BuildID id)
        {
            if (data[localPos.x, localPos.y] == null)
            {
                Debug.LogWarning("Cannot set wall on an empty cell.");
                return;
            }

            data[localPos.x, localPos.y].SetWall(d,id);
            g.transform.SetParent(wallsParents.transform);
            NeedRebuild = true;
        }
    }
}