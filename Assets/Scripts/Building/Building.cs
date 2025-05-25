
    using System;
    using UnityEngine;

    public class Building : MonoBehaviour
    {
       public BuildingData buildingData  { get; private set; }
       
       public BuildingData BuildingData
       {
           get => buildingData;
           set => buildingData = value;
       }    
    
       public void SetBuildingData(BuildingData data)
       {
           buildingData = data;
       }
        
    }



    [Serializable]
    public class BuildingData
    {
        public BuildID buildID;
        public Vector2Int gridSize = Vector2Int.one;

        public BuildingData(BuildDataSO so)
        {
            buildID = so.buildID;
            gridSize = so.gridSize;
        }
    }