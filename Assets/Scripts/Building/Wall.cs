using UnityEngine;
using System;


[Serializable]
public class Wall : Building 
{
    public Wall()
    {
        this.buildID = BuildID.Wall;
    }
}   

