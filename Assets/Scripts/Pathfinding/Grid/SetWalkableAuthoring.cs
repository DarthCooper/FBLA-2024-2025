using Unity.Entities;
using UnityEngine;

class SetWalkableAuthoring : MonoBehaviour
{
    public WalkableTypes walkableTypes;
}

public enum WalkableTypes
{
    walkable,
    nonWalkable,
    nonWalkableAllower,
}

class SetWalkableAuthoringBaker : Baker<SetWalkableAuthoring>
{
    public override void Bake(SetWalkableAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(entity, new IsWalkable
        {
            Value = authoring.walkableTypes
        });

    }
}
