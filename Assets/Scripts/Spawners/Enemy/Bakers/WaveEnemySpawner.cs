using JetBrains.Annotations;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

class WaveEnemySpawner : MonoBehaviour
{
    [SerializeField]
    public EnemyWaves[] spawns;

    public GameObject Ranger;
    public GameObject Fighter;
    public GameObject Barbarian;
    public GameObject Rogue;
    public GameObject Warlock;
}

[Serializable]
public class EnemyWaves
{
    [SerializeField] public WaveSpawnerData[] spawns;

    public float delay;
}

[Serializable]
public class WaveSpawnerData
{
    public float3 pos;
    public float delay;
    public float spawnRadius;
    public Enemies[] enemyTypes;
    public int amountPerSpawn;
}

class WaveEnemySpawnerBaker : Baker<WaveEnemySpawner>
{
    public override void Bake(WaveEnemySpawner authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);

        var builder = new BlobBuilder(Allocator.Temp);

        ref EnemyWaveData enemyWavePool = ref builder.ConstructRoot<EnemyWaveData>();

        enemyWavePool.waveIndex = 0;

        var enemyWaveArray = builder.Allocate(ref enemyWavePool.BlobArray, authoring.spawns.Length);
        for(int i = 0; i < enemyWaveArray.Length; i++)
        {
            ref SpawnerArrays spawnerPool = ref enemyWaveArray[i];

            spawnerPool.delay = authoring.spawns[i].delay;

            var spawnDataArray = builder.Allocate(ref spawnerPool.blob, authoring.spawns[i].spawns.Length);
            for (int j = 0; j < spawnDataArray.Length; j++)
            {
                ref SpawnerData data = ref spawnDataArray[j];

                data.pos = authoring.spawns[i].spawns[j].pos;
                data.amountPerSpawn = authoring.spawns[i].spawns[j].amountPerSpawn;
                data.maxDelay = authoring.spawns[i].spawns[j].delay;
                data.delay = authoring.spawns[i].spawns[j].delay;
                data.radius = authoring.spawns[i].spawns[j].spawnRadius;

                var enemyTypes = builder.Allocate(ref data.enemies, authoring.spawns[i].spawns[j].enemyTypes.Length);
                for (int k = 0; k < enemyTypes.Length; k++)
                {
                    ref Enemies enemy = ref enemyTypes[k];
                    enemy = authoring.spawns[i].spawns[j].enemyTypes[k];
                }
            }
        }

        var blobReference = builder.CreateBlobAssetReference<EnemyWaveData>(Allocator.Persistent);
        builder.Dispose();

        AddBlobAsset<EnemyWaveData>(ref blobReference, out var hash);

        AddComponent(entity, new WaveSpawners
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

        AddComponent<FiniteWaveSpawner>(entity);
    }
}
