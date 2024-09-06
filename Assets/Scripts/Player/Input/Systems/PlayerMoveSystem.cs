using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

partial struct PlayerMoveSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerMoveInput>();
        state.RequireForUpdate<PlayerMoveSpeed>();
        state.RequireForUpdate<PhysicsVelocity>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach(var (velocity, moveInput, moveSpeed, entity) in SystemAPI.Query<RefRW<PhysicsVelocity>, PlayerMoveInput, PlayerMoveSpeed>().WithEntityAccess())
        {
            float2 moveVector = moveInput.Value * moveSpeed.Value;
            velocity.ValueRW.Linear = new float3(moveVector.x, 0, moveVector.y);
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }

    public partial struct PlayerMoveJob : IJobEntity
    {
        private void Execute(ref PhysicsVelocity velocity, PlayerMoveInput moveInput, PlayerMoveSpeed moveSpeed)
        {
            float2 moveVector = moveInput.Value * moveSpeed.Value;
            velocity.Linear = new float3(moveVector.x, 0, moveVector.y);
        }
    }
}
