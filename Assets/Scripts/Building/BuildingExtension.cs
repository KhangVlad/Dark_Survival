using System;
using UnityEngine;
    public static class BuildingExtension
    {
        public static int GetWallDirectionIndex(Direction direction)
        {
            return direction switch
            {
                Direction.Top => 0,
                Direction.Right => 1,
                Direction.Bot => 2,
                Direction.Left => 3,
                _ => -1 // Invalid direction
            };
        }

        public static Direction GetOppositeWallDirection(Direction d)
        {
            return d switch
            {
                Direction.Top => Direction.Bot,
                Direction.Right => Direction.Left,  
                Direction.Bot => Direction.Top,
                Direction.Left => Direction.Right,
                _ => Direction.None // Invalid direction
            };
        }
    }
