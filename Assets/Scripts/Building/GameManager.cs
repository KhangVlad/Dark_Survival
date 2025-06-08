using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public BuildDataSO[] BuildingDataSO;
    
    public event Action OnDataLoaded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }


    private void Start()
    {
        LoadBuildDataSO();
    }

    private void LoadBuildDataSO()
    {
        BuildingDataSO = Resources.LoadAll<BuildDataSO>("Building");
        OnDataLoaded?.Invoke();
    }

    public BuildDataSO GetBuildingDataByID(EntityID id)
    {
        BuildDataSO data = null;
        for (int i = 0; i < BuildingDataSO.Length; i++)
        {
            if (BuildingDataSO[i].entityID == id)
            {
                data = BuildingDataSO[i];
            }
        }

        return data;
    }
}