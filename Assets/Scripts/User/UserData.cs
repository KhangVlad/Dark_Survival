using System;
using UnityEngine;

[Serializable]
public class UserData
{
    // Make inventory public to show in Inspector
    public Inventory inventory;

    // Property for code access
    public Inventory Inventory
    {
        get { return inventory; }
        set { inventory = value; }
    }
    
    public UserData()
    {
        inventory = new Inventory();
    }
}


[Serializable]
public class Inventory
{
    public Item[] items;

    public Inventory()
    {
        items = new Item[10];
        for (int i = 0; i < 10; i++)
        {
            items[i] = new Item(ItemID.None, 0);
        }
    }

    public void AddItem(ItemID itemID, int amount)
    {
        Debug .Log($"Adding item: {itemID} with amount: {amount}");
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].itemID == itemID)
            {
                items[i].AddAmount(amount);
                return;
            }
        }
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].itemID == ItemID.None)
            {
                items[i] = new Item(itemID, amount);
                return;
            }
        }
    }

    public void RemoveItem(ItemID itemID, int amount)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].itemID == itemID)
            {
                items[i].RemoveAmount(amount);
                if (items[i].amount <= 0)
                {
                    items[i] = new Item(ItemID.None, 0); // Reset to empty item
                }

                return;
            }
        }
    }
}

[Serializable]
public class Item
{
    public ItemID itemID;
    public int amount;

    public Item(ItemID id, int amount)
    {
        this.itemID = id;
        this.amount = amount;
    }

    public void AddAmount(int amount)
    {
        this.amount += amount;
    }

    public void RemoveAmount(int amount)
    {
        this.amount -= amount;
        if (this.amount < 0)
        {
            this.amount = 0;
        }
    }
}