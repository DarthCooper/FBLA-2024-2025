using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

public partial class NPCSpawnerSystem : SystemBase
{
    private EntityCommandBufferSystem _ecbSystem;

    protected override void OnCreate()
    {
        _ecbSystem = World.GetOrCreateSystemManaged<EntityCommandBufferSystem>();
    }

    protected override void OnStartRunning()
    {
        Entities.ForEach((Entity entity, int entityInQueryIndex, ref NPCSpawners spawner) =>
        {
            ref NPCSpawnerData data = ref spawner.BlobAssetReference.Value;
            data.random = Unity.Mathematics.Random.CreateFromIndex((uint) entityInQueryIndex);
        }).ScheduleParallel();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();

        Entities.WithNone<NPCsSpawned>().ForEach((Entity entity, int entityInQueryIndex, ref NPCSpawners spawner, ref NPCTypes npcTypes) =>
        {
            ref NPCSpawnerData data = ref spawner.BlobAssetReference.Value;
            float3 pos = data.pos + new float3
            {
                x = data.random.NextFloat(-data.radius, data.radius),
                y = 0,
                z = data.random.NextFloat(-data.radius, data.radius)
            };
            ref BlobArray<NPCs> npcs = ref data.npcs;

            for (int i = 0; i < npcs.Length; i++)
            {
                ref NPCs npcType = ref npcs[i];

                Entity npc = Entity.Null;

                switch (npcType)
                {
                    case NPCs.BARBARIAN:
                        npc = npcTypes.Barbarian;
                        break;
                    case NPCs.CLERIC:
                        npc = npcTypes.Cleric;
                        break;
                    case NPCs.RANGER:
                        npc = npcTypes.Ranger;
                        break;
                    case NPCs.ROGUE:
                        npc = npcTypes.Rogue;
                        break;
                    case NPCs.WARLOCK:
                        npc = npcTypes.Warlock;
                        break;
                    default:
                        npc = npcTypes.Rogue;
                        break;
                }
                ecb.Instantiate(entityInQueryIndex, npc);
                ecb.SetComponent(entityInQueryIndex, npc, new LocalTransform
                {
                    Position = pos,
                    Rotation = Quaternion.identity,
                    Scale = 1
                });
            }

            ecb.AddComponent<NPCsSpawned>(entityInQueryIndex, entity);
        }).Schedule();
    }
}
