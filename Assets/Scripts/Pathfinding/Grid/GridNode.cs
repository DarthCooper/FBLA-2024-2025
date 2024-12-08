using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class GridNode
{
    private Grid<GridNode> grid;
    private int x;
    private int y;

    private bool isWalkable;

    private Entity allowedEntity;

    public GridNode(Grid<GridNode> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        isWalkable = true;
    }

    public bool IsWalkable()
    {
        return isWalkable;
    }

    public void SetIsWalkable(bool isWalkable)
    {
        this.isWalkable = isWalkable;
        grid.TriggerGridObjectChanged(x, y);
    }

    public Entity GetAllowedEntity()
    {
        if (allowedEntity == null)
        {
            return Entity.Null;
        }else
        {
            return allowedEntity;
        }
    }

    public void SetAllowedEntity(Entity entity)
    {
        allowedEntity = entity;
    }

    public int2 GetPos()
    {
        return new int2 { x = x, y = y };
    }
}
