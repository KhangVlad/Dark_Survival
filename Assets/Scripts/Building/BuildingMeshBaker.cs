using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BuildingMeshBaker : MonoBehaviour
{
    public static BuildingMeshBaker Instance { get; private set; }
    
    [SerializeField] private List<BuildingMeshSettings> buildingMeshSettings = new List<BuildingMeshSettings>();
    
    private Dictionary<BuildID, BuildingMeshSettings> settingsLookup = new Dictionary<BuildID, BuildingMeshSettings>();
    
    private Dictionary<BuildID, Dictionary<Vector2Int, GameObject>> meshes = 
        new Dictionary<BuildID, Dictionary<Vector2Int, GameObject>>();
    
    // Combined mesh objects for each BuildID
    private Dictionary<BuildID, GameObject> combinedMeshObjects = new Dictionary<BuildID, GameObject>();
    
    
    // Track which types need rebuilding
    private HashSet<BuildID> needsRebuild = new HashSet<BuildID>();

    [SerializeField] private GameObject tempGameObject;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
            
        // Initialize settings lookup
        foreach (var setting in buildingMeshSettings)
        {
            settingsLookup[setting.buildingID] = setting;
            meshes[setting.buildingID] = new Dictionary<Vector2Int, GameObject>();
        }
    }

    private void Start()
    {
       BuildingManager.Instance.OnPlaceBuilding += CreateMesh;
       
    }

 
   
  [ContextMenu("Test Merge")]
public void Test()
{
    // Create a test GameObject if not provided
    if (tempGameObject == null)
    {
        Debug.LogError("No tempGameObject assigned for instancing test");
        return;
    }

    // Create a temporary container for test instances
    GameObject testContainer = new GameObject("TestInstances");
    testContainer.transform.parent = transform;

    // Get mesh data from template
    MeshFilter templateMeshFilter = tempGameObject.GetComponentInChildren<MeshFilter>();
    MeshRenderer templateRenderer = tempGameObject.GetComponentInChildren<MeshRenderer>();

    if (templateMeshFilter == null || templateMeshFilter.sharedMesh == null || templateRenderer == null)
    {
        Debug.LogError("Template object missing required components (MeshFilter or MeshRenderer)");
        Destroy(testContainer);
        return;
    }

    // Create list for combining
    List<CombineInstance> combines = new List<CombineInstance>(10000);
    Mesh meshToUse = templateMeshFilter.sharedMesh;
    Material materialToUse = templateRenderer.sharedMaterial;

    // Calculate grid size for a square layout
    int gridSize = 100; // 100x100 grid = 10,000 objects
    float spacing = 1.0f;

    Debug.Log("Creating 10,000 mesh instances...");
    
    // Generate 10,000 instances in a grid pattern
    for (int x = 0; x < gridSize; x++)
    {
        for (int z = 0; z < gridSize; z++)
        {
            // Create random variations
            float yRotation = UnityEngine.Random.Range(0f, 360f);
            float yOffset = UnityEngine.Random.Range(-0.05f, 0.05f);
            float scaleVariation = UnityEngine.Random.Range(0.9f, 1.1f);
            
            // Position in grid with slight random offset
            Vector3 position = new Vector3(
                x * spacing + UnityEngine.Random.Range(-0.1f, 0.1f),
                yOffset,
                z * spacing + UnityEngine.Random.Range(-0.1f, 0.1f)
            );
            
            // Create transform matrix
            Matrix4x4 matrix = Matrix4x4.TRS(
                position,
                Quaternion.Euler(0, yRotation, 0),
                Vector3.one * scaleVariation
            );
            
            // Add to combines list
            CombineInstance ci = new CombineInstance
            {
                mesh = meshToUse,
                transform = matrix
            };
            combines.Add(ci);
        }
    }
    
    Debug.Log($"Created {combines.Count} mesh instances, baking combined mesh...");
    
    // Create combined mesh
    Mesh combinedMesh = new Mesh();
    combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    
    // Batch combine for very large meshes (to avoid timeout)
    int batchSize = 1000;
    int totalBatches = Mathf.CeilToInt(combines.Count / (float)batchSize);
    
    GameObject combinedObject = new GameObject("Combined_10000_Instances");
    combinedObject.transform.parent = transform;
    
    for (int batchIndex = 0; batchIndex < totalBatches; batchIndex++)
    {
        int startIndex = batchIndex * batchSize;
        int count = Mathf.Min(batchSize, combines.Count - startIndex);
        
        // Create mesh for this batch
        Mesh batchMesh = new Mesh();
        batchMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        batchMesh.CombineMeshes(combines.GetRange(startIndex, count).ToArray(), true);
        
        // Create batch object
        GameObject batchObject = new GameObject($"Batch_{batchIndex}");
        batchObject.transform.parent = combinedObject.transform;
        
        MeshFilter batchFilter = batchObject.AddComponent<MeshFilter>();
        batchFilter.mesh = batchMesh;
        
        MeshRenderer batchRenderer = batchObject.AddComponent<MeshRenderer>();
        batchRenderer.material = materialToUse;
        batchRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        batchRenderer.receiveShadows = false;
        
        // Add collider if needed (might be too heavy for 10k objects)
        // MeshCollider collider = batchObject.AddComponent<MeshCollider>();
        // collider.sharedMesh = batchMesh;
    }
    
    Debug.Log($"Completed baking {combines.Count} instances into {totalBatches} batch objects");
    
    // Clean up temporary objects
    // Destroy(testContainer);
}

    private void Update()
    {
        // Rebuild any mesh types that need it
        if (needsRebuild.Count > 0)
        {
            foreach (var id in needsRebuild)
            {
                if (settingsLookup.ContainsKey(id) && settingsLookup[id].useCombinedMesh)
                {
                    RebuildCombinedMesh(id);
                }
            }
            needsRebuild.Clear();
        }
    }

    private void OnDestroy()
    {
       
    }



  
    public void CreateMesh(GameObject prefab, BuildID buildingID, Vector2Int gridPosition)
    {
        if (!settingsLookup.ContainsKey(buildingID))
        {
            // Create new settings for this building type
            BuildingMeshSettings newSettings = new BuildingMeshSettings
            {
                buildingID = buildingID,
                meshPrefab = prefab,
                useCombinedMesh = true,
                preserveOriginalMeshes = false
            };

            // Extract material from the prefab
            MeshRenderer renderer = prefab.GetComponentInChildren<MeshRenderer>();
            newSettings.material = renderer.sharedMaterial;

            buildingMeshSettings.Add(newSettings);
            settingsLookup[buildingID] = newSettings;
            meshes[buildingID] = new Dictionary<Vector2Int, GameObject>();

            // Create combined mesh object
            GameObject combinedObj = new GameObject($"Combined{buildingID}Mesh");
            combinedObj.transform.parent = transform;
            combinedObj.AddComponent<MeshFilter>();
            MeshRenderer combinedRenderer = combinedObj.AddComponent<MeshRenderer>();
            combinedRenderer.material = newSettings.material;
            combinedRenderer.enabled = newSettings.useCombinedMesh;
            combinedMeshObjects[buildingID] = combinedObj;
        }

        // Store reference to mesh position in the lookup dictionary - no instantiation
        if (!meshes[buildingID].ContainsKey(gridPosition))
        {
            meshes[buildingID][gridPosition] = null;
        }

        // Flag for rebuilding the combined mesh
        needsRebuild.Add(buildingID);
    }
    
    public void RemoveMesh(BuildID buildingID, Vector2Int gridPosition)
    {
        if (!meshes.ContainsKey(buildingID) || 
            !meshes[buildingID].TryGetValue(gridPosition, out GameObject existingMesh))
            return;
        
        if (existingMesh != null)
            Destroy(existingMesh);
            
        meshes[buildingID].Remove(gridPosition);
        needsRebuild.Add(buildingID);
    }
private void RebuildCombinedMesh(BuildID buildingID)
{
    if (!meshes.ContainsKey(buildingID) || meshes[buildingID].Count == 0)
    {
        if (combinedMeshObjects.TryGetValue(buildingID, out GameObject obj))
        {
            var meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter != null)
                meshFilter.mesh = null;
        }
        return;
    }

    var settings = settingsLookup[buildingID];
    var prefabMeshFilter = settings.meshPrefab.GetComponentInChildren<MeshFilter>();
    if (prefabMeshFilter == null || prefabMeshFilter.sharedMesh == null)
    {
        Debug.LogError($"Prefab mesh filter or mesh is missing for {buildingID}");
        return;
    }

    List<CombineInstance> combines = new List<CombineInstance>();
    Mesh meshToUse = prefabMeshFilter.sharedMesh;
    Transform childTransform = prefabMeshFilter.transform;
    Transform rootTransform = settings.meshPrefab.transform;

    // Calculate relative offset from root to child
    Matrix4x4 childOffset = Matrix4x4.TRS(
        childTransform.localPosition,
        childTransform.localRotation,
        childTransform.localScale
    );

    foreach (var kvp in meshes[buildingID])
    {
        Vector2Int gridPos = kvp.Key;
        Vector3 worldPosition = GridSystemExtension.GridToWorldPosition(
            gridPos, GridSystem.Instance.gridOrigin, GridSystem.Instance.cellSize);

        // For walls, apply position offset and rotation based on direction
        if (buildingID == BuildID.Wall)
        {
            // Get Floor at this position
            int index = gridPos.x + gridPos.y * GridSystem.Instance.gridWidth;
            Floor floor = null;

            if (index >= 0 && index < BuildingManager.Instance.floors.Length)
            {
                floor = BuildingManager.Instance.floors[index];
            }

            if (floor != null)
            {
                // Find the direction where there's a wall
                foreach (Direction dir in Enum.GetValues(typeof(Direction)))
                {
                    if (dir != Direction.None && dir != Direction.All && floor.IsHaveWallAtDirection(dir))
                    {
                        // Calculate offset and rotation based on wall direction
                        Vector3 offset = CalculateOffsetAndRotate(dir, out float rotation);
                        Vector3 adjustedPosition = worldPosition + offset;

                        // World transform of the root object with rotation
                        Matrix4x4 wallTransformMatrix = Matrix4x4.TRS(
                            adjustedPosition,
                            Quaternion.Euler(0, rotation, 0),
                            rootTransform.localScale
                        );

                        CombineInstance wallInstance = new CombineInstance
                        {
                            mesh = meshToUse,
                            transform = wallTransformMatrix * childOffset
                        };

                        combines.Add(wallInstance);
                    }
                }

                // If there are walls, skip standard handling
                continue;
            }
        }

        // Standard handling for non-walls or fallback
        Matrix4x4 standardTransformMatrix = Matrix4x4.TRS(
            worldPosition,
            rootTransform.rotation,
            rootTransform.localScale
        );

        CombineInstance standardInstance = new CombineInstance
        {
            mesh = meshToUse,
            transform = standardTransformMatrix * childOffset
        };

        combines.Add(standardInstance);
    }

    if (combines.Count == 0) return;

    Mesh combinedMesh = new Mesh();
    // Always use UInt32 index format to support larger meshes
    combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

    combinedMesh.CombineMeshes(combines.ToArray(), true);

    if (combinedMeshObjects.TryGetValue(buildingID, out GameObject combinedObject))
    {
        combinedObject.GetComponent<MeshFilter>().mesh = combinedMesh;
        MeshRenderer combinedRenderer = combinedObject.GetComponent<MeshRenderer>();
        combinedRenderer.material = settings.material;
        MeshCollider collider = combinedObject.GetComponent<MeshCollider>();
        if (collider == null) collider = combinedObject.AddComponent<MeshCollider>();
        collider.sharedMesh = combinedMesh;
        combinedRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        combinedRenderer.receiveShadows = false;
        combinedRenderer.enabled = settings.useCombinedMesh;
    }
    
    
}
  
    public void SetUseCombinedMesh(BuildID buildingID, bool useCombined)
    {
        if (!settingsLookup.ContainsKey(buildingID))
            return;
            
        var settings = settingsLookup[buildingID];
        if (settings.useCombinedMesh == useCombined)
            return;
            
        settings.useCombinedMesh = useCombined;
        
        if (!meshes.ContainsKey(buildingID))
            return;
            
        foreach (var mesh in meshes[buildingID].Values)
        {
            if (mesh != null)
            {
                var renderer = mesh.GetComponentInChildren<MeshRenderer>();
                if (renderer != null)
                    renderer.enabled = !useCombined || settings.preserveOriginalMeshes;
            }
        }
        
        if (combinedMeshObjects.TryGetValue(buildingID, out GameObject obj))
        {
            var renderer = obj.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.enabled = useCombined;
        }
        
        if (useCombined)
            needsRebuild.Add(buildingID);
    }
    
    public void SetPreserveOriginalMeshes(BuildID buildingID, bool preserve)
    {
        if (!settingsLookup.ContainsKey(buildingID))
            return;
            
        var settings = settingsLookup[buildingID];
        settings.preserveOriginalMeshes = preserve;
        
        if (!settings.useCombinedMesh)
            return;
            
        if (!meshes.ContainsKey(buildingID))
            return;
            
        foreach (var mesh in meshes[buildingID].Values)
        {
            if (mesh != null)
            {
                var renderer = mesh.GetComponentInChildren<MeshRenderer>();
                if (renderer != null)
                    renderer.enabled = preserve;
            }
        }
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
    
    public void DestroyBuildingAtPosition(BuildID buildingID, Vector2Int gridPosition)
{
    
    RemoveMesh(buildingID, gridPosition);
    
    // If it's a wall, we need to update the Floor data structure too
    if (buildingID == BuildID.Wall)
    {
       Debug.Log("not do yet");
    }
    else if (buildingID == BuildID.Floor)
    {
        // Clear the grid cell
        GridSystem.Instance.MarkGridCells(gridPosition, new Vector2Int(1,1), false);
        
        int index = gridPosition.x + gridPosition.y * GridSystem.Instance.gridWidth;
        if (index >= 0 && index < BuildingManager.Instance.floors.Length)
        {
            BuildingManager.Instance.floors[index] = null;
        }
    }
    
    needsRebuild.Add(buildingID);
}
}

[Serializable]
public class BuildingMeshSettings
{
    public BuildID buildingID;
    public GameObject meshPrefab;
    public Material material;
    public bool useCombinedMesh = true;
    public bool preserveOriginalMeshes = false;
}