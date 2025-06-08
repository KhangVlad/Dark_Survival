using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "SurvivalGame/ResourceData")]
public class ResourceDataSO : ScriptableObject
{
    public EntityID itemID;
    public GameObject prefab;
    [Tooltip("Higher weight means higher probability of spawning")]
    public float spawnWeight = 1f;
}
