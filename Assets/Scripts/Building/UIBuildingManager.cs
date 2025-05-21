using System;
using UnityEngine;

public class UIBuildingManager : MonoBehaviour
{
    private Canvas _canvas;

    [SerializeField] private UIBuildingSlot _uiPrefabs;
    [SerializeField] private Transform parrent;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        RenderUI();
    }

    private void OnDestroy()
    {
    }

    private void RenderUI()
    {
        foreach (Transform child in parrent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < GameManager.Instance.BuildingDataSO.Length; i++)
        {
            var slot = Instantiate(_uiPrefabs, parrent);
            BuildDataSO dataSO =
                GameManager.Instance.GetBuildingDataByID(GameManager.Instance.BuildingDataSO[i].buildID);
            slot.InitializeData(dataSO);
            slot.OnClick += (o => { 
                GridBuildingSystem.Instance.StartPlacingBuilding(o);
                ActiveCanvas(false);
            });
        }
    }

    public void ActiveCanvas(bool active)
    {
        _canvas.enabled = active;
    }
}
