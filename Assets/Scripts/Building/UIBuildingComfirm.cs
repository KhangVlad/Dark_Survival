using System;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildingComfirm : MonoBehaviour //world space ui show option build confirm or cancel
{
    private Canvas _canvas;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
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
    
    private void OnConfirm()
    {
        Debug.Log("confirm");
        GridSystem.Instance.PlaceBuilding();
    }
    
    private void OnCancel()
    {
        Debug.Log("cancel");
        // Cancel the building placement
        GridSystem.Instance.CancelBuilding();
        CanvasController.Instance.ActiveBuildingCanvas(false);
        CanvasController.Instance.SetActiveGameplayCanvas(true);
    }
    
    public void UpdatePosition(Vector3 targetPosition)
    {
        if (uiTransform != null)
        {
            uiTransform.position = targetPosition + offset;

            // Adjust rotation for isometric view
            if (Camera.main != null)
            {
                // Match the camera's rotation for isometric alignment
                uiTransform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
            }
        }
    }
    
    public void ActiveCanvas(bool active)
    {
        Debug.Log($"ActiveCanvas: {active}");
        _canvas.enabled = active;
        
        // Ensure that the components are properly enabled/disabled
        if (confirmButton != null)
            confirmButton.gameObject.SetActive(active);
            
        if (cancelButton != null)
            cancelButton.gameObject.SetActive(active);
    }
}

