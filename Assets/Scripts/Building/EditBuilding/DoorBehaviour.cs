using UnityEngine;
using System;
using DG.Tweening;

public class DoorBehaviour : BuildBehaviour
{
    public int floorIndex = -1;
    public int wallIndex = -1;
    public Vector2Int gridPosition;
    public Transform doorHandle;
    
    private Quaternion defaultRotation;
    private Quaternion openRotation;
    private BoxCollider doorCollider;
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private Ease easeType = Ease.OutBack;
    private bool isOpen = false;

    public void Init(int wIndex, int floor, EntityID id, Vector2Int position)
    {
        wallIndex = wIndex;
        floorIndex = floor;
        SetBuildID(id);
        gridPosition = position;
        doorHandle = transform.GetChild(0);
        defaultRotation = doorHandle.rotation;
        openRotation = defaultRotation * Quaternion.Euler(0f, 90f, 0f);
        doorCollider = GetComponent<BoxCollider>();
        
        if (doorCollider == null)
        {
            doorCollider = gameObject.AddComponent<BoxCollider>();
            // Set default collider size if needed
            doorCollider.size = new Vector3(1f, 2f, 0.2f);
            doorCollider.center = new Vector3(0f, 1f, 0f);
        }
    }


    public override void DeleteBuilding()
    {
        Direction direction = BuildingExtension.GetWallDirectionByIndex(wallIndex);
        GridSystem.Instance.SetWallData(gridPosition, direction, EntityID.None);
        Destroy(this.gameObject);
    }

    public override void InteractWithBuilding()
    {
        HandleDoor();
    }

    public void HandleDoor()
    {
        if (isOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        doorHandle.DORotateQuaternion(openRotation, animationDuration)
            .SetEase(easeType)
            .OnComplete(() => {
                isOpen = true;
                ToggleCollider(false); // Disable collider when door is open
            });
    }

    private void CloseDoor()
    {
        doorHandle.DORotateQuaternion(defaultRotation, animationDuration)
            .SetEase(easeType)
            .OnComplete(() => {
                isOpen = false;
                ToggleCollider(true); // Enable collider when door is closed
            });
    }
    
    private void ToggleCollider(bool enabled)
    {
        doorCollider.isTrigger = !enabled;
    }
}