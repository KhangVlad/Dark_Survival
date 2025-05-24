
    public static class BuildingExtension
    {
        public static int GetWallDirectionIndex(WallDirection direction)
        {
            return direction switch
            {
                WallDirection.Top => 0,
                WallDirection.Right => 1,
                WallDirection.Bot => 2,
                WallDirection.Left => 3,
                _ => -1 // Invalid direction
            };
        }

        public static WallDirection GetOppositeWallDirection(WallDirection d)
        {
            return d switch
            {
                WallDirection.Top => WallDirection.Bot,
                WallDirection.Right => WallDirection.Left,
                WallDirection.Bot => WallDirection.Top,
                WallDirection.Left => WallDirection.Right,
                _ => WallDirection.None // Invalid direction
            };
        }
    }
