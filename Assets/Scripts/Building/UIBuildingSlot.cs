using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildingSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI name;
    [SerializeField] private Image icon;
    [SerializeField] private Button choseBtn;
    private BuildDataSO _data;
    public event Action<BuildDataSO> OnClick;

    private void Start()
    {
        choseBtn.onClick.AddListener(() =>
        {
            OnClick?.Invoke(_data);
        });
    }

    private void OnDestroy()
    {
        choseBtn.onClick.RemoveAllListeners();
    }


    public void InitializeData(BuildDataSO ob)
    {
        _data = ob;
        name.text = ob.objectName;
        icon.sprite = ob.icon;
    }
}