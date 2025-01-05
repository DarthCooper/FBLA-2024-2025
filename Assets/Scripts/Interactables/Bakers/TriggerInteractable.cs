using Unity.Entities;
using UnityEngine;

class TriggerInteractable : MonoBehaviour
{
    public InteractableType interactableType;
}

class TriggerInteractableBaker : Baker<TriggerInteractable>
{
    public override void Bake(TriggerInteractable authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent<TriggerInteractableTag>(entity);
        AddComponent(entity, new InteractableTypeData
        {
            Value = authoring.interactableType
        });
    }
}
