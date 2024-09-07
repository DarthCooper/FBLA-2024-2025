using Unity.Entities;
using Unity.Mathematics;

public struct PlayerTag : IComponentData { }

public struct PlayerChecks : IComponentData
{
    public bool groundCheck;
    public bool leftWallCheck;
    public bool rightWallCheck;
    public bool ceilingCheck;
    public bool forwardCheck;
    public bool backCheck;
}

public struct PlayerChecksOffset : IComponentData
{
    public float4 Value;
}
