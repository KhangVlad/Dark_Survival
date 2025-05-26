
    using System;
    using UnityEngine;

    
    [Serializable]
    public class Building 
    {
        public BuildingData buildingData;
        public BuildID buildID;

       public void SetBuildingData(BuildingData data)
       {
           buildingData = data;
       }
        
    }



    [Serializable]
    public class BuildingData
    {
       
        public Vector2Int gridSize = Vector2Int.one;

        public BuildingData(BuildDataSO so)
        {
            gridSize = so.gridSize;
        }
    }