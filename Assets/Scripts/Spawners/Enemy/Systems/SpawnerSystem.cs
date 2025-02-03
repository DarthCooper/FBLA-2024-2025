using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

partial class SpawnerSystem : SystemBase
{
    private EntityCommandBufferSystem _ecbSystem;

    protected override void OnCreate()
    {
        _ecbSystem = World.GetOrCreateSystemManaged<EntityCommandBufferSystem>();
    }

    protected override void OnStartRunning()
    {
        Entities.WithAll<InfiniteTimeSpawner>().ForEach((Entity entity, int entityInQueryIndex, ref Spawners spawner) =>
        {
            ref SpawnerArrays spawnArrays = ref spawner.BlobAssetReference.Value;
            for (int i = 0; i < spawnArrays.blob.Length; i++)
            {
                ref SpawnerData data = ref spawnArrays.blob[i];
                data.random = Unity.Mathematics.Random.CreateFromIndex((uint)i);
            }
        }).ScheduleParallel();
        Entities.WithAll<FiniteWaveSpawner>().ForEach((Entity entity, int entityInQueryIndex, ref WaveSpawners spawner) =>
        {
            ref EnemyWaveData waveData = ref spawner.BlobAssetReference.Value;
            for(int i = 0; i < waveData.BlobArray.Length; i++)
            {
                ref SpawnerArrays spawnArrays = ref waveData.BlobArray[i];
                for (int j = 0; j < spawnArrays.blob.Length; j++)
                {
                    ref SpawnerData data = ref spawnArrays.blob[j];
                    data.random = Unity.Mathematics.Random.CreateFromIndex((uint)j);
                }
            }
        }).ScheduleParallel();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();

        SystemAPI.TryGetSingletonEntity<PlayerTag>(out Entity player);

        Entities.WithAll<CanSpawn>().WithAll<InfiniteTimeSpawner>().ForEach((Entity entity, int entityInQueryIndex, ref EnemyTypes enemyTypes, ref Spawners spawner) =>
        {
            if(player.Equals(Entity.Null)) { return; }

            ref SpawnerArrays spawnArrays = ref spawner.BlobAssetReference.Value;
            for (int i = 0; i < spawnArrays.blob.Length; i++)
            {
                ref SpawnerData data = ref spawnArrays.blob[i];

                if (data.delay >= 0)
                {
                    data.delay -= SystemAPI.Time.DeltaTime;
                    continue;
                }

                for(int j = 0; j < data.amountPerSpawn; j++)
                {
                    data.random = Unity.Mathematics.Random.CreateFromIndex((uint)math.pow(j, j * i));

                    float3 pos = data.pos + new float3
                    {
                        x = data.random.NextFloat(-data.radius, data.radius), 
                        y = 0,
                        z = data.random.NextFloat(-data.radius, data.radius)
                    };
                    ref BlobArray<Enemies> enemies = ref data.enemies;

                    if (enemies.Length > 0)
                    {
                        Enemies enemyType = enemies[data.random.NextInt(0, enemies.Length)];
                        data.random = Unity.Mathematics.Random.CreateFromIndex((uint)i + (uint)data.random.NextInt(0, 100));
                        Entity enemy = Entity.Null;
                        switch(enemyType)
                        {
                            case Enemies.BARBARIAN:
                                enemy = enemyTypes.Barbarian;
                                break;
                            case Enemies.FIGHTER:
                                enemy = enemyTypes.Fighter;
                                break;
                            case Enemies.RANGER:
                                enemy = enemyTypes.Ranger;
                                break;
                            case Enemies.ROGUE:
                                enemy = enemyTypes.Rogue;
                                break;
                            case Enemies.WARLOCK:
                                enemy = enemyTypes.Warlock;
                                break;
                            default:
                                enemy = enemyTypes.Fighter;
                                break;
                        }
                        ecb.Instantiate(entityInQueryIndex, enemy);
                        ecb.SetComponent(entityInQueryIndex, enemy, new LocalTransform
                        {
                            Position = pos,
                            Rotation = Quaternion.identity,
                            Scale = 1
                        });
                        ecb.SetComponent(entityInQueryIndex, enemy, new PathFollowTarget
                        {
                            Value = player
                        });
                    }
                }
                data.delay = data.maxDelay;
            }
        }).ScheduleParallel();
        Entities.WithAll<CanSpawn>().WithAll<FiniteWaveSpawner>().ForEach((Entity entity, int entityInQueryIndex, ref EnemyTypes enemyTypes, ref WaveSpawners spawner) =>
        {
            ref EnemyWaveData waveData = ref spawner.BlobAssetReference.Value;
            int i = waveData.waveIndex;
            if(i >= waveData.BlobArray.Length) { ecb.RemoveComponent<CanSpawn>(entityInQueryIndex, entity); return; }
            ref SpawnerArrays spawnArrays = ref waveData.BlobArray[i];

            bool waveSpawned = true;

            if (spawnArrays.delay >= 0)
            {
                spawnArrays.delay -= SystemAPI.Time.DeltaTime;
                waveSpawned = false;
                return;
            }

            for (int j = 0; j < spawnArrays.blob.Length; j++)
            {
                ref SpawnerData data = ref spawnArrays.blob[j];

                if(data.spawned) { continue; }

                if (data.delay >= 0)
                {
                    data.delay -= SystemAPI.Time.DeltaTime;
                    waveSpawned = false;
                    continue;
                }

                for (int k = 0; k < data.amountPerSpawn; k++)
                {
                    data.random = Unity.Mathematics.Random.CreateFromIndex((uint)math.pow(i, math.pow(j, k)));

                    float3 pos = data.pos + new float3
                    {
                        x = data.random.NextFloat(-data.radius, data.radius),
                        y = 0,
                        z = data.random.NextFloat(-data.radius, data.radius)
                    };

                    ref BlobArray<Enemies> enemies = ref data.enemies;

                    if (enemies.Length > 0)
                    {
                        Enemies enemyType = enemies[data.random.NextInt(0, enemies.Length)];
                        data.random = Unity.Mathematics.Random.CreateFromIndex((uint)j + (uint)data.random.NextInt(0, 100));
                        Entity enemy = Entity.Null;
                        switch (enemyType)
                        {
                            case Enemies.BARBARIAN:
                                enemy = enemyTypes.Barbarian;
                                break;
                            case Enemies.FIGHTER:
                                enemy = enemyTypes.Fighter;
                                break;
                            case Enemies.RANGER:
                                enemy = enemyTypes.Ranger;
                                break;
                            case Enemies.ROGUE:
                                 enemy = enemyTypes.Rogue;
                                 break;
                            case Enemies.WARLOCK:
                                 enemy = enemyTypes.Warlock;
                                 break;
                            default:
                                 enemy = enemyTypes.Fighter;
                                 break;
                            }
                        Entity spawnedEnemy = ecb.Instantiate(entityInQueryIndex, enemy);
                        ecb.SetComponent(entityInQueryIndex, spawnedEnemy, new LocalTransform
                        {
                            Position = pos,
                            Rotation = Quaternion.identity,
                            Scale = 1
                        });
                        ecb.SetComponent(entityInQueryIndex, spawnedEnemy, new PathFollowTarget
                        {
                            Value = player
                        });
                        ecb.AddComponent(entityInQueryIndex, spawnedEnemy, new Stunned
                        {
                            Value = 1f
                        });
                    }
                }
                data.spawned = true;
                data.delay = data.maxDelay;
            }
            if(waveSpawned)
            {
                waveData.waveIndex++;
            }
        }).ScheduleParallel();
    }
}
