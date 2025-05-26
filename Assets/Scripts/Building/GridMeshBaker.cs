// using System.Collections.Generic;
// using UnityEngine;
//
// public class GridMeshBaker : MonoBehaviour
// {
//     public static GridMeshBaker Instance { get; private set; }
//
//     [Header("Mesh Settings")]
//     [SerializeField] private Material combinedMeshMaterial;
//     [SerializeField] private bool includeFloors = true;
//     [SerializeField] private float meshOffset = 0.01f; // Small offset to prevent z-fighting
//
//     private List<GameObject> bakedMeshObjects = new List<GameObject>();
//
//     private void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }
//
//     [ContextMenu("Bake")]
//   public void BakeMesh()
// {
//     ClearBakedMeshes();
//
//     Floor[] floors = GetFloorsFromGridSystem();
//     if (floors == null || floors.Length == 0)
//     {
//         Debug.LogWarning("No floors found in GridSystem to bake.");
//         return;
//     }
//
//     List<CombineInstance> globalFloorCombines = new List<CombineInstance>();
//     List<CombineInstance> globalWallCombines = new List<CombineInstance>();
//
//     foreach (Floor floor in floors)
//     {
//         if (floor == null)
//             continue;
//
//         // Add floor mesh
//         if (includeFloors)
//         {
//             MeshFilter floorMeshFilter = floor.GetComponentInChildren<MeshFilter>();
//             if (floorMeshFilter != null && floorMeshFilter.sharedMesh != null)
//             {
//                 Matrix4x4 offsetMatrix = floorMeshFilter.transform.localToWorldMatrix;
//                 Vector3 offsetPosition = floorMeshFilter.transform.position + (Vector3.up * meshOffset);
//                 offsetMatrix.SetColumn(3, new Vector4(offsetPosition.x, offsetPosition.y, offsetPosition.z, 1));
//
//                 globalFloorCombines.Add(new CombineInstance
//                 {
//                     mesh = floorMeshFilter.sharedMesh,
//                     transform = offsetMatrix
//                 });
//             }
//         }
//     }
//
//     if (globalFloorCombines.Count > 0)
//         CreateCombinedMesh(globalFloorCombines, "BakedFloors");
//
//     if (globalWallCombines.Count > 0)
//         CreateCombinedMesh(globalWallCombines, "BakedWalls");
//
//     Debug.Log($"Optimized baking complete. Combined into {bakedMeshObjects.Count} meshes.");
// }
//
//
//     private Floor[] GetFloorsFromGridSystem()
//     {
//         if (GridSystem.Instance == null)
//         {
//             Debug.LogError("GridSystem.Instance is null. Cannot get floors.");
//             return null;
//         }
//
//         // Use reflection to access the private floors array in GridSystem
//         System.Reflection.FieldInfo floorsField = typeof(GridSystem).GetField("floors", 
//             System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//             
//         if (floorsField == null)
//         {
//             Debug.LogError("Could not find 'floors' field in GridSystem via reflection.");
//             return null;
//         }
//
//         return (Floor[])floorsField.GetValue(GridSystem.Instance);
//     }
//
//     private List<List<Floor>> FindConnectedFloorGroups(Floor[] allFloors)
//     {
//         List<List<Floor>> floorGroups = new List<List<Floor>>();
//         HashSet<Floor> visitedFloors = new HashSet<Floor>();
//
//         foreach (Floor floor in allFloors)
//         {
//             // Skip null or already visited floors
//             if (floor == null || visitedFloors.Contains(floor))
//                 continue;
//
//             // Start a new group with this floor
//             List<Floor> currentGroup = new List<Floor>();
//             Queue<Floor> floorQueue = new Queue<Floor>();
//             
//             floorQueue.Enqueue(floor);
//             visitedFloors.Add(floor);
//
//             // BFS to find all connected floors
//             while (floorQueue.Count > 0)
//             {
//                 Floor currentFloor = floorQueue.Dequeue();
//                 currentGroup.Add(currentFloor);
//
//                 // Check all four directions for connected floors
//                 CheckAndEnqueueConnectedFloor(currentFloor, Direction.Top, floorQueue, visitedFloors);
//                 CheckAndEnqueueConnectedFloor(currentFloor, Direction.Right, floorQueue, visitedFloors);
//                 CheckAndEnqueueConnectedFloor(currentFloor, Direction.Bot, floorQueue, visitedFloors);
//                 CheckAndEnqueueConnectedFloor(currentFloor, Direction.Left, floorQueue, visitedFloors);
//             }
//
//             floorGroups.Add(currentGroup);
//         }
//
//         return floorGroups;
//     }
//
//     private void CheckAndEnqueueConnectedFloor(Floor currentFloor, Direction direction, Queue<Floor> queue, HashSet<Floor> visited)
//     {
//         // Get the grid position of the current floor
//         Vector2Int gridPosition = GridSystem.Instance.WorldToGridPosition(currentFloor.transform.position);
//         Vector2Int neighborPos = gridPosition;
//
//         // Determine the position of the adjacent floor based on direction
//         switch (direction)
//         {
//             case Direction.Top: neighborPos += Vector2Int.up; break;
//             case Direction.Right: neighborPos += Vector2Int.right; break;
//             case Direction.Bot: neighborPos += Vector2Int.down; break;
//             case Direction.Left: neighborPos += Vector2Int.left; break;
//         }
//
//         // Check if the position is valid
//         if (!GridSystemExtension.IsValidGridPosition(neighborPos, GridSystem.Instance.gridWidth, GridSystem.Instance.gridHeight))
//             return;
//
//         // Get the index in the floors array
//         int index = neighborPos.x + neighborPos.y * GridSystem.Instance.gridWidth;
//         Floor[] floors = GetFloorsFromGridSystem();
//
//         // Check if there's a floor and it hasn't been visited
//         if (index >= 0 && index < floors.Length && floors[index] != null && !visited.Contains(floors[index]))
//         {
//             queue.Enqueue(floors[index]);
//             visited.Add(floors[index]);
//         }
//     }
//
// private void BakeFloorGroup(List<Floor> floorGroup)
// {
//     // Create separate lists for floor meshes and wall meshes
//     List<CombineInstance> floorCombineInstances = new List<CombineInstance>();
//     List<CombineInstance> wallCombineInstances = new List<CombineInstance>();
//
//     // Collect all meshes
//     foreach (Floor floor in floorGroup)
//     {
//         // Add floor mesh
//         if (includeFloors)
//         {
//             MeshFilter floorMeshFilter = floor.GetComponentInChildren<MeshFilter>();
//             if (floorMeshFilter != null && floorMeshFilter.sharedMesh != null)
//             {
//                 // Create a modified transform matrix that includes the offset
//                 Matrix4x4 offsetMatrix = floorMeshFilter.transform.localToWorldMatrix;
//                 Vector3 offsetPosition = floorMeshFilter.transform.position + (Vector3.up * meshOffset);
//                 offsetMatrix.SetColumn(3, new Vector4(offsetPosition.x, offsetPosition.y, offsetPosition.z, 1));
//
//                 CombineInstance ci = new CombineInstance
//                 {
//                     mesh = floorMeshFilter.sharedMesh,
//                     transform = offsetMatrix
//                 };
//                 floorCombineInstances.Add(ci);
//             }
//         }
//     }
//
//     // Create and combine floor meshes
//     if (floorCombineInstances.Count > 0)
//     {
//         CreateCombinedMesh(floorCombineInstances, "BakedFloors");
//     }
//
//     // Create and combine wall meshes
//     if (wallCombineInstances.Count > 0)
//     {
//         CreateCombinedMesh(wallCombineInstances, "BakedWalls");
//     }
// }
// private void CreateCombinedMesh(List<CombineInstance> combineInstances, string name)
// {
//     // Create a new game object for the combined mesh
//     GameObject combinedObject = new GameObject(name);
//     combinedObject.transform.SetParent(transform);
//     combinedObject.transform.localPosition = Vector3.zero;
//
//     // Add mesh components
//     MeshFilter meshFilter = combinedObject.AddComponent<MeshFilter>();
//     MeshRenderer meshRenderer = combinedObject.AddComponent<MeshRenderer>();
//
//     // Create the combined mesh
//     Mesh combinedMesh = new Mesh();
//     combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // Support larger meshes
//     combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);
//     
//     // Optimize the mesh
//     combinedMesh.Optimize();
//     combinedMesh.RecalculateNormals();
//     combinedMesh.RecalculateBounds();
//
//     // Assign the mesh
//     meshFilter.sharedMesh = combinedMesh;
//
//     // Add a material
//     meshRenderer.material = combinedMeshMaterial != null ?
//         combinedMeshMaterial : new Material(Shader.Find("Standard"));
//
//     // Add a mesh collider for physics interactions
//     MeshCollider meshCollider = combinedObject.AddComponent<MeshCollider>();
//     meshCollider.sharedMesh = combinedMesh;
//
//     // Keep track of created objects
//     bakedMeshObjects.Add(combinedObject);
//     
//     Debug.Log($"Combined mesh {name} has {combinedMesh.triangles.Length/3} triangles");
// }
//     private void ClearBakedMeshes()
//     {
//         foreach (GameObject obj in bakedMeshObjects)
//         {
//             if (obj != null)
//             {
//                 DestroyImmediate(obj);
//             }
//         }
//         bakedMeshObjects.Clear();
//     }
//
// }

using System.Collections.Generic;
using UnityEngine;

public class GridMeshBaker : MonoBehaviour
{
    public static GridMeshBaker Instance { get; private set; }

    [Header("Mesh Settings")]
    [SerializeField] private Material combinedMeshMaterial;
    [SerializeField] private bool includeFloors = true;
    [SerializeField] private float meshOffset = 0.01f; // Small offset to prevent z-fighting
    [SerializeField] private bool autoRebake = true;

    private List<GameObject> bakedMeshObjects = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Subscribe to events in GridSystem for auto-rebaking
        if (autoRebake && GridSystem.Instance != null)
        {
            GridSystem.Instance.OnBuildingPlaced += HandleBuildingChanged;
            GridSystem.Instance.OnBuildingRemoved += HandleBuildingChanged;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events when destroyed
        if (GridSystem.Instance != null)
        {
            GridSystem.Instance.OnBuildingPlaced -= HandleBuildingChanged;
            GridSystem.Instance.OnBuildingRemoved -= HandleBuildingChanged;
        }
    }

    private void HandleBuildingChanged(Building building)
    {
        // Auto-rebake when buildings are placed or removed
        if (autoRebake && gameObject.activeInHierarchy)
        {
            BakeMesh();
        }
    }

    [ContextMenu("Bake")]
    public void BakeMesh()
    {
        ClearBakedMeshes();

        Floor[] floors = GetFloorsFromGridSystem();
        if (floors == null || floors.Length == 0)
        {
            Debug.LogWarning("No floors found in GridSystem to bake.");
            return;
        }

        List<CombineInstance> globalFloorCombines = new List<CombineInstance>();
        List<CombineInstance> globalWallCombines = new List<CombineInstance>();

        foreach (Floor floor in floors)
        {
            if (floor == null)
                continue;

            // Add floor mesh
            if (includeFloors)
            {
                MeshFilter floorMeshFilter = floor.GetComponentInChildren<MeshFilter>();
                if (floorMeshFilter != null && floorMeshFilter.sharedMesh != null)
                {
                    Matrix4x4 offsetMatrix = floorMeshFilter.transform.localToWorldMatrix;
                    Vector3 offsetPosition = floorMeshFilter.transform.position + (Vector3.up * meshOffset);
                    offsetMatrix.SetColumn(3, new Vector4(offsetPosition.x, offsetPosition.y, offsetPosition.z, 1));

                    globalFloorCombines.Add(new CombineInstance
                    {
                        mesh = floorMeshFilter.sharedMesh,
                        transform = offsetMatrix
                    });
                }
            }

            // Process walls attached to this floor
            if (floor.attachedWalls != null)
            {
                foreach (Wall wall in floor.attachedWalls)
                {
                    if (wall == null) continue;

                    MeshFilter wallMeshFilter = wall.GetComponentInChildren<MeshFilter>();
                    if (wallMeshFilter != null && wallMeshFilter.sharedMesh != null)
                    {
                        Matrix4x4 offsetMatrix = wallMeshFilter.transform.localToWorldMatrix;
                        Vector3 offsetPosition = wallMeshFilter.transform.position + (Vector3.up * meshOffset);
                        offsetMatrix.SetColumn(3, new Vector4(offsetPosition.x, offsetPosition.y, offsetPosition.z, 1));

                        globalWallCombines.Add(new CombineInstance
                        {
                            mesh = wallMeshFilter.sharedMesh,
                            transform = offsetMatrix
                        });
                    }
                }
            }
        }

        if (globalFloorCombines.Count > 0)
            CreateCombinedMesh(globalFloorCombines, "BakedFloors");

        if (globalWallCombines.Count > 0)
            CreateCombinedMesh(globalWallCombines, "BakedWalls");

        Debug.Log($"Optimized baking complete. Combined into {bakedMeshObjects.Count} meshes.");
    }

    private Floor[] GetFloorsFromGridSystem()
    {
        return GridSystem.Instance.floors;
    }

    private List<List<Floor>> FindConnectedFloorGroups(Floor[] allFloors)
    {
        List<List<Floor>> floorGroups = new List<List<Floor>>();
        HashSet<Floor> visitedFloors = new HashSet<Floor>();

        foreach (Floor floor in allFloors)
        {
            // Skip null or already visited floors
            if (floor == null || visitedFloors.Contains(floor))
                continue;

            // Start a new group with this floor
            List<Floor> currentGroup = new List<Floor>();
            Queue<Floor> floorQueue = new Queue<Floor>();

            floorQueue.Enqueue(floor);
            visitedFloors.Add(floor);

            // BFS to find all connected floors
            while (floorQueue.Count > 0)
            {
                Floor currentFloor = floorQueue.Dequeue();
                currentGroup.Add(currentFloor);

                // Check all four directions for connected floors
                CheckAndEnqueueConnectedFloor(currentFloor, Direction.Top, floorQueue, visitedFloors);
                CheckAndEnqueueConnectedFloor(currentFloor, Direction.Right, floorQueue, visitedFloors);
                CheckAndEnqueueConnectedFloor(currentFloor, Direction.Bot, floorQueue, visitedFloors);
                CheckAndEnqueueConnectedFloor(currentFloor, Direction.Left, floorQueue, visitedFloors);
            }

            floorGroups.Add(currentGroup);
        }

        return floorGroups;
    }

    private void CheckAndEnqueueConnectedFloor(Floor currentFloor, Direction direction, Queue<Floor> queue, HashSet<Floor> visited)
    {
        // Get the grid position of the current floor
        Vector2Int gridPosition = GridSystem.Instance.WorldToGridPosition(currentFloor.transform.position);
        Vector2Int neighborPos = gridPosition;

        // Determine the position of the adjacent floor based on direction
        switch (direction)
        {
            case Direction.Top: neighborPos += Vector2Int.up; break;
            case Direction.Right: neighborPos += Vector2Int.right; break;
            case Direction.Bot: neighborPos += Vector2Int.down; break;
            case Direction.Left: neighborPos += Vector2Int.left; break;
        }

        // Check if the position is valid
        if (!GridSystemExtension.IsValidGridPosition(neighborPos, GridSystem.Instance.gridWidth, GridSystem.Instance.gridHeight))
            return;

        // Get the index in the floors array
        int index = neighborPos.x + neighborPos.y * GridSystem.Instance.gridWidth;
        Floor[] floors = GridSystem.Instance.floors;

        // Check if there's a floor and it hasn't been visited
        if (index >= 0 && index < floors.Length && floors[index] != null && !visited.Contains(floors[index]))
        {
            queue.Enqueue(floors[index]);
            visited.Add(floors[index]);
        }
    }

    private void BakeFloorGroup(List<Floor> floorGroup)
    {
        // Create separate lists for floor meshes and wall meshes
        List<CombineInstance> floorCombineInstances = new List<CombineInstance>();
        List<CombineInstance> wallCombineInstances = new List<CombineInstance>();

        // Collect all meshes
        foreach (Floor floor in floorGroup)
        {
            // Add floor mesh
            if (includeFloors)
            {
                MeshFilter floorMeshFilter = floor.GetComponentInChildren<MeshFilter>();
                if (floorMeshFilter != null && floorMeshFilter.sharedMesh != null)
                {
                    // Create a modified transform matrix that includes the offset
                    Matrix4x4 offsetMatrix = floorMeshFilter.transform.localToWorldMatrix;
                    Vector3 offsetPosition = floorMeshFilter.transform.position + (Vector3.up * meshOffset);
                    offsetMatrix.SetColumn(3, new Vector4(offsetPosition.x, offsetPosition.y, offsetPosition.z, 1));

                    CombineInstance ci = new CombineInstance
                    {
                        mesh = floorMeshFilter.sharedMesh,
                        transform = offsetMatrix
                    };
                    floorCombineInstances.Add(ci);
                }
            }
        }

        // Create and combine floor meshes
        if (floorCombineInstances.Count > 0)
        {
            CreateCombinedMesh(floorCombineInstances, "BakedFloors");
        }

        // Create and combine wall meshes
        if (wallCombineInstances.Count > 0)
        {
            CreateCombinedMesh(wallCombineInstances, "BakedWalls");
        }
    }

    private void CreateCombinedMesh(List<CombineInstance> combineInstances, string name)
    {
        // Create a new game object for the combined mesh
        GameObject combinedObject = new GameObject(name);
        combinedObject.transform.SetParent(transform);
        combinedObject.transform.localPosition = Vector3.zero;

        // Add mesh components
        MeshFilter meshFilter = combinedObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = combinedObject.AddComponent<MeshRenderer>();

        // Create the combined mesh
        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // Support larger meshes
        combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);

        // Optimize the mesh
        combinedMesh.Optimize();
        combinedMesh.RecalculateNormals();
        combinedMesh.RecalculateBounds();

        // Assign the mesh
        meshFilter.sharedMesh = combinedMesh;

        // Add a material
        meshRenderer.material = combinedMeshMaterial != null ?
            combinedMeshMaterial : new Material(Shader.Find("Standard"));

        // Add a mesh collider for physics interactions
        MeshCollider meshCollider = combinedObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = combinedMesh;

        // Keep track of created objects
        bakedMeshObjects.Add(combinedObject);

        Debug.Log($"Combined mesh {name} has {combinedMesh.triangles.Length/3} triangles");
    }

    private void ClearBakedMeshes()
    {
        foreach (GameObject obj in bakedMeshObjects)
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        bakedMeshObjects.Clear();
    }
}