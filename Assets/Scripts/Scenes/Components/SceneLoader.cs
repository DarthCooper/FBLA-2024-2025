using Unity.Entities;
using Unity.Entities.Serialization;

public struct SceneLoader : IComponentData
{
    public EntitySceneReference SceneReference;   
}

public struct SceneStats : IComponentData
{
    public int SceneIndex;
    public int SceneBuildIndex;
    public Hash128 SceneGUID;
}

public struct UnloadCurScene : IComponentData { }
