using Unity.Entities;
using UnityEngine;

class PlayerAuthoring : MonoBehaviour
{
    public float MoveSpeed;
}

class PlayerAuthoringBaker : Baker<PlayerAuthoring>
{
    public override void Bake(PlayerAuthoring authoring)
    {
        Entity playerEntity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent<PlayerTag>(playerEntity);
        AddComponent<PlayerMoveInput>(playerEntity);

        AddComponent(playerEntity, new PlayerMoveSpeed
        {
            Value = authoring.MoveSpeed,
        });
    }
}
