using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

class PlayerAuthoring : MonoBehaviour
{
    public float moveSpeed;

    public float sprintSpeed;

    public float jumpSpeed;

    public float4 playerChecksOffset;
}

class PlayerAuthoringBaker : Baker<PlayerAuthoring>
{
    public override void Bake(PlayerAuthoring authoring)
    {
        Entity playerEntity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent<PlayerTag>(playerEntity);
        AddComponent<PlayerMoveInput>(playerEntity);
        AddComponent<PlayerSprintInput>(playerEntity);
        AddComponent<PlayerJumpInput>(playerEntity);
        AddComponent<PlayerChecks>(playerEntity);
        AddComponent(playerEntity, new PlayerJumpForce
        {
            Value = authoring.jumpSpeed
        });
        AddComponent(playerEntity, new PlayerMoveSpeed
        {
            Value = authoring.moveSpeed,
        });
        AddComponent(playerEntity, new PlayerSprintSpeed
        {
            Value = authoring.sprintSpeed,
        });
        AddComponent(playerEntity, new PlayerChecksOffset
        {
            Value = authoring.playerChecksOffset
        });
    }
}
