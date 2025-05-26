using System;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildingComfirm : MonoBehaviour //world space ui show option build confirm or cancel
{
    private Canvas _canvas;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button destroyButton; //optional, only have if gridsystem is editing mode
    [SerializeField] private Transform uiTransform; //update position of this ui to the target transform 
    [SerializeField] private Vector3 offset = new Vector3(0.5f, 3f, 0); //offset position of this ui to the target transform

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        confirmButton.onClick.AddListener(OnConfirm);
        cancelButton.onClick.AddListener(OnCancel);
    }

    private void OnDestroy()
    {
        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
    }

    private void OnConfirm()
    {
        BuildingManager.Instance.PlaceBuilding();
    }
    
 
    
    private void OnCancel()
    {
        BuildingManager.Instance.CancelBuilding();
        CanvasController.Instance.ActiveBuildingCanvas(false);
        CanvasController.Instance.SetActiveGameplayCanvas(true);
    }
    
    public void UpdatePosition(Vector3 targetPosition)
    {
        if (uiTransform != null)
        {
            uiTransform.position = targetPosition + offset;
            if (Camera.main != null)
            {
                uiTransform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
            }
        }
    }
    
    public void ActiveCanvas(bool active)
    {
        _canvas.enabled = active;
    }
}

