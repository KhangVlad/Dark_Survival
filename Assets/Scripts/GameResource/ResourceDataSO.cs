using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "SurvivalGame/ResourceData")]
public class ResourceDataSO : ScriptableObject
{
    public ItemID itemID;
    public GameObject prefab;
    [Tooltip("Higher weight means higher probability of spawning")]
    public float spawnWeight = 1f;
    [Tooltip("Minimum number to spawn")]
    public int minAmount = 5;
    [Tooltip("Maximum number to spawn")]
    public int maxAmount = 15;
}