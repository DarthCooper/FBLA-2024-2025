using Unity.Entities;
using Unity.Mathematics;

public struct IsFollowing : IComponentData
{
    public bool Value;
}

public struct Direction : IComponentData
{
    public float3 Value;
}

public struct PathFollowTarget : IComponentData
{
    public Entity Value;
}

public struct UpdateDelay : IComponentData
{
    public float Value;
    public float maxValue;
}

public struct PathFollowSpeed : IComponentData
{
    public float Value;
}

public struct PathFollowTargetDistance : IComponentData
{
    public float Value; 
}

public struct PathStartedTag : IComponentData { }

public struct PathFollowRetreatDistances : IComponentData
{
    public float Max;
    public float Min;

    public float Trigger;
}

public struct PathFollowerPreviousTarget : IComponentData
{
    public Entity Value;
}

public struct PathFollowerPreviousTargetDistance : IComponentData
{
    public float Value;
}

public struct PathFollowerScoutingDistances : IComponentData
{
    public float Max;
    public float Min;
}

public struct Retreating : IComponentData { }
public struct Attacking : IComponentData { }
public struct Hunting : IComponentData { }
public struct Scouting : IComponentData { }

