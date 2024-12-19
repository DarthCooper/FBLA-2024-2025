using Unity.Entities;
using UnityEngine;

class NPCBaker : MonoBehaviour
{
    
}

class NPCBakerBaker : Baker<NPCBaker>
{
    public override void Bake(NPCBaker authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent<NPCTag>(entity);
        AddComponent<Ally>(entity);
    }
}
