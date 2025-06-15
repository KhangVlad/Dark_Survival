using System;
using UnityEngine;

public class UserManager : MonoBehaviour
{
    public static UserManager Instance { get; private set; }


    [SerializeField] public UserData userData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }

    private void Start()
    {
        userData = SaveManager.Instance.LoadUserData();
        if (userData == null)
        {
            InitializeUserData();
        }
    }

    private void InitializeUserData()
    {
        userData = new UserData();
        userData.inventory.AddItem(ItemID.Log, 100);
    }
}