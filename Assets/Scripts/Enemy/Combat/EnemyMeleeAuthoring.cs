using Unity.Entities;
using UnityEngine;

class EnemyMeleeAuthoring : MonoBehaviour
{
    public GameObject animObject;
    public GameObject pivotObject;
    public float delay = 4f;
}

class EnemyMeleeAuthoringBaker : Baker<EnemyMeleeAuthoring>
{
    public override void Bake(EnemyMeleeAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new MeleeAttacks
        {
            animEntity = GetEntity(authoring.animObject, TransformUsageFlags.Dynamic),
            pivotEntity = GetEntity(authoring.pivotObject, TransformUsageFlags.Dynamic),
            delay = 0,
            maxDelay = authoring.delay
        });
    }
}
