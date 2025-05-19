using System;
using UnityEngine;
using UnityEngine.UI;

public class UIGameplay : MonoBehaviour
{
    private Canvas _canvas;
    [SerializeField] private Button buildBtn;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        buildBtn.onClick.AddListener(() => CanvasController.Instance.ActiveBuildingCanvas(true));
    }

    private void OnDestroy()
    {
        buildBtn.onClick.RemoveAllListeners();
    }
}