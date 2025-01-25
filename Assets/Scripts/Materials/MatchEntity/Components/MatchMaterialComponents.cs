using Unity.Entities;

public struct FollowerEntity : IComponentData 
{
    public Entity Value;
}

public struct LeaderEntity : IComponentData 
{ 
    public Entity Value;
}
