using Unity.Entities;
using UnityEngine;

class StaticObstacleAuthoring : MonoBehaviour
{
    
}

class StaticObstacleAuthoringBaker : Baker<StaticObstacleAuthoring>
{
    public override void Bake(StaticObstacleAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent<StaticObstacleTag>(entity);
        AddBuffer<TakenCells>(entity);
    }
}
