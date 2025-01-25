using Unity.Entities;
using UnityEngine;

class MatchEntityAuthoring : MonoBehaviour
{
    public GameObject leader;
}

class MatchEntityAuthoringBaker : Baker<MatchEntityAuthoring>
{
    public override void Bake(MatchEntityAuthoring authoring)
    {
        Entity followerEntity = GetEntity(TransformUsageFlags.Dynamic);
        Entity leaderEntity = GetEntity(authoring.leader, TransformUsageFlags.Dynamic);

        AddComponent(followerEntity, new FollowerEntity
        {
            Value = leaderEntity
        });
    }
}
