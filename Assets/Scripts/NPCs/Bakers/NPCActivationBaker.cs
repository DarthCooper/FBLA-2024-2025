using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

class NPCActivationBaker : MonoBehaviour
{
    [SerializeField] public NPCActivationSetter[] NPCs;
}

[Serializable]
public class NPCActivationSetter
{
    public GameObject Object;
    public GameObject Target;
    public bool CanAttack;
    public float dist;
}

class NPCActivationBakerBaker : Baker<NPCActivationBaker>
{
    public override void Bake(NPCActivationBaker authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        DynamicBuffer<ActivateNPCElement> activateBuffer = AddBuffer<ActivateNPCElement>(entity);
        for(int i = 0; i < authoring.NPCs.Length; i++)
        {
            activateBuffer.Add(new ActivateNPCElement
            {
                entity = GetEntity(authoring.NPCs[i].Object, TransformUsageFlags.Dynamic),
                target = GetEntity(authoring.NPCs[i].Target, TransformUsageFlags.Dynamic),
                CanAttack = authoring.NPCs[i].CanAttack,
                followDist = authoring.NPCs[i].dist
            });
        }
    }
}
