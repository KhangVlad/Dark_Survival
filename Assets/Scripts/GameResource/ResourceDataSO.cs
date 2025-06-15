using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "SurvivalGame/ResourceData")]
public class ResourceDataSO : ScriptableObject
{
    public EntityID ItemID;
    public GameObject prefab;
    [Tooltip("Higher weight means higher probability of spawning")]
    public float SpawnWeight = 1f;

    public Item ItemRecive;

}
