using System;
using UnityEngine;
using UnityEngine.UI;

public class UIBuilding : MonoBehaviour
{
    public static UIBuilding Instance { get; private set; }
    private Canvas _canvas;
    [SerializeField] private UIBuildingSlot _uiPrefabs;
    [SerializeField] private Transform parrent;
    [SerializeField] private Button editModeButton;
    public event Action<bool> SwitchEditMode;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        _canvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        RenderUI();
        editModeButton.onClick.AddListener(() =>
        {
            SwitchEditModeHandler(true);
            ActiveCanvas(false);
        });
    }
    
    private void SwitchEditModeHandler(bool isEditMode)
    {
        Debug.Log($"Switching edit mode: {isEditMode}");
        if (isEditMode)
        {
            SwitchEditMode?.Invoke(true);
        }
        else
        {
            SwitchEditMode?.Invoke(false);
        }
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
                GameManager.Instance.GetBuildingDataByID(GameManager.Instance.BuildingDataSO[i].entityID);
            slot.InitializeData(dataSO);
            slot.OnClick += (o =>
            {
                BuildingManager.Instance.StartPlacingBuilding(o);
                ActiveCanvas(false);
            }); 
        }
    }

    public void ActiveCanvas(bool active)
    {
        _canvas.enabled = active;
    }
    
    public void CancelEditMode()
    {
        SwitchEditModeHandler(false);
        ActiveCanvas(false);
    }
}