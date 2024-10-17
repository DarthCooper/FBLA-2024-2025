using Unity.Entities;
using UnityEngine;

class MovingObstacleAuthoring : MonoBehaviour
{
    
}

class MovingObstacleAuthoringBaker : Baker<MovingObstacleAuthoring>
{
    public override void Bake(MovingObstacleAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent<MovingObstacleTag>(entity);
        AddBuffer<TakenCells>(entity);
    }
}
