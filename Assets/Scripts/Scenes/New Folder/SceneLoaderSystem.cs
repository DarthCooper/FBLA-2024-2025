using Unity.Collections;
using Unity.Entities;
using Unity.Scenes;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Entities.Serialization;
using UnityEngine.SceneManagement;

[RequireMatchingQueriesForUpdate]
public partial class SceneLoaderSystem : SystemBase
{
    private EntityQuery newRequests;

    protected override void OnUpdate()
    {
        var requests = newRequests.ToComponentDataArray<SceneLoader>(Allocator.Temp);

        // Can't use a foreach with a query as SceneSystem.LoadSceneAsync does structural changes
        for (int i = 0; i < requests.Length; i += 1)
        {
            SceneSystem.LoadSceneAsync(World.Unmanaged, requests[i].SceneReference);
        }

        requests.Dispose();
        EntityManager.DestroyEntity(newRequests);
    }
}
