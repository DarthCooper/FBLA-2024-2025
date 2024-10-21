using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

class StaticObstacleAuthoring : MonoBehaviour
{
    public int3 Aura;
}

class StaticObstacleAuthoringBaker : Baker<StaticObstacleAuthoring>
{
    public override void Bake(StaticObstacleAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent<StaticObstacleTag>(entity);
        AddComponent(entity, new Aura
        {
            Value = authoring.Aura
        });
        AddBuffer<TakenCells>(entity);
    }
}
