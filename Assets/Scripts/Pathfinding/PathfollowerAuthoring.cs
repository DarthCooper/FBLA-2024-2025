using Unity.Entities;
using UnityEngine;

class PathfollowerAuthoring : MonoBehaviour
{
    [Header("Base Stats")]
    public GameObject startingTarget;
    public float moveSpeed;
    public float targetDistance;

    [Header("Scouting")]
    public bool scouts = true;
    public float maxScoutDistance;
    public float minScoutDistance;

    [Header("Retreats - via distance")]
    public bool retreats;
    public float maxRetreatDistance;
    public float minRetreatDistance;
    public float triggerDistance;


}

class PathfollowerAuthoringBaker : Baker<PathfollowerAuthoring>
{
    public override void Bake(PathfollowerAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(entity, new PathFollowSpeed
        {
            Value = authoring.moveSpeed
        });
        AddComponent(entity, new PathFollowTargetDistance
        {
            Value = authoring.targetDistance
        });
        AddComponent(entity, new IsFollowing
        {
            Value = false
        });

        AddComponent(entity, new PathFollowerPreviousTarget
        {
            Value = Entity.Null
        });
        AddComponent(entity, new PathFollowerPreviousTargetDistance
        {
            Value = 1f
        });

        if (authoring.startingTarget != null)
        {
            AddComponent(entity, new PathFollowTarget
            {
                Value = GetEntity(authoring.startingTarget, TransformUsageFlags.Dynamic)
            });
        }else
        {
            AddComponent(entity, new PathFollowTarget
            {
                Value = Entity.Null
            });
        }
        if (authoring.retreats)
        {
            AddComponent(entity, new PathFollowRetreatDistances
            {
                Max = authoring.maxRetreatDistance,
                Min = authoring.minRetreatDistance,
                Trigger = authoring.triggerDistance
            });
        }
        if(authoring.scouts)
        {
            AddComponent(entity, new PathFollowerScoutingDistances
            {
                Max = authoring.maxScoutDistance,
                Min = authoring.minScoutDistance
            });
        }
    }
}
