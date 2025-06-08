using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "building", menuName = "SurvivalGame/Building")]
public class BuildDataSO : ScriptableObject
{
    public EntityID entityID;
    public string objectName;
    public GameObject prefab;
    public Vector2Int gridSize = Vector2Int.one;
    public Sprite icon;
}

// BuildingType.cs - Enum for different building types