using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

class PointSpawner : MonoBehaviour
{
    [SerializeField]
    public DelaySpawnerData[] spawns;

    public GameObject Ranger;
    public GameObject Fighter;
    public GameObject Barbarian;
    public GameObject Rogue;
    public GameObject Warlock;
}

[Serializable]
public class DelaySpawnerData
{
    public float3 pos;
    public float delay;
    public Enemies[] enemyTypes;
    public int amountPerSpawn;
}

class PointSpawnerBaker : Baker<PointSpawner>
{
    public override void Bake(PointSpawner authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);

        var builder = new BlobBuilder(Allocator.Temp);
        ref SpawnerArrays spawnerPool = ref builder.ConstructRoot<SpawnerArrays>();

        var spawnDataArray = builder.Allocate(ref spawnerPool.blob, authoring.spawns.Length);

        for (int i = 0; i < spawnDataArray.Length; i++)
        {
            ref SpawnerData data = ref spawnDataArray[i];

            data.pos = authoring.spawns[i].pos;
            data.amountPerSpawn = authoring.spawns[i].amountPerSpawn;
            data.maxDelay = authoring.spawns[i].delay;
            data.delay = authoring.spawns[i].delay;

            var enemyTypes = builder.Allocate(ref data.enemies, authoring.spawns[i].enemyTypes.Length);
            for(int j = 0; j < enemyTypes.Length; j++)
            {
                ref Enemies enemy = ref enemyTypes[j];
                enemy = authoring.spawns[i].enemyTypes[j];
            }
        }

        var blobReference = builder.CreateBlobAssetReference<SpawnerArrays>(Allocator.Persistent);
        builder.Dispose();

        AddBlobAsset<SpawnerArrays>(ref blobReference, out var hash);

        AddComponent(entity, new Spawners
        {
            BlobAssetReference = blobReference
        });

        AddComponent(entity, new EnemyTypes
        {
            Barbarian = GetEntity(authoring.Barbarian, TransformUsageFlags.Dynamic),
            Fighter = GetEntity(authoring.Fighter, TransformUsageFlags.Dynamic),
            Ranger = GetEntity(authoring.Ranger, TransformUsageFlags.Dynamic),
            Rogue = GetEntity(authoring.Rogue, TransformUsageFlags.Dynamic),
            Warlock = GetEntity(authoring.Warlock, TransformUsageFlags.Dynamic),
        });
    }
}
