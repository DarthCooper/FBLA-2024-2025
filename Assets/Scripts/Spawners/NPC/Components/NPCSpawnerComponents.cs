using Unity.Entities;
using Unity.Mathematics;

public struct NPCSpawners : IComponentData
{
    public BlobAssetReference<NPCSpawnerData> BlobAssetReference;
}

public struct NPCSpawnerData
{
    public float3 pos;
    public float radius;

    public BlobArray<NPCs> npcs;

    public Random random;
}

public enum NPCs
{
    RANGER,
    CLERIC,
    BARBARIAN,
    ROGUE,
    WARLOCK
}

public struct NPCTypes : IComponentData
{
    public Entity Ranger;
    public Entity Cleric;
    public Entity Barbarian;
    public Entity Rogue;
    public Entity Warlock;
}

public struct NPCsSpawned : IComponentData { }