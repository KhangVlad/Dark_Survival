using System;
using UnityEngine;
using StarterAssets;

public class CanvasController : MonoBehaviour
{
    public static CanvasController Instance { get; private set; }
    public UIGameplay uiGameplay { get; set; }
    public UIBuilding uiBuilding { get; set; }
    public UICanvasControllerInput joystick { get; set; }

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
        uiGameplay = FindFirstObjectByType<UIGameplay>();
        uiBuilding = FindFirstObjectByType<UIBuilding>();
        joystick = FindFirstObjectByType<UICanvasControllerInput>();
    }

    public void ActiveBuildingCanvas(bool active)
    {
        uiBuilding.ActiveCanvas(active);
      
    }
    
    public void SetActiveGameplayCanvas(bool active)
    {
        uiGameplay.SetActiveCanvas(active);
    }
    public void ActiveJoyStickCanvas(bool active)
    {
        joystick.ActiveCanvas(active);
    }
}