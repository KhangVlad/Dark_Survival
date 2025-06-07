using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DynamicFractureMesh : MonoBehaviour
{
    [Header("Fracture Settings")] [Range(2, 20)]
    public int pieceCount = 5;

    [Range(0f, 10f)] public float explosionForce = 5f;
    [Range(0f, 5f)] public float explosionRadius = 2f;
    [Range(0f, 3f)] public float fragmentLifetime = 2f;
    public bool useGravity = true;

    [Header("Fragment Material")] public Material fragmentMaterial;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh originalMesh;
    private bool hasBeenFractured = false;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        originalMesh = meshFilter.sharedMesh;

        if (fragmentMaterial == null)
        {
            fragmentMaterial = meshRenderer.sharedMaterial;
        }
    }


    [ContextMenu("Fracture")]
    public void Fracture()
    {
        if (hasBeenFractured) return;

        Vector3 explosionPoint = transform.position;
        List<GameObject> fragments = CreateFragments();
        ApplyExplosionForce(fragments, explosionPoint);

        hasBeenFractured = true;
        gameObject.SetActive(false);
    }

    private List<GameObject> CreateFragments()
    {
        List<GameObject> fragments = new List<GameObject>();
        Mesh mesh = originalMesh;

        // Make sure we have a valid mesh
        if (mesh == null || mesh.triangles.Length < 3)
        {
            Debug.LogError("Cannot fracture - invalid mesh");
            return fragments;
        }

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector2[] uvs = mesh.uv;
        Vector3[] normals = mesh.normals;

        // Create a simple fragment distribution
        for (int i = 0; i < pieceCount; i++)
        {
            // Create fragment object
            GameObject fragment = new GameObject($"Fragment_{gameObject.name}_{i}");
            fragment.transform.position = transform.position;
            fragment.transform.rotation = transform.rotation;
            fragment.transform.localScale = transform.localScale;

            // Add required components
            MeshFilter fragmentMeshFilter = fragment.AddComponent<MeshFilter>();
            MeshRenderer fragmentRenderer = fragment.AddComponent<MeshRenderer>();
            MeshCollider fragmentCollider = fragment.AddComponent<MeshCollider>();
            Rigidbody fragmentRb = fragment.AddComponent<Rigidbody>();

            // Configure rigidbody
            fragmentRb.useGravity = useGravity;
            fragmentRb.mass = 0.3f;

            // Apply material
            fragmentRenderer.material = fragmentMaterial;

            // Create fragment mesh
            Mesh fragmentMesh = CreateFragmentMesh(i, mesh, vertices, triangles, uvs, normals);
            fragmentMeshFilter.mesh = fragmentMesh;
            fragmentCollider.sharedMesh = fragmentMesh;
            fragmentCollider.convex = true;

            // Destroy after lifetime
            if (fragmentLifetime > 0)
            {
                Destroy(fragment, fragmentLifetime);
            }

            fragments.Add(fragment);
        }

        return fragments;
    }

    private Mesh CreateFragmentMesh(int fragmentIndex, Mesh originalMesh, Vector3[] vertices, int[] triangles,
        Vector2[] uvs, Vector3[] normals)
    {
        Mesh fragmentMesh = new Mesh();
        List<Vector3> fragmentVertices = new List<Vector3>();
        List<int> fragmentTriangles = new List<int>();
        List<Vector2> fragmentUvs = new List<Vector2>();
        List<Vector3> fragmentNormals = new List<Vector3>();

        // Simple method: distribute triangles among fragments
        int triCount = triangles.Length / 3;
        int trisPerFragment = triCount / pieceCount;
        int startTri = fragmentIndex * trisPerFragment * 3;
        int endTri = (fragmentIndex == pieceCount - 1) ? triangles.Length : (fragmentIndex + 1) * trisPerFragment * 3;

        // Create a mapping to remap vertex indices
        Dictionary<int, int> vertexMapping = new Dictionary<int, int>();

        for (int i = startTri; i < endTri; i += 3)
        {
            if (i >= triangles.Length - 2) break;

            for (int j = 0; j < 3; j++)
            {
                int originalIndex = triangles[i + j];

                if (!vertexMapping.ContainsKey(originalIndex))
                {
                    vertexMapping[originalIndex] = fragmentVertices.Count;
                    fragmentVertices.Add(vertices[originalIndex]);

                    if (uvs != null && uvs.Length > originalIndex)
                        fragmentUvs.Add(uvs[originalIndex]);

                    if (normals != null && normals.Length > originalIndex)
                        fragmentNormals.Add(normals[originalIndex]);
                }

                fragmentTriangles.Add(vertexMapping[originalIndex]);
            }
        }

        fragmentMesh.SetVertices(fragmentVertices);
        fragmentMesh.SetTriangles(fragmentTriangles, 0);

        if (fragmentUvs.Count > 0)
            fragmentMesh.SetUVs(0, fragmentUvs);

        if (fragmentNormals.Count > 0)
            fragmentMesh.SetNormals(fragmentNormals);
        else
            fragmentMesh.RecalculateNormals();

        fragmentMesh.RecalculateBounds();

        return fragmentMesh;
    }

    private void ApplyExplosionForce(List<GameObject> fragments, Vector3 explosionPoint)
    {
        foreach (GameObject fragment in fragments)
        {
            Rigidbody rb = fragment.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Add slight position offset to prevent fragments from spawning at exact same position
                Vector3 randomOffset = new Vector3(
                    Random.Range(-0.1f, 0.1f),
                    Random.Range(-0.1f, 0.1f),
                    Random.Range(-0.1f, 0.1f)
                );
                fragment.transform.position += randomOffset;

                // Apply explosion force
                rb.AddExplosionForce(explosionForce, explosionPoint, explosionRadius);

                // Add random rotation
                rb.AddTorque(Random.insideUnitSphere * explosionForce, ForceMode.Impulse);
            }
        }
    }

    // Trigger fracture on collision (optional - can be used or removed)
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 5f)
        {
            Fracture();
        }
    }

    // Utility method to fracture from external scripts
    public void FractureAt(Vector3 point)
    {
        if (hasBeenFractured) return;

        List<GameObject> fragments = CreateFragments();
        ApplyExplosionForce(fragments, point);

        hasBeenFractured = true;
        gameObject.SetActive(false);
    }
}