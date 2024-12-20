using Unity.Entities;
using UnityEngine;

class InteractableBaker : MonoBehaviour
{
    public InteractableType interactableType;
}

class InteractableBakerBaker : Baker<InteractableBaker>
{
    public override void Bake(InteractableBaker authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent<InteractableTag>(entity);
        AddComponent(entity, new InteractableTypeData
        {
            Value = authoring.interactableType
        });
    }
}
