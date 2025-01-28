using Unity.Entities;
using UnityEngine;

class EnemyAuthoring : MonoBehaviour
{
    
}

class EnemyAuthoringBaker : Baker<EnemyAuthoring>
{
    public override void Bake(EnemyAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent<EnemyTag>(entity);
        AddComponent(entity, new CurDir
        {
            Value = Directions.RIGHT
        });
    }
}
