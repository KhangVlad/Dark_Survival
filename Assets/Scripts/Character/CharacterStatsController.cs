using System;
using UnityEngine;

public class CharacterStatsController : MonoBehaviour
{
    private CharacterStatsSO dataSO; //base data , will + stat if wear item

    [SerializeField] private SurvivalStats currentStats;
    

    private void Start()
    {
        InitializeData();
        TimeManager.Instance.OnMinuteElapsed += OnMinuteElapsed;
    }

    private void OnDestroy()
    {
        TimeManager.Instance.OnMinuteElapsed -= OnMinuteElapsed;
    }

    private void InitializeData()
    {
        CharacterStatsSO so = Resources.Load<CharacterStatsSO>("Character/Stats/CharacterStats");
        dataSO = so;
        currentStats = new SurvivalStats();
        currentStats.currentHunger = so.baseHunger;
        currentStats.currentThirst = so.baseThirst;
        currentStats.currentHP = so.baseHP;
    }

    private void Update()
    {
        
    }

    private void OnMinuteElapsed() //use formula to calculate later / this just temp
    {
        currentStats.currentHunger -= 2;
        currentStats.currentThirst -= 1;
    }
    
}


[Serializable]
public class SurvivalStats
{
    public float currentHP;
    public float currentHunger;
    public float currentThirst;

    public SurvivalStats()
    {
        
    }
}