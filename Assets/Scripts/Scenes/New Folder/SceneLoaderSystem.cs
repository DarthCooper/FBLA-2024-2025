using Unity.Collections;
using Unity.Entities;
using Unity.Scenes;
using UnityEngine;
using Unity.Entities.Serialization;
using UnityEngine.SceneManagement;
using System;

[RequireMatchingQueriesForUpdate]
public partial class SceneLoaderSystem : SystemBase
{
    public SubScene subScene;

    public Action OnSceneChange;

    protected override void OnCreate()
    {
        RequireForUpdate<SceneLoader>();
    }

    protected override void OnUpdate()
    {
        EntityQuery query = EntityManager.CreateEntityQuery(typeof(SceneLoader));

        // Get the entities matching the query
        NativeArray<SceneLoader> entities = query.ToComponentDataArray<SceneLoader>(Unity.Collections.Allocator.Temp);


        if (entities.Length > 0)
        {
            SceneLoader scene = entities[0];

            var result = SceneSystem.LoadSceneAsync(World.Unmanaged, scene.SceneReference, new SceneSystem.LoadParameters { AutoLoad = true });

            Unity.Entities.Hash128 sceneGUID = subScene.SceneGUID;

            Entity sceneEntity = SceneSystem.GetSceneEntity(World.Unmanaged, sceneGUID);

            SceneSystem.UnloadScene(World.Unmanaged, sceneEntity, SceneSystem.UnloadParameters.DestroyMetaEntities);

            OnSceneChange?.Invoke();

            EntityManager.DestroyEntity(query);
        }

        entities.Dispose();
    }
}