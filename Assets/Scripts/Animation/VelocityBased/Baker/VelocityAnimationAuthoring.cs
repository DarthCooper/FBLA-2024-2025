using Unity.Entities;
using UnityEngine;

class VelocityAnimationAuthoring : MonoBehaviour
{
    public GameObject animationController;

    public bool flipViaDir = false;
    public GameObject[] graphics;
}

class VelocityAnimationAuthoringBaker : Baker<VelocityAnimationAuthoring>
{
    public override void Bake(VelocityAnimationAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new AnimationController 
        {
            Value = GetEntity(authoring.animationController, TransformUsageFlags.Dynamic),
        });
        AddComponent<VelocityAnimationController>(entity);
        AddComponent(entity, new FlipViaDir
        {
            Value = authoring.flipViaDir,
        });

        DynamicBuffer<AnimationGraphics> graphics = AddBuffer<AnimationGraphics>(entity);
        foreach(var graphic in authoring.graphics)
        {
            graphics.Add(new AnimationGraphics
            {
                Value = GetEntity(graphic, TransformUsageFlags.NonUniformScale),
            });
        }
    }
}
