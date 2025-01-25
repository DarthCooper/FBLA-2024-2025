using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public partial class MaterialAnimationSystem : SystemBase
{
    private EntityCommandBufferSystem _ecbSystem;

    protected override void OnCreate()
    {
        _ecbSystem = World.GetOrCreateSystemManaged<EntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();

        ComponentLookup<MaterialMeshInfo> meshLookup = SystemAPI.GetComponentLookup<MaterialMeshInfo>();
        ComponentLookup<DisabledAnimationData> staticAnimationData = SystemAPI.GetComponentLookup<DisabledAnimationData>();

        Entities.ForEach((Entity entity, int entityInQueryIndex, ref DynamicBuffer<MaterialFrame> animationFrames, ref AnimationData animationData, ref Playing playing) =>
        {
            if (!playing.Value && !staticAnimationData.HasComponent(entity)) { animationData.index = 0; return; }
            if (!playing.Value)
            {
                DisabledAnimationData disabledAnimationData = staticAnimationData[entity];
                ecb.SetComponent(entityInQueryIndex, entity, meshLookup[disabledAnimationData.entity]);
                animationData.index = 0;
                return;
            }

            int i = animationData.index;

            MaterialFrame frame = animationFrames[i];


            if(!meshLookup.HasComponent(frame.entity)) { return; }
            meshLookup.SetComponentEnabled(frame.entity, true);

            ecb.SetComponent(entityInQueryIndex, entity, meshLookup[frame.entity]);

            DynamicBuffer<MaterialFrame> frames = ecb.SetBuffer<MaterialFrame>(entityInQueryIndex, entity);

            if (frame.time <= 0)
            {
                i++;
                if(i >= animationFrames.Length)
                {
                    if(animationData.looping)
                    {
                        i = 0;
                    }else
                    {
                        playing.Value = false;
                    }
                }
                frame.time = frame.maxTime;
                animationData.index = i;
            }

            for (int j = 0; j < animationFrames.Length; j++)
            {
                if (j == i)
                {
                    frames.Add(new MaterialFrame
                    {
                        entity = frame.entity,
                        time = frame.time - SystemAPI.Time.DeltaTime * 20f,
                        maxTime = frame.maxTime,
                    });
                }
                else
                {
                    frames.Add(animationFrames[j]);
                }
            }
        }).Run();
    }
}
