using System;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildingComfirm : MonoBehaviour //world space ui show option build confirm or cancel
{
    private Canvas _canvas;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Transform uiTransform; //update position of this ui to the target transform 
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0); //offset position of this ui to the target transform

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        confirmButton.onClick.AddListener(OnConfirm);
        cancelButton.onClick.AddListener(OnCancel);
    }
    
    private void OnConfirm()
    {
        Debug.Log("confirm");
        GridBuildingSystem.Instance.PlaceBuilding();
    }
    
    private void OnCancel()
    {
        Debug.Log("cancel");
        // Cancel the building placement
        GridBuildingSystem.Instance.CancelBuilding();
        CanvasController.Instance.ActiveBuildingCanvas(false);
    }
    
    public void UpdatePosition(Vector3 targetPosition)
    {
        if (uiTransform != null)
        {
            uiTransform.position = targetPosition + offset;
            
            // Optional: Make UI face the camera
            if (Camera.main != null)
            {
                uiTransform.LookAt(Camera.main.transform);
                uiTransform.Rotate(0, 180, 0); // Flip to face the camera correctly
            }
        }
    }
    
    public void ActiveCanvas(bool active)
    {
        _canvas.enabled = active;
        
        // Ensure that the components are properly enabled/disabled
        if (confirmButton != null)
            confirmButton.gameObject.SetActive(active);
            
        if (cancelButton != null)
            cancelButton.gameObject.SetActive(active);
    }
}

