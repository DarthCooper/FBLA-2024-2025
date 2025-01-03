using System;
using Unity.Entities;
using UnityEngine;

class NPCDeActivationBaker : MonoBehaviour
{
    public GameObject[] deactivateObjects;
}

class NPCDeActivationBakerBaker : Baker<NPCDeActivationBaker>
{
    public override void Bake(NPCDeActivationBaker authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        DynamicBuffer<DeActivateNPCElement> buffer = AddBuffer<DeActivateNPCElement>(entity); 
        foreach(GameObject gameObject in authoring.deactivateObjects)
        {
            buffer.Add(new DeActivateNPCElement
            {
                entity = GetEntity(gameObject, TransformUsageFlags.Dynamic)
            });
        }
    }
}
