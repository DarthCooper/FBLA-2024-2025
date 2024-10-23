using Unity.Entities;
using UnityEngine;

class EnemyVisionAuthoring : MonoBehaviour
{
    public GameObject vision;
}

class EnemyVisionAuthoringBaker : Baker<EnemyVisionAuthoring>
{
    public override void Bake(EnemyVisionAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(entity, new VisionEntity
        {
            Value = GetEntity(authoring.vision, TransformUsageFlags.Dynamic)
        });
    }
}
