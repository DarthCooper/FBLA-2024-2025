using Unity.Entities;
using UnityEngine;

class HealthAuthoring : MonoBehaviour
{
    public float maxHealth;
}

class HealthAuthoringBaker : Baker<HealthAuthoring>
{
    public override void Bake(HealthAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new MaxHealth
        {
            Value = authoring.maxHealth
        });
        AddComponent(entity, new Health
        {
            Value = authoring.maxHealth
        });
    }
}
