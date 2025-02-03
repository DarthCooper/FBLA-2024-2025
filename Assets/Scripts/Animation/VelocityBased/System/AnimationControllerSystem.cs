using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public partial class AnimationControllerSystem : SystemBase
{
    private EntityCommandBufferSystem _ecbSystem;

    protected override void OnCreate()
    {
        _ecbSystem = World.GetOrCreateSystemManaged<EntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        ComponentLookup<Playing> animationsPlaying = SystemAPI.GetComponentLookup<Playing>();
        ComponentLookup<LocalTransform> transforms = SystemAPI.GetComponentLookup<LocalTransform>();

        EntityCommandBuffer.ParallelWriter ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();

        Entities.WithAll<VelocityAnimationController>().ForEach((Entity entity, int entityInQueryIndex, ref AnimationController controller, ref FlipViaDir flip, ref DynamicBuffer<AnimationGraphics> graphics, ref CurDir dir, ref PhysicsVelocity velocity) =>
        {
            bool playing = false;
            if(animationsPlaying.HasComponent(controller.Value)) { playing = animationsPlaying[controller.Value].Value; }

            if(flip.Value && graphics.Length > 0 && Manager.GetMagnitude(velocity.Linear) > 1)
            {
                float xVel = velocity.Linear.x;
                switch (dir.Value)
                {
                    case Directions.RIGHT:
                        if(xVel >= 0) { break; }
                        ecb.SetComponent(entityInQueryIndex, entity, new CurDir
                        {
                            Value = Directions.LEFT,
                        });
                        foreach(AnimationGraphics graphic in graphics)
                        {
                            LocalTransform transform = transforms[graphic.Value];
                            ecb.SetComponent(entityInQueryIndex, graphic.Value, new LocalTransform
                            {
                                Position = transform.Position,
                                Rotation = Quaternion.Euler(0, 180, 0),
                                Scale = transform.Scale
                            });
                        }
                        break;
                    case Directions.LEFT:
                        if(xVel <= 0) { break; }
                        ecb.SetComponent(entityInQueryIndex, entity, new CurDir
                        {
                            Value = Directions.RIGHT,
                        });
                        foreach (AnimationGraphics graphic in graphics)
                        {
                            LocalTransform transform = transforms[graphic.Value];
                            ecb.SetComponent(entityInQueryIndex, graphic.Value, new LocalTransform
                            {
                                Position = transform.Position,
                                Rotation = Quaternion.Euler(0, 0, 0),
                                Scale = transform.Scale
                            });
                        }
                        break;
                }
            }

            if (Manager.GetMagnitude(velocity.Linear) > 1 && !playing)
            {
                ecb.SetComponent(entityInQueryIndex, controller.Value, new Playing
                {
                    Value = true
                });
            }

            if (Manager.GetMagnitude(velocity.Linear) <= 1 && playing)
            {
                ecb.SetComponent(entityInQueryIndex, controller.Value, new Playing
                {
                    Value = false
                });
            }
        }).Schedule();
    }
}
