// using System;
// using System.Collections.Generic;
// using UnityEngine;
//
// public class FloorMeshBaker : MonoBehaviour
// {
//     public static FloorMeshBaker Instance { get; private set; }
//     private string bakeGameObjectName = "BakedFloorMesh";
//     [SerializeField] private GameObject bakedFloorMeshPrefab;
//     [SerializeField] private Material floorMaterial;
//     [SerializeField] private bool useCombinedMesh = true;
//     private Dictionary<BuildID, FloorMeshBakerSettings> floorMeshBakerSettings = new Dictionary<BuildID, FloorMeshBakerSettings>();
//     // Track floor meshes by position
//     private Dictionary<Vector2Int, GameObject> floorMeshes = new Dictionary<Vector2Int, GameObject>();
//     private GameObject combinedMeshObject;
//     private bool needsRebuild = false;
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
//     private void Start()
//     {
//         GridSystem.Instance.OnGridCellChanged += OnGridCellChanged;
//         combinedMeshObject = new GameObject("CombinedFloorMesh");
//         combinedMeshObject.AddComponent<MeshFilter>();
//         combinedMeshObject.AddComponent<MeshRenderer>().material = floorMaterial;
//     }
//
//     private void Update()
//     {
//         if (needsRebuild && useCombinedMesh)
//         {
//             RebuildCombinedMesh();
//             needsRebuild = false;
//         }
//     }
//
//     private void OnDestroy()
//     {
//         GridSystem.Instance.OnGridCellChanged -= OnGridCellChanged;
//     }
//
//     private void OnGridCellChanged(Vector2Int gridPosition, bool isOccupied)
//     {
//         if (isOccupied)
//         {
//             BakeFloorMeshAtGridPosition(gridPosition);
//         }
//         else
//         {
//             RemoveBakedMeshAtGridPosition(gridPosition);
//         }
//     }
//
//     public void CreateFloorMesh(Vector2Int gridPosition)
//     {
//         Vector3 worldPosition = GridSystemExtension.GridToWorldPosition(gridPosition, GridSystem.Instance.gridOrigin,
//             GridSystem.Instance.cellSize);
//         GameObject bakedMesh = Instantiate(bakedFloorMeshPrefab, worldPosition, Quaternion.identity);
//         bakedMesh.name = $"{bakeGameObjectName}_{gridPosition.x}_{gridPosition.y}";
//         bakedMesh.GetComponentInChildren<MeshRenderer>().material = floorMaterial;
//         
//         // Store reference to this mesh
//         floorMeshes[gridPosition] = bakedMesh;
//         
//         // Hide individual mesh if using combined mesh
//         if (useCombinedMesh)
//         {
//             bakedMesh.GetComponentInChildren<MeshRenderer>().enabled = false;
//             needsRebuild = true;
//         }
//     }
//
//     private void BakeFloorMeshAtGridPosition(Vector2Int gridPosition)
//     {
//         if (floorMeshes.ContainsKey(gridPosition))
//             return;
//
//         floorMeshes[gridPosition] = null; // Placeholder for tracking
//         needsRebuild = true;
//     }
//
//
//     private void RemoveBakedMeshAtGridPosition(Vector2Int gridPosition)
//     {
//         if (floorMeshes.TryGetValue(gridPosition, out GameObject existingMesh))
//         {
//             Destroy(existingMesh);
//             floorMeshes.Remove(gridPosition);
//             
//             if (useCombinedMesh)
//                 needsRebuild = true;
//         }
//     }
//     
//    private void RebuildCombinedMesh()
// {
//     if (floorMeshes.Count == 0)
//     {
//         combinedMeshObject.GetComponent<MeshFilter>().mesh = null;
//         return;
//     }
//
//     List<CombineInstance> combines = new List<CombineInstance>();
//
//     // Get mesh and transform info from prefab child
//     MeshFilter prefabMeshFilter = bakedFloorMeshPrefab.GetComponentInChildren<MeshFilter>();
//     if (prefabMeshFilter == null || prefabMeshFilter.sharedMesh == null)
//     {
//         Debug.LogError("Prefab mesh filter or mesh is missing.");
//         return;
//     }
//
//     Mesh meshToUse = prefabMeshFilter.sharedMesh;
//     Transform childTransform = prefabMeshFilter.transform;
//     Transform rootTransform = bakedFloorMeshPrefab.transform;
//
//     // Calculate relative offset from root to child
//     Matrix4x4 childOffset = Matrix4x4.TRS(
//         childTransform.localPosition,
//         childTransform.localRotation,
//         childTransform.localScale
//     );
//
//     foreach (var kvp in floorMeshes)
//     {
//         Vector2Int gridPos = kvp.Key;
//
//         Vector3 worldPosition = GridSystemExtension.GridToWorldPosition(
//             gridPos, GridSystem.Instance.gridOrigin, GridSystem.Instance.cellSize);
//
//         // World transform of the root object
//         Matrix4x4 rootTransformMatrix = Matrix4x4.TRS(
//             worldPosition,
//             rootTransform.rotation,   // Use prefab root rotation
//             rootTransform.localScale  // Use prefab root scale
//         );
//
//         CombineInstance ci = new CombineInstance
//         {
//             mesh = meshToUse,
//             transform = rootTransformMatrix * childOffset
//         };
//
//         combines.Add(ci);
//     }
//
//     Mesh combinedMesh = new Mesh();
//     combinedMesh.CombineMeshes(combines.ToArray(), true);
//
//     combinedMeshObject.GetComponent<MeshFilter>().mesh = combinedMesh;
//     combinedMeshObject.GetComponent<MeshRenderer>().material = floorMaterial;
//
//     // Optional: collider
//     MeshCollider collider = combinedMeshObject.GetComponent<MeshCollider>();
//     if (collider == null) collider = combinedMeshObject.AddComponent<MeshCollider>();
//     collider.sharedMesh = combinedMesh;
// }
//
//
//     
//     // Toggle between combined and individual meshes
//     public void SetUseCombinedMesh(bool useCombined)
//     {
//         if (useCombinedMesh == useCombined) return;
//         
//         useCombinedMesh = useCombined;
//         
//         foreach (var floorMesh in floorMeshes.Values)
//         {
//             if (floorMesh != null)
//                 floorMesh.GetComponentInChildren<MeshRenderer>().enabled = !useCombined;
//         }
//         
//         combinedMeshObject.GetComponent<MeshRenderer>().enabled = useCombined;
//         
//         if (useCombined)
//             needsRebuild = true;
//     }
// }
// [Serializable]
// public class FloorMeshBakerSettings
// {
//     public GameObject bakedFloorMeshPrefab;
//     public Material floorMaterial;
//     public bool useCombinedMesh = true;
//
//     public FloorMeshBakerSettings(GameObject prefab, Material material, bool combined)
//     {
//         bakedFloorMeshPrefab = prefab;
//         floorMaterial = material;
//         useCombinedMesh = combined;
//     }
// }