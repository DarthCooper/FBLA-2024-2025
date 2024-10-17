using Unity.Entities;

public struct IsFollowing : IComponentData
{
    public bool Value;
}

public struct PathFollowTarget : IComponentData
{
    public Entity Value;
}

public struct PathFollowSpeed : IComponentData
{
    public float Value;
}

public struct PathFollowTargetDistance : IComponentData
{
    public float Value; 
}

public struct PathFollowRetreatDistances : IComponentData
{
    public float Max;
    public float Min;

    public float Trigger;
}

