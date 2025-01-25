using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

class MaterialAnimationAuthoring : MonoBehaviour
{
    public MaterialAnimationFrameSetter[] animationFrames;
    public bool looping = true;

    public bool startEnabled = true;

    public GameObject staticMaterial;
}

[Serializable]
class MaterialAnimationFrameSetter
{
    public GameObject gameObject;
    public float time;
}

class MaterialAnimationAuthoringBaker : Baker<MaterialAnimationAuthoring>
{
    public override void Bake(MaterialAnimationAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);

        DynamicBuffer<MaterialFrame> animationFrames = AddBuffer<MaterialFrame>(entity);
        foreach (var frame in authoring.animationFrames)
        {
            animationFrames.Add(new MaterialFrame
            {
                entity = GetEntity(frame.gameObject, TransformUsageFlags.Renderable),
                time = frame.time,
                maxTime = frame.time,
            });
        }
        AddComponent(entity, new AnimationData
        {
            index = 0,
            looping = authoring.looping,
        });
        AddComponent(entity, new Playing
        {
            Value = authoring.startEnabled
        });

        if(authoring.staticMaterial)
        {
            AddComponent(entity, new DisabledAnimationData
            {
                entity = GetEntity(authoring.staticMaterial, TransformUsageFlags.Renderable)
            });
        }
    }
}
