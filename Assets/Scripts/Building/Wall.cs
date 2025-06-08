using UnityEngine;
using System;


[Serializable]
public class Wall : Building 
{
    public Wall()
    {
        this.entityID = EntityID.Wall;
    }
}   

