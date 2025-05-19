
    using System;
    using UnityEngine;

    public class PersistantObject : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }
