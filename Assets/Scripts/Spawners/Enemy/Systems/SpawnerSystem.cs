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
        Entities.ForEach((Entity entity, int entityInQueryIndex, ref Spawners spawner) =>
        {
            ref SpawnerArrays spawnArrays = ref spawner.BlobAssetReference.Value;
            for (int i = 0; i < spawnArrays.blob.Length; i++)
            {
                ref SpawnerData data = ref spawnArrays.blob[i];
                data.random = Unity.Mathematics.Random.CreateFromIndex((uint)i);
            }
        }).ScheduleParallel();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();

        Entity player = SystemAPI.GetSingletonEntity<PlayerTag>();

        Entities.ForEach((Entity entity, int entityInQueryIndex, ref EnemyTypes enemyTypes, ref Spawners spawner) =>
        {
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
        }).Schedule();
    }
}
