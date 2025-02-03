using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial class PlayerSyncSystem : SystemBase
{
    protected override void OnUpdate()
    {
        SystemAPI.TryGetSingletonEntity<PlayerTag>(out Entity playerEntity);
        if(playerEntity.Equals(Entity.Null)) { return; }
        LocalToWorld playerTransform = SystemAPI.GetComponent<LocalToWorld>(playerEntity);

        var playerGameObject = PlayerSyncSingleton.Instance;
        if (playerGameObject == null) { return; }

        playerGameObject.transform.position = playerTransform.Position;
        playerGameObject.transform.rotation = playerTransform.Rotation;
        playerGameObject.transform.localScale = playerTransform.Value.Scale();
    }
}
