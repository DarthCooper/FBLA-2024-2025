using Unity.Entities;
using Unity.Mathematics;

public struct ApplyKnockBack : IComponentData { }

public struct KnockBackStrength : IComponentData
{
    public float Value;
}

public struct KnockBackDir : IComponentData 
{
    public float3 Value;
}

public struct KnockBackStartPos : IComponentData
{
    public float3 Value;
}

public struct KnockBackMaxDist : IComponentData
{
    public float Value;
}
