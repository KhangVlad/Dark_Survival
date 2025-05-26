using UnityEngine;
using System;


[Serializable]
public class Wall : Building //1 wall could be attached to 2 floors from 2 direction of floor
{
    public Floor[] attachedFloor = new Floor[2];
    
    public Wall(Floor floor, BuildID id)
    {
        this.buildID = id;
        attachedFloor[0] = floor;
    }
    
    
    public void SetFloorWithDirection(Floor f)
    {
        if (attachedFloor[0] == null)
        {
            attachedFloor[0] = f;
            return;
        }
        
        if (attachedFloor[1] == null)
        {
            attachedFloor[1] = f;
            return;
        }
        
        Debug.LogWarning("Wall already has two attached floors, cannot add more.");
    }
}
