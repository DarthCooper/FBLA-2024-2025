using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Stateful;

partial struct ProjectileSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ProjectileTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach((ProjectileSpeed speed, ProjectileDirection dir, ProjectileParent parent, ProjectileDamage damage ,RefRW<PhysicsVelocity> velocity, DynamicBuffer<StatefulTriggerEvent> triggerEventBuffer, Entity entity) in SystemAPI.Query<ProjectileSpeed, ProjectileDirection, ProjectileParent, ProjectileDamage, RefRW<PhysicsVelocity>, DynamicBuffer<StatefulTriggerEvent>>().WithEntityAccess())
        {
            velocity.ValueRW.Linear = dir.Value * speed.Speed;

            for (int i = 0; i < triggerEventBuffer.Length; i++)
            {
                var colliderEvent = triggerEventBuffer[i];
                var otherEntity = colliderEvent.GetOtherEntity(entity);

                if(otherEntity.Equals(parent.Value)) { continue; }
                if(state.EntityManager.HasComponent<Health>(otherEntity))
                {
                    RefRW<Health> health = SystemAPI.GetComponentRW<Health>(otherEntity);
                    if (colliderEvent.State == StatefulEventState.Enter)
                    {
                        health.ValueRW.Value -= damage.Damage;
                        ecb.DestroyEntity(entity);
                        continue;
                    }

                }

                // exclude other triggers and processed events
                if (colliderEvent.State == StatefulEventState.Enter)
                {
                    ecb.DestroyEntity(entity);
                }

            }
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
