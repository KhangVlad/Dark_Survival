using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "building", menuName = "SurvivalGame/Building")]
public class BuildDataSO : ScriptableObject
{
    public BuildID buildID;
    public string objectName;
    public GameObject prefab;
    public Vector2Int gridSize = Vector2Int.one; 
    public Sprite icon;
}

// BuildingType.cs - Enum for different building types
public enum BuildID
{
    None = 0,
    Floor = 1,
    Wall =2,
    Door = 3,
}