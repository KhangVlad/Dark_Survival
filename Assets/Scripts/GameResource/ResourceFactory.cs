using System.Collections;
using UnityEngine;
public static class ResourceFactory
{

    public static ResourceBehavior CreateResourceBehavior(ResourceNode resourceNode, GameObject g)
    {
        ResourceBehavior resourceBehavior = null;
        switch (resourceNode.entityID)
        {
            case EntityID.Bush:
                resourceBehavior = g.AddComponent<BushBehavior>();
                break;
            case EntityID.Stone:
                resourceBehavior = g.AddComponent<StoneBehavior>();
                break;
            case EntityID.Log:
                resourceBehavior = g.AddComponent<LogBehavior>();
                break;
            case EntityID.PineTree:
                resourceBehavior = g.AddComponent<PineTreeBehavior>();
                break;
            default:
                Debug.LogError($"Resource type {resourceNode.entityID} not recognized.");
                break;
        }
        resourceBehavior?.Initialize(resourceNode);
        return resourceBehavior;
    }
}