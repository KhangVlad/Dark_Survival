
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIToolButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("References")]
    [SerializeField] private CharacterResourceSensor characterResourceSensor;
    [SerializeField] private Image buttonImage;

    [Header("Tool Sprites")]
    [SerializeField] private Sprite collectSprite;
    [SerializeField] private Sprite miningSprite;
    [SerializeField] private Sprite axeSprite;
    [SerializeField] private Sprite defaultSprite;

    [Header("Text Display Settings")]
    [SerializeField] private Vector3 textOffset = new Vector3(0f, 2f, 0f); // Offset above character

    private void Start()
    {
        if (characterResourceSensor != null)
        {
            characterResourceSensor.OnTargetChanged += UpdateButtonState;
        }

        if (buttonImage == null)
        {
            buttonImage = GetComponent<Image>();
        }


        // Initialize with default state
        UpdateButtonState(null);
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (characterResourceSensor != null)
        {
            characterResourceSensor.OnTargetChanged -= UpdateButtonState;
        }
    }

    private void UpdateButtonState(ResourceBehavior target)
    {
        if (target == null)
        {
            // No target - show default state or hide button
            SetButtonSprite(defaultSprite);
            SetButtonInteractable(false);
            return;
        }

        // Check if ResourceNode is properly initialized
        if (target.ResourceNode == null)
        {
            SetButtonSprite(defaultSprite);
            SetButtonInteractable(false);
            return;
        }

        // Update button based on resource type
        Sprite spriteToUse = GetSpriteForResource(target.ResourceNode.entityID);
        SetButtonSprite(spriteToUse);
        SetButtonInteractable(true);
    }

    private Sprite GetSpriteForResource(EntityID entityID)
    {
        switch (entityID)
        {
            case EntityID.PineTree:
                return axeSprite;

            case EntityID.Stone:
                return miningSprite;

            case EntityID.Bush:
                return collectSprite;

            default:
                return miningSprite; // Default sprite
        }
    }

    private void SetButtonSprite(Sprite sprite)
    {
        if (buttonImage != null && sprite != null)
        {
            buttonImage.sprite = sprite;
        }
    }

    private void SetButtonInteractable(bool interactable)
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {

    }

    public void OnPointerUp(PointerEventData eventData)
    {

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (characterResourceSensor == null || characterResourceSensor.CurrentTarget == null)
        {
            Debug.Log("No target to interact with");
            return;
        }

        ResourceBehavior target = characterResourceSensor.CurrentTarget;

        // Check if target is still valid and has ResourceNode
        if (target == null || target.ResourceNode == null)
        {
            Debug.LogWarning("Target or ResourceNode is null during click");
            return;
        }

        InteractWithTarget(target);
    }

    private void InteractWithTarget(ResourceBehavior target)
    {
        EntityID resourceType = target.ResourceNode.entityID;

        // Call the resource's collection method
        target.OnCollected();

        ShowCollectionText(resourceType);
        // Play collection effect
        PlayCollectionEffect(resourceType);
    }

    private void ShowCollectionText(EntityID resourceType)
    {
        if (TextManager.Instance == null)
        {
            Debug.LogWarning("UITextPooling.Instance is null");
            return;
        }

        // Get text position - use the resource's world position instead of character position
        Vector3 textPosition;

        textPosition = PlayerControl.Instance.GetCharacterPosition();
        UIText uiText = TextManager.Instance.GetFromPool();
        if (uiText != null)
        {
            SetupCollectionText(uiText, resourceType, textPosition);
        }
        else
        {
        }
    }

    private void SetupCollectionText(UIText uiText, EntityID resourceType, Vector3 worldPosition)
    {
        // Get resource info for display
        ResourceCollectionInfo info = GetResourceCollectionInfo(resourceType);

        // Setup text properties
        uiText.SetText(info.displayText);
        uiText.SetColor(info.textColor);

        // Set world position with offset
        Vector3 offsetPosition = worldPosition + textOffset;
        uiText.SetWorldPosition(offsetPosition);

        // Make text look at camera
        uiText.LookAtCamera();

        uiText.StartCollectionAnimation();

    }
    private ResourceCollectionInfo GetResourceCollectionInfo(EntityID resourceType)
    {
        ResourceCollectionInfo info = new ResourceCollectionInfo();

        switch (resourceType)
        {
            case EntityID.PineTree:
                info.displayText = "+5 Wood";
                info.textColor = new Color(0.6f, 0.4f, 0.2f); // Brown
                break;

            case EntityID.Stone:
                info.displayText = "+3 Stone";
                info.textColor = Color.gray;
                break;

            case EntityID.Bush:
                info.displayText = "+2 Berry";
                info.textColor = Color.red;
                break;

            case EntityID.Log:
                info.displayText = "+1 Log";
                info.textColor = new Color(0.4f, 0.2f, 0.1f); // Dark brown
                break;

            default:
                info.displayText = "+1 Resource";
                info.textColor = Color.white;
                break;
        }

        return info;
    }

    private void PlayCollectionEffect(EntityID resourceType)
    {

    }

    // Helper struct for resource collection display info
    [System.Serializable]
    private struct ResourceCollectionInfo
    {
        public string displayText;
        public Color textColor;
    }
}