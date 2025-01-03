using System;
using Unity.Entities;
using UnityEngine;

public partial class EventManagerSystem : SystemBase
{
    public Action OnEndLevel;

    protected override void OnUpdate()
    {
        Entity eventManagerEntity = SystemAPI.GetSingletonEntity<EventManger>();
        if(EntityManager.HasComponent<EndLevelEvent>(eventManagerEntity))
        {
            OnEndLevel?.Invoke();
            EntityManager.RemoveComponent<EndLevelEvent>(eventManagerEntity);
        }
        if(EntityManager.HasComponent<SpawnEnemiesEvent>(eventManagerEntity))
        {
            SpawnEnemiesEvent spawnEvent = EntityManager.GetComponentData<SpawnEnemiesEvent>(eventManagerEntity);
            if(spawnEvent.spawnEntity.Equals(Entity.Null) || !SystemAPI.Exists(spawnEvent.spawnEntity)) { return; }
            EntityManager.AddComponent<CanSpawn>(spawnEvent.spawnEntity);
            EntityManager.RemoveComponent<SpawnEnemiesEvent>(eventManagerEntity);
        }
        if(EntityManager.HasComponent<ActivateEntitiesEvent>(eventManagerEntity))
        {
            ActivateEntitiesEvent spawnEvent = EntityManager.GetComponentData<ActivateEntitiesEvent>(eventManagerEntity);
            if (spawnEvent.ActivateEntityHolder.Equals(Entity.Null) || !SystemAPI.Exists(spawnEvent.ActivateEntityHolder)) { return; }
            EntityManager.AddComponent<CanActivate>(spawnEvent.ActivateEntityHolder);
            EntityManager.RemoveComponent<ActivateEntitiesEvent>(eventManagerEntity);
        }
        if(EntityManager.HasComponent<DeActivateEntitiesEvent>(eventManagerEntity))
        {
            DeActivateEntitiesEvent deActivateEvent = EntityManager.GetComponentData<DeActivateEntitiesEvent>(eventManagerEntity);
            if(deActivateEvent.DeActivateEntityHolder.Equals(Entity.Null) || !SystemAPI.Exists(deActivateEvent.DeActivateEntityHolder)) { return; }
            EntityManager.AddComponent<CanDeActivate>(deActivateEvent.DeActivateEntityHolder);
            EntityManager.RemoveComponent<DeActivateEntitiesEvent>(eventManagerEntity);
        }
        if(EntityManager.HasComponent<ShakeCameraEvent>(eventManagerEntity))
        {
            CameraManagers.Instance.Impulse(EntityManager.GetComponentData<ShakeCameraEvent>(eventManagerEntity).index);
            EntityManager.RemoveComponent<ShakeCameraEvent>(eventManagerEntity);
        }
    }
}
