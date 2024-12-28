using Unity.Entities;
using Unity.Mathematics;

public struct Spawners : IComponentData
{
    public BlobAssetReference<SpawnerArrays> BlobAssetReference;
}

public struct WaveSpawners : IComponentData
{
    public BlobAssetReference<EnemyWaveData> BlobAssetReference;
}

public struct EnemyWaveData : IComponentData
{
    public BlobArray<SpawnerArrays> BlobArray;

    public int waveIndex;
}

public struct SpawnerArrays
{
    public BlobArray<SpawnerData> blob;

    public float delay;
}

public struct SpawnerData
{
    public float3 pos;
    public float delay;
    public float maxDelay;
    public int amountPerSpawn;
    public float radius;

    public BlobArray<Enemies> enemies;

    public Unity.Mathematics.Random random;

    public bool spawned;
}

public struct CanSpawn : IComponentData { }

public enum Enemies
{
    RANGER,
    FIGHTER,
    BARBARIAN,
    ROGUE,
    WARLOCK
}

public struct EnemyTypes : IComponentData
{
    public Entity Ranger;
    public Entity Fighter;
    public Entity Barbarian;
    public Entity Rogue;
    public Entity Warlock;
}

public struct FiniteWaveSpawner : IComponentData { }
public struct FiniteTimeSpawner : IComponentData { }
public struct InfiniteTimeSpawner : IComponentData { };