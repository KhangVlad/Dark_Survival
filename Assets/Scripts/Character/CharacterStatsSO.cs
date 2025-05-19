using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStats", menuName = "SurvivalGame/Character Stats")]
public class CharacterStatsSO : ScriptableObject
{
    [Header("Core Stats")]
    public float baseHP = 100f;
    public float baseStamina = 100f;

    [Header("Survival Stats")]
    public float baseHunger = 100f;
    public float baseThirst = 100f;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float sprintSpeed = 6f;
}