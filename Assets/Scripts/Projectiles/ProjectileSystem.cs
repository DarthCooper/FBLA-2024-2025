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

                // exclude other triggers and processed events
                if (colliderEvent.State == StatefulEventState.Enter)
                {
                    if (state.EntityManager.HasComponent<Health>(otherEntity))
                    {
                        RefRW<Health> health = SystemAPI.GetComponentRW<Health>(otherEntity);
                        health.ValueRW.Value -= damage.Damage;
                    }
                    if (state.EntityManager.HasComponent<PhysicsVelocity>(otherEntity) && state.EntityManager.HasComponent<RangedProjectileKnockback>(entity))
                    {
                        ecb.AddComponent<ApplyKnockBack>(otherEntity);
                        ecb.AddComponent(otherEntity, new KnockBackDir
                        {
                            Value = dir.Value,
                        });
                        ecb.AddComponent(otherEntity, new KnockBackStrength
                        {
                            Value = state.EntityManager.GetComponentData<RangedProjectileKnockback>(entity).Value
                        });
                        ecb.AddComponent(otherEntity, new Stunned
                        {
                            Value = 2f
                        });
                    }
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
