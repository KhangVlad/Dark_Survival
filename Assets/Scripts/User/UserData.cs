using System;
using UnityEngine;

[Serializable]
public class UserData
{
    public Inventory inventory;
    public bool IsFirstLogin;

    public UserData()
    {
        inventory = new Inventory();
        IsFirstLogin = true; // Default to true for first login
    }
}

[Serializable]
public class Inventory
{
    public Item[] items;

    public Inventory()
    {
      
    }

    public void AddItem(ItemID itemID, int amount)
    {
       
    }

    public void RemoveItem(ItemID itemID, int amount)
    {
      
    }
}

[Serializable]
public class Item
{
    public ItemID itemID;
    public int amount;

}