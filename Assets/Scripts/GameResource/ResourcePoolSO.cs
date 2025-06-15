using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourcePool", menuName = "SurvivalGame/ResourcePool")]
public class ResourcePoolSO : ScriptableObject
{
    public List<ResourceDataSO> resources;
}       