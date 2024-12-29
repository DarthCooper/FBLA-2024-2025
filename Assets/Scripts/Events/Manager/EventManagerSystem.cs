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
            if(spawnEvent.Equals(Entity.Null) || !SystemAPI.Exists(spawnEvent.spawnEntity)) { return; }
            EntityManager.AddComponent<CanSpawn>(spawnEvent.spawnEntity);
            EntityManager.RemoveComponent<SpawnEnemiesEvent>(eventManagerEntity);
        }
    }
}
