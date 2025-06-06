using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "building", menuName = "SurvivalGame/Building")]
public class BuildDataSO : ScriptableObject
{
    public BuildID buildID;
    public string objectName;
    public Building prefab;
    public Vector2Int gridSize = Vector2Int.one; 
    public Sprite icon;
}

// BuildingType.cs - Enum for different building types
public enum BuildID
{
    Floor = 0,
    Wall = 1,
    Chest = 2,
}