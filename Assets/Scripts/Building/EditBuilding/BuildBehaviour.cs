
using UnityEngine;

public abstract class BuildBehaviour : MonoBehaviour
{
    public EntityID buildID;

    public void SetBuildID(EntityID newBuildID)
    {
        buildID = newBuildID;
    }


    public abstract void DeleteBuilding();

    public abstract void InteractWithBuilding();
}
