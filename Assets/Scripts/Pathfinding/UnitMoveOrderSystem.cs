using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;

partial class UnitMoveOrderSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<EnemyTag>();
    }

    protected override void OnUpdate()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Entities.WithAll<EnemyTag>().WithStructuralChanges().ForEach((Entity entity, ref LocalTransform transform) =>
            {
                EntityManager.AddComponentData(entity, new PathfindingParams
                {
                    startPosition = new int2(0, 0),
                    endPosition = new int2(4, 0)
                });
            }).Run();
        }
    }
}
