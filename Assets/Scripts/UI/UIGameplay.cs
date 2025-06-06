using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIGameplay : MonoBehaviour
{
    private Canvas _canvas;
    [SerializeField] private Button buildBtn;
    [SerializeField] private Sprite editIcon;
    [SerializeField] private Sprite normalIcon;
    public bool IsEditMode = false;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        buildBtn.onClick.AddListener(HandleClick);
        UIBuildingManager.Instance.SwitchEditMode += SwitchEditMode;
        // Start with normal icon
        buildBtn.image.sprite = normalIcon;
    }

    private void HandleClick()
    {
        if (!IsEditMode)
        {
            // Normal mode -> enter building selection
            CanvasController.Instance.ActiveBuildingCanvas(true);
            UIBuildingManager.Instance.ActiveCanvas(true);
        }
        else
        {
            UIBuildingManager.Instance.CancelEditMode();
            CanvasController.Instance.ActiveBuildingCanvas(false);
        }
    }

    private void OnDestroy()
    {
        buildBtn.onClick.RemoveAllListeners();
        if (UIBuildingManager.Instance != null)
        {
            UIBuildingManager.Instance.SwitchEditMode -= SwitchEditMode;
        }
    }

    public void SetActiveCanvas(bool active)
    {
        _canvas.enabled = active;
    }

    private void SwitchEditMode(bool isEditMode)
    {
        this.IsEditMode = isEditMode;
        if (isEditMode)
        {
            buildBtn.image.sprite = editIcon; // Change to edit icon
        }
        else
        {
            buildBtn.image.sprite = normalIcon; // Change back to normal icon
        }

        SetActiveCanvas(true); // Make sure gameplay UI is visible
    }
}