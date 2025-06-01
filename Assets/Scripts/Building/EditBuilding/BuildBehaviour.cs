
    using UnityEngine;

    public abstract class BuildBehaviour : MonoBehaviour
    {
        public BuildID buildID;
        
        public void SetBuildID(BuildID newBuildID)
        {
            buildID = newBuildID;
        }
        
        
        public abstract void DeleteBuilding();
        
        public abstract void InteractWithBuilding();
    }
