using UnityEngine;
using System;


[Serializable]
public class Wall : Building 
{
    public Wall(EntityID id)
    {
        this.entityID = id;
    }
}   

