using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct Spawners : IComponentData
{
    public BlobAssetReference<SpawnerArrays> BlobAssetReference;
}

public struct SpawnerArrays
{
    public BlobArray<SpawnerData> blob;
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
}

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
