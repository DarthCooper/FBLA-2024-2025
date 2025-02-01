using Unity.Entities;
using Unity.Entities.Serialization;

public struct SceneLoader : IComponentData
{
    public EntitySceneReference SceneReference;   
}
