using UnityEngine;

public abstract class ResourceBehavior : MonoBehaviour
{

    public ResourceNode ResourceNode;


    public void Initialize(ResourceNode resourceNode)
    {
        ResourceNode = resourceNode;
    }


    public abstract void OnCollected();
}
public interface IResourceBehavior
{
    void Initialize(ResourceNode resourceNode);
    void OnCollected();
}