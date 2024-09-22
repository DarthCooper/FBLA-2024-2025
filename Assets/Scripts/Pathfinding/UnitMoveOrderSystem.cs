using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;

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
        if(Input.GetMouseButtonDown(0))
        {
            Entities.WithAll<EnemyTag>().WithStructuralChanges().ForEach((Entity entity, ref LocalTransform transform) =>
            {
                float3 pos = new float3 { x = transform.Position.x, y = transform.Position.z, z = 0 };
                Grid<GridNode> grid = GridSystem.instance.grid;

                float cellSize = grid.GetCellSize();

                grid.GetXY(pos + new float3(1, 1, 0) * cellSize * .5f, out int startX, out int startY);

                ValidateGridPosition(ref startX, ref startY, grid);
                EntityManager.AddComponentData(entity, new PathfindingParams
                {
                    startPosition = new int2(startX, startY),
                    endPosition = new int2(15, 7)
                });
            }).Run();

            updateDelay = 0.5f;
        }
    }

    private void ValidateGridPosition(ref int x, ref int y, Grid<GridNode> grid)
    {
        x = math.clamp(x, 0, grid.GetWidth() - 1);
        y = math.clamp(y, 0, grid.GetHeight() - 1);
    }
}
