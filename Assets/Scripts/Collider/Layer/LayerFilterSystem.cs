using Unity.Burst;
using Unity.Entities;
using Unity.Physics;

partial struct LayerFilterSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Dependency = new ChangeLayer().Schedule(state.Dependency);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }

    [WithNone(typeof(FilterSetTag))]
    partial struct ChangeLayer : IJobEntity
    {
        public void Execute(ref LayerFilterData data, ref PhysicsCollider collider)
        {
            if(collider.Value.Value.GetCollisionFilter().Equals(data.Value)) { return; }
            collider.Value.Value.SetCollisionFilter(data.Value);
        }
    }
}
