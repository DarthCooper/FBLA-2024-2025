using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;
using NUnit.Framework.Constraints;

partial class UnitMoveOrderSystem : SystemBase
{
    public float updateDelay = 0.5f;
    protected override void OnCreate()
    {
        RequireForUpdate<EnemyTag>();
    }

    protected override void OnUpdate()
    {
        updateDelay -= SystemAPI.Time.DeltaTime;
        if(updateDelay <= 0)
        {
            /*
            Entities.WithAll<EnemyTag>().WithStructuralChanges().ForEach((Entity entity, ref LocalTransform transform, ref PathFollowTarget target) =>
            {
                float3 pos = new float3 { x = transform.Position.x, y = transform.Position.z, z = 0 };
                Grid<GridNode> grid = GridSystem.instance.grid;

                float cellSize = grid.GetCellSize();

                grid.GetXY(pos + new float3(1, 0, 1) * cellSize * .5f, out int startX, out int startY);

                float3 playerPos = GetPlayerPos();
                float3 convertedPlayerPos = new float3 { x = playerPos.x, y = playerPos.z, z = 0 };

                grid.GetXY(convertedPlayerPos + new float3(1, 0, 1) * cellSize * .5f, out int endX, out int endY);

                ValidateGridPosition(ref startX, ref startY, grid);
                ValidateGridPosition(ref endX, ref endY, grid);
                EntityManager.AddComponentData(entity, new PathfindingParams
                {
                    startPosition = new int2(startX, startY),
                    endPosition = new int2(endX, endY)
                });

                target.Value = GetPlayer();
            }).Run();

            updateDelay = 0.5f;
            */
        }
    }

    public float3 GetPlayerPos()
    {
        Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
        LocalToWorld playerTransform = SystemAPI.GetComponent<LocalToWorld>(playerEntity);
        return playerTransform.Position;
    }

    public Entity GetPlayer()
    {
        return SystemAPI.GetSingletonEntity<PlayerTag>();
    }

    private void ValidateGridPosition(ref int x, ref int y, Grid<GridNode> grid)
    {
        x = math.clamp(x, 0, grid.GetWidth() - 1);
        y = math.clamp(y, 0, grid.GetHeight() - 1);
    }
}
