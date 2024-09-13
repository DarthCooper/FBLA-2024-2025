using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial class PlayerSyncSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
        LocalToWorld playerTransform = SystemAPI.GetComponent<LocalToWorld>(playerEntity);

        var playerGameObject = PlayerSyncSingleton.Instance;
        if (playerGameObject == null) { return; }

        playerGameObject.transform.position = playerTransform.Position;
        playerGameObject.transform.rotation = playerTransform.Rotation;
        playerGameObject.transform.localScale = playerTransform.Value.Scale();
    }
}
