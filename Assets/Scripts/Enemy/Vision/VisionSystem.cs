using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Stateful;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

partial struct VisionSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<VisionEntity>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach((VisionEntity visionEntity, PhysicsVelocity velocity ,Entity entity) in SystemAPI.Query<VisionEntity, PhysicsVelocity>().WithEntityAccess())
        {
            RefRW<LocalTransform> visTransform = SystemAPI.GetComponentRW<LocalTransform>(visionEntity.Value);

            float3 dir = velocity.Linear;
            Quaternion rot = Quaternion.Euler(dir);
            visTransform.ValueRW.Rotate(rot);
        }

        foreach (var (triggerEventBuffer, parent, entity) in SystemAPI.Query<DynamicBuffer<StatefulTriggerEvent>, Parent>().WithEntityAccess())
        {
            for (int i = 0; i < triggerEventBuffer.Length; i++)
            {
                var triggerEvent = triggerEventBuffer[i];
                var otherEntity = triggerEvent.GetOtherEntity(entity);

                if (!state.EntityManager.HasComponent<PlayerTag>(otherEntity)) { continue; }

                if (triggerEvent.State == StatefulEventState.Stay)
                {
                    continue;
                }

                if (triggerEvent.State == StatefulEventState.Enter)
                {
                    ecb.RemoveComponent<Scouting>(entity);
                    continue;
                }

                if(triggerEvent.State == StatefulEventState.Exit)
                {
                    continue;
                }
            }
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
