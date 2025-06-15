public static class EntitiesExtention
{
    public static T CreateEntityByID<T>(this EntityID entityID) where T : Entity
    {
        switch (entityID)
        {
            case EntityID.Floor:
                return new Floor(entityID) as T;
            case EntityID.Wall:
                return new Wall(entityID) as T;
            case EntityID.Door:
                return new Door(entityID) as T;
            case EntityID.PineTree:
                return new ResourceNode(entityID) as T;
            case EntityID.Bush:
                return new ResourceNode(entityID) as T;
            case EntityID.Stone:
                return new ResourceNode(entityID) as T;
            case EntityID.Log:
                return new ResourceNode(entityID) as T;
            default:
                return null;
        }
    }
}