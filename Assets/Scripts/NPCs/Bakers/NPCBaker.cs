using Unity.Entities;
using UnityEngine;

class NPCBaker : MonoBehaviour
{
    public bool startActive = true;
}

class NPCBakerBaker : Baker<NPCBaker>
{
    public override void Bake(NPCBaker authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent<NPCTag>(entity);
        AddComponent<Ally>(entity);
        AddComponent(entity, new CurDir
        {
            Value = Directions.RIGHT
        });
        if(authoring.startActive)
        {
            AddComponent<Active>(entity);
        }else
        {
            AddComponent<DeActive>(entity);
        }
    }
}
