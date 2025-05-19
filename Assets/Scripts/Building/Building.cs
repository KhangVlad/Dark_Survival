
    using UnityEngine;

    public class Building : MonoBehaviour
    {
        public BuildID buildID;
        public BuildDataSO data;
        public void Initialize(BuildDataSO so)
        {
            this.data = so;
        }

    }
